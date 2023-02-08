using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SeniorProject.Models
{
    public class MASTER_STATUS_WORKING
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int status_working_id { get; set; }
        public string status_working_name { get; set;}
    }
}
