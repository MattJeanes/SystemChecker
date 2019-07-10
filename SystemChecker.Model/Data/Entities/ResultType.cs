using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblResultType")]
    public class ResultType
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

        public string Identifier { get; set; }
    }
}