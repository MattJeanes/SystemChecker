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
    [Table("tblCheckData")]
    public class CheckData
    {
        [Key]
        public int ID { get; set; }

        [NotMapped]
        public dynamic TypeOptions { get => JsonConvert.DeserializeObject(TypeOptionsJSON); set => TypeOptionsJSON = JsonConvert.SerializeObject(value); }

        public T GetTypeOptions<T>() => JsonConvert.DeserializeObject<T>(TypeOptionsJSON);

        [Column("TypeOptions")]
        public string TypeOptionsJSON { get; set; }
    }
}
