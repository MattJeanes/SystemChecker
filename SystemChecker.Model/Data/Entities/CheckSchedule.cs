﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblCheckSchedule")]
    public class CheckSchedule
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("CheckID")]
        public Check Check { get; set; }

        public string Expression { get; set; }

        public bool Active { get; set; }

        public bool SkipPublicHolidays { get; set; }
    }
}
