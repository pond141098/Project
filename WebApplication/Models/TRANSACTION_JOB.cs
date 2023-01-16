using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SeniorProject.Models
{
    public class TRANSACTION_JOB
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int transaction_job_id { get; set; }
        public string job_name { get; set; }
        public string job_detail { get; set; }
        public int amount_person { get; set; }
        public int amount_date { get; set; }
        public DateTime close_register_date { get; set; }
        public string create_by { get; set; } 
        public DateTime update_date { get; set; }
        public int faculty_id { get; set; } 
        public int branch_id { get; set; }  
        public string owner_job { get; set; }   

    }
}
