using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblCheckTypeOption")]
    public class CheckTypeOption : Option
    {
        public int CheckTypeID { get; set; }

        [ForeignKey("CheckTypeID")]
        public CheckType CheckType { get; set; }
    }
}
