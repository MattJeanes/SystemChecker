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
    public abstract class Option
    {
        [Key]
        public int ID { get; set; }

        [Column("OptionTypeID")]
        public OptionType OptionType { get; set; }

        public string Label { get; set; }

        public string DefaultValue { get; set; }

        public bool IsRequired { get; set; }
    }
}
