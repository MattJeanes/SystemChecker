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
    [Table("tblApiKey")]
    public class ApiKey
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("UserID")]
        public User User { get; set; }

        public int UserID { get; set; }

        public string Name { get; set; }

        public string Key { get; set; }
    }
}
