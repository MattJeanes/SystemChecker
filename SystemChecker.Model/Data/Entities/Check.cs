using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblCheck")]
    public class Check
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }

        public string Description { get; set; }

        [Column("TypeID")]
        public CheckType Type { get; set; }

        public int TypeID { get; set; }

        [Column("GroupID")]
        public CheckGroup Group { get; set; }

        public int? GroupID { get; set; }

        public List<CheckSchedule> Schedules { get; set; }

        public CheckData Data { get; set; }

        [Column("EnvironmentID")]
        public Environment Environment { get; set; }

        public int EnvironmentID { get; set; }

        public List<CheckResult> Results { get; set; }

        public List<SubCheck> SubChecks { get; set; }

        public List<CheckNotification> Notifications { get; set; }
    }
}
