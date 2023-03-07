using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SeniorProject.Models
{
    public class MASTER_TIMEWORKING
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int time_working_id { get; set; }
        public string time_working_name { get; set; }
    }
}
