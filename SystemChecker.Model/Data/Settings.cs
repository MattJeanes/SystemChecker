using System.Collections.Generic;

namespace SystemChecker.Model.Data
{
    public class Settings
    {
        public List<Login> Logins { get; set; }
        public List<ConnString> ConnStrings { get; set; }
    }
}