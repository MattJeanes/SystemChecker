using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("vwLastResultStatus")]
    public class LastResultStatus
    {
        [Key]
        public int CheckID { get; set; }
        public int? StatusID { get; set; }
        public int? TypeID { get; set; }
    }
}
