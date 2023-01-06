using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace SeniorProject.Models
{
    public class MASTER_FACULTY
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int faculty_id { get; set; }
        public string faculty_name { get; set; }
    }
}