using System;

namespace SeniorProject.ViewModels.Report
{
    public class Payment
    {
        public int Id { get; set; }
        public string Faculty { get; set; }
        public string Province { get; set; }
        public DateTime DMY { get; set; }
        public int Row { get; set; }
        public string FullName { get; set; }
        public string date_work { get; set; }
        public int hour_work { get; set; }
        public int half_day_work { get; set; }
        public int full_day_work { get; set; }
        
    }
}
