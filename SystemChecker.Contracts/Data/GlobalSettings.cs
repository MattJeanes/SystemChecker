using System;
using System.Collections.Generic;
using SystemChecker.Contracts.DTO;

namespace SystemChecker.Contracts.Data
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
        public string AuthenticationGroup { get; set; }
        public string SlackToken { get; set; }
        public string Url { get; set; }
        public int? ResultRetentionMonths { get; set; }
        public int? ResultAggregateDays { get; set; }
        public string CleanupSchedule { get; set; }
        public int LoginExpireAfterDays { get; set; }
        public string TimeZoneId { get; set; }
        public string CountryCode { get; set; }
    }
}