using Newtonsoft.Json;
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
    [Table("tblCheckNotification")]
    public class CheckNotification
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("TypeID")]
        public CheckNotificationType Type { get; set; }

        public int TypeID { get; set; }

        [Required]
        [ForeignKey("CheckID")]
        public Check Check { get; set; }

        public int CheckID { get; set; }

        public bool Active { get; set; }

        [NotMapped]
        public dynamic Options { get => JsonConvert.DeserializeObject(OptionsJSON); set => OptionsJSON = JsonConvert.SerializeObject(value); }

        [Column("Options")]
        public string OptionsJSON { get; set; }
    }
}
