using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblSubCheckType")]
    public class SubCheckType
    {
        [Key]
        public int ID { get; set; }

        [Column("CheckTypeID")]
        public CheckType CheckType { get; set; }

        public int CheckTypeID { get; set; }

        public string Name { get; set; }

        public List<SubCheckTypeOption> Options { get; set; }
    }
}
