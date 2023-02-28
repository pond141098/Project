using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SeniorProject.Models
{
    public class TRANSACTION_WORKING
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int transaction_working_id { get; set; }
        public int transaction_register_id { get; set; }
        public int transaction_job_id { get; set; }
        public string detail_working { get; set; }
        public DateTime start_work { get; set; }
        public DateTime end_work { get; set;}
        public string file_work_end { get; set; }
        public string file_work_start { get; set; }
        public int status_working_id { get; set; }
        public int status_id { get; set; }
        public decimal longitude_start { get; set; }
        public decimal latitude_start { get; set;}
        public decimal longitude_end { get; set; }
        public decimal latitude_end { get; set;}
    }
}
