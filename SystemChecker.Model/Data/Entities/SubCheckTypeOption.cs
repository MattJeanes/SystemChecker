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
    [Table("tblSubCheckTypeOption")]
    public class SubCheckTypeOption : Option
    {
        [ForeignKey("SubCheckTypeID")]
        public SubCheckType SubCheckType { get; set; }
    }
}
