using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Helpers
{
    public interface IEncryptionHelper
    {
        string Encrypt(string text);
        string Decrypt(string text);
    }
    public class EncryptionHelper : IEncryptionHelper
    {
        private readonly AppSettings _appSettings;
        public EncryptionHelper(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public string Encrypt(string text)
        {
            return Encrypt(text, _appSettings.EncryptionKey, _appSettings.EncryptionKeySize);
        }

        public string Decrypt(string text)
        {
            return Decrypt(text, _appSettings.EncryptionKey, _appSettings.EncryptionKeySize);
        }

        private string GenerateKey(int iKeySize)
        {
            RijndaelManaged aesEncryption = new RijndaelManaged
            {
                KeySize = iKeySize,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
            aesEncryption.GenerateIV();
            string ivStr = Convert.ToBase64String(aesEncryption.IV);
            aesEncryption.GenerateKey();
            string keyStr = Convert.ToBase64String(aesEncryption.Key);
            string completeKey = ivStr + "," + keyStr;

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(completeKey));
        }

        /// <summary>
        /// Encrypt
        /// From : www.chapleau.info/blog/2011/01/06/usingsimplestringkeywithaes256encryptioninc.html
        /// </summary>
        private string Encrypt(string iPlainStr, string iCompleteEncodedKey, int iKeySize)
        {
            RijndaelManaged aesEncryption = new RijndaelManaged
            {
                KeySize = iKeySize,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = Convert.FromBase64String(Encoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey)).Split(',')[0]),
                Key = Convert.FromBase64String(Encoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey)).Split(',')[1])
            };
            byte[] plainText = Encoding.UTF8.GetBytes(iPlainStr);
            ICryptoTransform crypto = aesEncryption.CreateEncryptor();
            byte[] cipherText = crypto.TransformFinalBlock(plainText, 0, plainText.Length);
            return Convert.ToBase64String(cipherText);
        }

        /// <summary>
        /// Decrypt
        /// From : www.chapleau.info/blog/2011/01/06/usingsimplestringkeywithaes256encryptioninc.html
        /// </summary>
        private string Decrypt(string iEncryptedText, string iCompleteEncodedKey, int iKeySize)
        {
            RijndaelManaged aesEncryption = new RijndaelManaged
            {
                KeySize = iKeySize,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = Convert.FromBase64String(Encoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey)).Split(',')[0]),
                Key = Convert.FromBase64String(Encoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey)).Split(',')[1])
            };
            ICryptoTransform decrypto = aesEncryption.CreateDecryptor();
            byte[] encryptedBytes = Convert.FromBase64CharArray(iEncryptedText.ToCharArray(), 0, iEncryptedText.Length);
            return Encoding.UTF8.GetString(decrypto.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length));
        }
    }
}
