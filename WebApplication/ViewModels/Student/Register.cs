using Microsoft.AspNetCore.Http;
using System;

namespace SeniorProject.ViewModels.Student
{
    public class Register
    {
        public int transaction_job_id { get; set; }
        public int status_id { get; set; }
        public int banktype_id { get; set; }
        public string bank_no { get; set; }
        public string bank_file { get; set; }
        public string because_job { get; set; }
        public DateTime register_date { get; set; }
        public string s_id { get; set; }
        public string fullname { get; set; }
    }
}
