using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeniorProject.Data;
using SeniorProject.Models;
using SeniorProject.ViewModels.Devstudent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Controllers;
using WebApplication.Data;
using WebApplication.Models;

namespace SeniorProject.Controllers
{
    public class DevstudentController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;

        public DevstudentController(
             ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext db)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            DB = db;
        }
        public IActionResult Index()
        {
            return View("Index");
        }
        public IActionResult ListStudentFaculty()
        {
            return View("ListStudentFaculty");
        }
        public async Task<IActionResult> getListStudentFaculty()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetPerson = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var GetFaculty = await DB.MASTER_FACULTY.ToListAsync();
            var Models = new List<ListStudentRegisterFaculty>();

            foreach (var data in GetJob.Where(w => w.faculty_id == CurrentUser.faculty_id))
            {
                foreach (var item in GetPerson.Where(w => w.transaction_job_id == data.transaction_job_id)) 
                {
                    var Model = new ListStudentRegisterFaculty();
                    Model.job_faculty_id = CurrentUser.faculty_id;
                    Model.job_name = data.job_name;
                    Model.student_name = GetPerson.Where(w => w.transaction_job_id == data.transaction_job_id).Select(s => s.fullname).FirstOrDefault();
                    Model.s_id = GetPerson.Where(w => w.transaction_job_id == data.transaction_job_id).Select(s => s.s_id).FirstOrDefault();
                    //มันไปเอาวันที่สร้างงานมาเเสดง
                    Model.register_date = GetPerson.Where(w => w.register_date != data.update_date).Select(s => s.register_date).FirstOrDefault();
                    Models.Add(Model);
                }
            }

            return PartialView("getListStudentFaculty",Models);
        }
    }
}
