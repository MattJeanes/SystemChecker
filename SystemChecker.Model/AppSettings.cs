using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model
{
    public class AppSettings
    {
        public int ResultRetentionMonths { get; set; }
        public int ResultAggregateDays { get; set; }
        public string CleanupSchedule { get; set; }
        public string Url { get; set; }
    }
}
