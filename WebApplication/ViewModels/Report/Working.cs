namespace SeniorProject.ViewModels.Report
{
    public class Working
    {
        public int Id { get; set; }
        public string faculty { get; set; } //หน่วยงาน
        public string student_name { get; set; }
        public string month { get; set; }
        public string year { get; set; }
        public string dmy { get; set; } //วันเดือนปี
        public string detail_working { get; set; }
        public string sign_name_start { get; set; }
        public string time_in { get; set; }
        public string sign_name_end { get; set; }
        public string time_out { get; set; }
        public int hours_work { get; set; }
        public int half_day_work { get; set; }
        public int full_day_work { get; set; }
        public string sign_owner_job { get; set; }
    }
}
