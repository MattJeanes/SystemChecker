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

        public string Value { get; set; }
    }
}
