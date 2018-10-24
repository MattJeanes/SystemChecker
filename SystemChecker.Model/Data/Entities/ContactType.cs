using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblContactType")]
    public class ContactType
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
    }
}
