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
        public string detail_working { get; set; }
        public DateTime start_work { get; set; }
        public DateTime end_work { get; set;}
        public string img_work { get; set; }
    }
}
