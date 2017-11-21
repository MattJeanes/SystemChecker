using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Enums;

namespace SystemChecker.Model.Data
{
    [Table("tblCheckResult")]
    public class CheckResult
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("CheckID")]
        public Check Check { get; set; }

        public CheckResultStatus Status { get; set; }

        public int TimeMS { get; set; }
    }
}