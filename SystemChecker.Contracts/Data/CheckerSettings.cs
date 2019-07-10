using System.Collections.Generic;
using SystemChecker.Contracts.DTO;

namespace SystemChecker.Contracts.Data
{
    public class CheckerSettings
    {
        public List<LoginDTO> Logins { get; set; }
        public List<ConnStringDTO> ConnStrings { get; set; }
        public List<EnvironmentDTO> Environments { get; set; }
        public List<ContactDTO> Contacts { get; set; }
        public List<CheckGroupDTO> CheckGroups { get; set; }
        public GlobalSettings Global { get; set; }
    }
}