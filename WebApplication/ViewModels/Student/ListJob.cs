using System;

namespace SeniorProject.ViewModels.Student
{
    public class ListJob
    {
        public int id { get; set; }
        public string jobname { get; set; }
        public string jobplace { get; set; }
        public string job_detail { get; set; }
        public string job_owner_name { get; set; }
        public DateTime close_register { get; set; }
        public int amount_person { get; set;}
        public int amount_working { get; set;}
        public DateTime create_job { get; set; }
    }
}
