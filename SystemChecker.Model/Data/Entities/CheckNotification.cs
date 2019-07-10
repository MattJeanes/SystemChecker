using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public T GetOptions<T>()
        {
            return JsonConvert.DeserializeObject<T>(OptionsJSON);
        }

        [Column("Options")]
        public string OptionsJSON { get; set; }

        public DateTimeOffset? Sent { get; set; }

        public int? FailCount { get; set; }

        public int? FailMinutes { get; set; }
    }
}
