using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblCheckNotificationTypeOption")]
    public class CheckNotificationTypeOption : Option
    {
        [ForeignKey("CheckNotificationTypeID")]
        public CheckNotificationType CheckNotificationType { get; set; }
    }
}
