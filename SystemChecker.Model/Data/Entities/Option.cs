using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    public abstract class Option
    {
        public string ID { get; set; }

        [Column("OptionTypeID")]
        public int OptionType { get; set; }

        public string Label { get; set; }

        public string DefaultValue { get; set; }

        public bool IsRequired { get; set; }

        public bool Multiple { get; set; }
    }
}
