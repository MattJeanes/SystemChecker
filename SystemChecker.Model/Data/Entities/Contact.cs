using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Enums;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblContact")]
    public class Contact
    {
        [Key]
        public int ID { get; set; }

        [Column("TypeID")]
        public ContactType Type { get; set; }

        public int TypeID { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
