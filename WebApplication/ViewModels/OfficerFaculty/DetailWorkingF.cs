using System;

namespace SeniorProject.ViewModels.OfficerFaculty
{
    public class DetailWorkingF
    {
        public int transaction_working_id { get; set; }
        public string date { get; set; }
        public string check_in { get; set; }
        public string check_out { get; set; }
        public string file_in { get; set; }
        public string file_out { get; set; }
        public decimal longitude_in { get; set; }
        public decimal laitude_in { get; set; }
        public decimal longitude_out { get; set; }
        public decimal laitude_out { get; set; }
        public string status { get; set; }
    }
}
