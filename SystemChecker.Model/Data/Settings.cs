using System.Collections.Generic;
using SystemChecker.Model.DTO;

namespace SystemChecker.Model.Data
{
    public interface ISettings
    {
        List<LoginDTO> Logins { get; set; }
        List<ConnStringDTO> ConnStrings { get; set; }
        List<EnvironmentDTO> Environments { get; set; }
    }
    public class Settings : ISettings
    {
        public List<LoginDTO> Logins { get; set; }
        public List<ConnStringDTO> ConnStrings { get; set; }
        public List<EnvironmentDTO> Environments { get; set; }
    }
}