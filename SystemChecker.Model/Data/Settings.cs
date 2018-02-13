using System.Collections.Generic;
using SystemChecker.Model.DTO;

namespace SystemChecker.Model.Data
{
    public interface ISettings
    {
        List<LoginDTO> Logins { get; set; }
        List<ConnStringDTO> ConnStrings { get; set; }
        List<EnvironmentDTO> Environments { get; set; }
        List<ContactDTO> Contacts { get; set; }
        List<CheckGroupDTO> CheckGroups { get; set; }
        GlobalSettings Global { get; set; }
    }
    public class Settings : ISettings
    {
        public List<LoginDTO> Logins { get; set; }
        public List<ConnStringDTO> ConnStrings { get; set; }
        public List<EnvironmentDTO> Environments { get; set; }
        public List<ContactDTO> Contacts { get; set; }
        public List<CheckGroupDTO> CheckGroups { get; set; }
        public GlobalSettings Global { get; set; }
    }
}