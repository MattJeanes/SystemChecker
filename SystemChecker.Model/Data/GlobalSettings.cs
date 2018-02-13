using System.Collections.Generic;
using SystemChecker.Model.DTO;

namespace SystemChecker.Model.Data
{
    public class EmailSettings
    {
        public string From { get; set; }
        public string Server { get; set; }
        public int? Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool TLS { get; set; }
    }
    public class ClickatellSettings
    {
        public string ApiKey { get; set; }
        public string ApiUrl { get; set; }
        public string From { get; set; }
    }
    public class GlobalSettings
    {
        public EmailSettings Email { get; set; }
        public ClickatellSettings Clickatell { get; set; }
        public dynamic AuthenticationGroup { get; set; }
        public dynamic SlackToken { get; set; }
    }
}