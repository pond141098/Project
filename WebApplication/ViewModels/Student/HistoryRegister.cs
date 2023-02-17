using System;

namespace SeniorProject.ViewModels.Student
{
    public class HistoryRegister
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string place { get; set; }
        public string detail { get; set; }
        public string status { get; set; }
        public int status_id { get; set; }
        public DateTime register_date { get; set; }
        public string file { get; set; }    
    }
}
