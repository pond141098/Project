﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeniorProject.ViewModels.Teacher
{
    public class ListStudentRegister
    {
        public string student_name { get; set; }
        public string s_id { get; set; }
        public string job_name{ get; set; }
        public DateTime register_date { get; set; } 
        public string status_name { get; set; }
    }
}