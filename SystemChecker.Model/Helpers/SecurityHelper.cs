using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;

namespace SystemChecker.Model.Helpers
{
    public interface ISecurityHelper
    {
        Task<User> CreateUser(string username, string password);
        Task<LoginResult> Login(LoginRequest request, HttpContext context = null);
        Task<SymmetricSecurityKey> GetSecurityKey();
        Task<JwtSecurityToken> ValidateToken(string token);
        Task<string> GetToken(string username);
        Task<string> GetToken(User user);
        string GetUsername(JwtSecurityToken token);
    }
    public class SecurityHelper : ISecurityHelper
    {
        private readonly IUserRepository _users;
        private readonly IRepository<GlobalSetting> _globalSettings;
        private readonly ISettingsHelper _settingsHelper;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        public SecurityHelper(IUserRepository users, IRepository<GlobalSetting> globalSettings, ISettingsHelper settingsHelper)
        {
            _users = users;
            _globalSettings = globalSettings;
            _settingsHelper = settingsHelper;
        }

        public async Task<User> CreateUser(string username, string password)
        {
            var user = new User
            {
                Username = username,
                Password = SecurePasswordHasher.Hash(password)
            };
            _users.Add(user);
            await _users.SaveChangesAsync();
            return user;
        }

        public async Task<LoginResult> Login(LoginRequest request, HttpContext context = null)
        {
            var result = new LoginResult
            {
                Success = false,
                Error = "OK"
            };

            User user = null;
            if (request.Username == null) // auto-login attempt
            {
                if (context == null)
                {
                    result.Error = "Username not supplied";
                }
                else
                {
                    user = await AutoLogin(result, context);
                    if (user == null)
                    {
                        var anyUsers = await _users.GetAll().AnyAsync(x => !x.IsWindowsUser);
                        if (!anyUsers)
                        {
                            result.InitRequired = true;
                        }
                    }
                }
            }
            else
            {
                user = await _users.GetByUsername(request.Username, false);
                if (user == null)
                {
                    result.Error = "User not found";
                }
                else if (!SecurePasswordHasher.Verify(request.Password, user.Password))
                {
                    result.Error = "Password incorrect";
                }
                else
                {
                    result.Success = true;
                }
            }



            if (result.Success && user != null)
            {
                result.Token = await GetToken(user);
            }

            return result;
        }

        public async Task<SymmetricSecurityKey> GetSecurityKey()
        {
            var securityKey = (await _globalSettings.Find("SecurityKey")) ?? new GlobalSetting { Key = "SecurityKey" };
            if (securityKey.Value == null)
            {
                _globalSettings.Add(securityKey);
                var hmac = new HMACSHA256();
                var key = Convert.ToBase64String(hmac.Key);
                securityKey.Value = key;
                await _globalSettings.SaveChangesAsync();
            }
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey.Value));
        }

        public async Task<JwtSecurityToken> ValidateToken(string token)
        {
            var securityKey = await GetSecurityKey();
            _tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey
            }, out var validatedToken);

            return (JwtSecurityToken)validatedToken;
        }

        public string GetUsername(JwtSecurityToken token)
        {
            var username = token.Claims.FirstOrDefault(x => x.Type == "username")?.Value;
            if (username == null)
            {
                throw new Exception("Username not found in token");
            }
            return username;
        }

        public async Task<string> GetToken(string username)
        {
            var user = await _users.GetByUsername(username);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return await GetToken(user);
        }

        public async Task<string> GetToken(User user)
        {
            var claims = new[]
            {
                new Claim("username", user.Username),
                new Claim("isWindowsUser", user.IsWindowsUser.ToString())
            };

            var key = await GetSecurityKey();
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var global = await _settingsHelper.GetGlobal();
            var token = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddDays(global.LoginExpireAfterDays),
                signingCredentials: creds);

            return _tokenHandler.WriteToken(token);
        }

        private async Task<User> AutoLogin(LoginResult result, HttpContext context)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result.Error = "Automatic login is only supported on Windows";
                return null;
            }
            if (!(context.User.Identity is WindowsIdentity) || ((WindowsIdentity)context.User.Identity).IsAnonymous)
            {
                throw new UnauthorizedAccessException("Unauthorized"); // triggers NTLM auth in browser
            };
            var winUser = (WindowsIdentity)context.User.Identity;
            var authenticationGroup = (await _settingsHelper.Get()).Global.AuthenticationGroup;
            if (string.IsNullOrEmpty(authenticationGroup))
            {
                result.Error = "Authentication group not set up";
                return null;
            }
            var groupNames = winUser.Groups.Select(x => x.Translate(typeof(NTAccount)).Value);
            if (!groupNames.Any(x => x.Substring(x.IndexOf(@"\") + 1) == authenticationGroup))
            {
                result.Error = $"You are not part of the '{authenticationGroup}' group";
                return null;
            }

            var username = Regex.Replace(winUser.Name, ".*\\\\(.*)", "$1", RegexOptions.None);

            var user = await _users.GetAll().FirstOrDefaultAsync(x => x.Username == username);
            if (user == null)
            {
                user = new User
                {
                    Username = username,
                    IsWindowsUser = true
                };
                _users.Add(user);
                await _users.SaveChangesAsync();
            }
            else if (!user.IsWindowsUser)
            {
                result.Error = $"User {user.Username} exists but is not a windows user";
                return null;
            }

            result.Success = true;
            return user;
        }
    }
}