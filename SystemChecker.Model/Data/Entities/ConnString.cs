﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Entities
{
    [Table("tblCheckConnString")]
    public class ConnString
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
        
        [Column("EnvironmentID")]
        public Environment Environment { get; set; }

        public int EnvironmentID { get; set; }

        public string Value { get; set; }
    }
}
