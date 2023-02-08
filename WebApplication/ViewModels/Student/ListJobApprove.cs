using System;

namespace SeniorProject.ViewModels.Student
{
    public class ListJobApprove
    {
        public int id { get; set; }
        public int j_id { get; set; }
        public string job_name { get; set; }
        public string job_detail { get; set; }
        public string job_status { get; set;}
        public DateTime confirm_approve { get; set; }
    }
}
