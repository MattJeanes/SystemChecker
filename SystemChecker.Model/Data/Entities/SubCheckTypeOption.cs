using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblSubCheckTypeOption")]
    public class SubCheckTypeOption : Option
    {
        public int SubCheckTypeID { get; set; }

        [ForeignKey("SubCheckTypeID")]
        public SubCheckType SubCheckType { get; set; }
    }
}
