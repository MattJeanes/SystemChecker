using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblCheckNotificationTypeOption")]
    public class CheckNotificationTypeOption : Option
    {
        public int CheckNotificationTypeID { get; set; }

        [ForeignKey("CheckNotificationTypeID")]
        public CheckNotificationType CheckNotificationType { get; set; }
    }
}
