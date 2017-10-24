using System.Collections.Generic;
using SystemChecker.Model.DTO;

namespace SystemChecker.Model.Data
{
    public class Settings
    {
        public List<LoginDTO> Logins { get; set; }
        public List<ConnStringDTO> ConnStrings { get; set; }
    }
}