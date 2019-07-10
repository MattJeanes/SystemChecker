using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblUser")]
    public class User
    {
        [Key]
        public int ID { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool IsWindowsUser { get; set; }

        public List<ApiKey> ApiKeys { get; set; }
    }
}
