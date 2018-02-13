using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblGlobalSettings")]
    public class GlobalSetting
    {
        [Key]
        public string Key { get; set; }

        [NotMapped]
        public dynamic Value { get => JsonConvert.DeserializeObject(ValueJSON); set => ValueJSON = JsonConvert.SerializeObject(value); }

        [Column("Value")]
        public string ValueJSON { get; set; }
    }
}
