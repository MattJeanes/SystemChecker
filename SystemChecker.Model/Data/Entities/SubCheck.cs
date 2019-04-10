using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblSubCheck")]
    public class SubCheck
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("TypeID")]
        public SubCheckType Type { get; set; }

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
    }
}
