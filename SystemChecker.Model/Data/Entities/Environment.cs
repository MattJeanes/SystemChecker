using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblEnvironment")]
    public class Environment
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
    }
}
