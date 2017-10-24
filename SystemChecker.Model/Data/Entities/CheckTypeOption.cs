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
    [Table("tblCheckTypeOption")]
    public class CheckTypeOption
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("CheckTypeID")]
        public CheckType CheckType { get; set; }

        [Column("OptionTypeID")]
        public CheckTypeOptionType OptionType { get; set; }

        public string Label { get; set; }

        public string DefaultValue { get; set; }

        public bool IsRequired { get; set; }
    }
}
