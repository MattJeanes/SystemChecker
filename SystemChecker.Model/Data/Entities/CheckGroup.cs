using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblCheckGroup")]
    public class CheckGroup
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
    }
}
