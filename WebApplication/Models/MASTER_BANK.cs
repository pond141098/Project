using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace SeniorProject.Models
{
    public class MASTER_BANK
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int banktype_id { get; set; }
        public string banktype_name { get; set; }
    }
}
