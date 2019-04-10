using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblResultStatus")]
    public class ResultStatus
    {
        [Key]
        public int ID { get; set; }

        public int TypeID { get; set; }

        [ForeignKey(nameof(TypeID))]
        public ResultType Type { get; set; }

        public string Name { get; set; }

        public string Identifier { get; set; }
    }
}