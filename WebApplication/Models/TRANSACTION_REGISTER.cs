using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SeniorProject.Models
{
    public class TRANSACTION_REGISTER
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int transaction_register_id { get; set; } 
        public int transaction_job_id { get; set; } 
        public int status_id { get; set; } 
        public int banktype_id { get; set; } 
        public string bank_no { get; set; } 
        public string bank_file { get; set; } 
        public string because_job { get; set; } 
        public DateTime register_date { get; set; } 
        public string s_id { get; set; } 
        public string fullname { get; set; } 
        public string owner_job_id { get; set; } 
    }
}
