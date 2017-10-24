using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SystemChecker.Model.Data;
using SystemChecker.Model.Enums;

namespace SystemChecker.Web.Controllers
{
    [Route("api")]
    public class APIController : Controller
    {
        [HttpGet]
        public List<Check> GetAll()
        {
            var checks = new List<Check>
            {
                new Check
                {
                    ID = 1,
                    Active = true,
                    TypeID = 1,
                    Name = "Test check"
                }
            };
            return checks;
        }

        [HttpGet("types")]
        public List<CheckType> GetTypes()
        {
            return new List<CheckType>
            {
                new CheckType{
                    ID = 1,
                    Name = "Test",
                    Options = new List<CheckTypeOption>
                    {
                        new CheckTypeOption
                        {
                            ID = 1,
                            Label = "Test option",
                            DefaultValue = "test default",
                            IsRequired = true,
                            OptionType = CheckTypeOptionType.String
                        },
                        new CheckTypeOption
                        {
                            ID = 2,
                            Label = "Login test",
                            IsRequired = false,
                            OptionType = CheckTypeOptionType.Login
                        },
                        new CheckTypeOption
                        {
                            ID = 3,
                            Label = "ConnString test",
                            IsRequired = false,
                            OptionType = CheckTypeOptionType.ConnString
                        }
                    }
                }
            };
        }

        [HttpGet("settings")]
        public Settings GetSettings()
        {
            return new Settings {
                Logins = new List<Login>
                {
                    new Login
                    {
                        ID = 1,
                        Username = "test user",
                        Password = "test pass"
                    }
                },
                ConnStrings = new List<ConnString>
                {
                    new ConnString
                    {
                        ID = 1,
                        Name = "Test",
                        Value = "testconnstring"
                    }
                }
            };
        }

        [HttpGet("{id:int}")]
        public CheckDetail GetDetails(int id)
        {
            return new CheckDetail
            {
                ID = id,
                Name = $"test check {id}",
                Data = new CheckData
                {
                    TypeOptions = new
                    {
                        Stuff = true
                    }
                },
                Active = true,
                Schedules = new List<CheckSchedule>
                {
                    new CheckSchedule
                    {
                        ID = 1,
                        Active = true,
                        Expression = "*/30 * * * * *"
                    }
                },
                TypeID = 1
            };
        }

        [HttpPost]
        public CheckDetail Edit([FromBody]CheckDetail value)
        {
            return value;
        }

        [HttpPost("settings")]
        public Settings SetSettings([FromBody]Settings value)
        {
            return value;
        }

        [HttpPost("run/{id:int}")]
        public bool StartRun(int id)
        {
            return true;
        }

        [HttpDelete("{id:int}")]
        public bool Delete(int id)
        {
            return true;
        }
    }
}
