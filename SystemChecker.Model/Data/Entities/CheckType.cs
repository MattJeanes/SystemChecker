using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblCheckType")]
    public class CheckType
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

        public List<CheckTypeOption> Options { get; set; }
    }
}
