using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblCheckNotificationType")]
    public class CheckNotificationType
    {
        [Key]
        public int ID { get; set; }

        [Column("ID")]
        public Enums.CheckNotificationType Type { get; set; }

        public string Name { get; set; }

        public List<CheckNotificationTypeOption> Options { get; set; }
    }
}
