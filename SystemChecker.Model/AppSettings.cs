using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model
{
    public class AppSettings
    {
        public string EncryptionKey { get; set; }
        public int EncryptionKeySize { get; set; }
        public int ResultRetentionMonths { get; set; }
        public int ResultAggregateDays { get; set; }
        public string CleanupSchedule { get; set; }
    }
}
