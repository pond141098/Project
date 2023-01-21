using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeniorProject.Data;
using SeniorProject.Models;
using SeniorProject.ViewModels.Teacher;
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
        public async Task<IActionResult>getListStudentFaculty()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJobName = await DB.TRANSACTION_JOB.ToListAsync();
            var GetPerson = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var Models = new List<ListStudentRegister>();

            foreach (var data in GetPerson.Where(w => w.owner_job_id == CurrentUser.Id))
            {
                var ViewModel = new ListStudentRegister();
                if (data.status_id == 7)
                {
                    ViewModel.job_name = GetJobName.Where(w => w.create_by == CurrentUser.Id).Select(s => s.job_name).FirstOrDefault();
                    ViewModel.student_name = GetPerson.Select(s => s.fullname).FirstOrDefault();
                    ViewModel.s_id = GetPerson.Select(s => s.s_id).FirstOrDefault();
                    ViewModel.register_date = GetPerson.Select(s => s.register_date).FirstOrDefault();
                    ViewModel.status_name = "รออนุมัติ";
                }
                else if (data.status_id == 8)
                {
                    ViewModel.job_name = GetJobName.Where(w => w.create_by == CurrentUser.Id).Select(s => s.job_name).FirstOrDefault();
                    ViewModel.student_name = GetPerson.Select(s => s.fullname).FirstOrDefault();
                    ViewModel.s_id = GetPerson.Select(s => s.s_id).FirstOrDefault();
                    ViewModel.register_date = GetPerson.Select(s => s.register_date).FirstOrDefault();
                    ViewModel.status_name = "รอส่งอนุมัติ";
                }
                else if (data.status_id == 6)
                {
                    ViewModel.job_name = GetJobName.Where(w => w.create_by == CurrentUser.Id).Select(s => s.job_name).FirstOrDefault();
                    ViewModel.student_name = GetPerson.Select(s => s.fullname).FirstOrDefault();
                    ViewModel.s_id = GetPerson.Select(s => s.s_id).FirstOrDefault();
                    ViewModel.register_date = GetPerson.Select(s => s.register_date).FirstOrDefault();
                    ViewModel.status_name = "ไม่อนุมัติ";
                }
                else if (data.status_id == 5)
                {
                    ViewModel.job_name = GetJobName.Where(w => w.create_by == CurrentUser.Id).Select(s => s.job_name).FirstOrDefault();
                    ViewModel.student_name = GetPerson.Select(s => s.fullname).FirstOrDefault();
                    ViewModel.s_id = GetPerson.Select(s => s.s_id).FirstOrDefault();
                    ViewModel.register_date = GetPerson.Select(s => s.register_date).FirstOrDefault();
                    ViewModel.status_name = "อนุมัติ";
                }
                Models.Add(ViewModel);

            }
            return PartialView("getListStudentFaculty",Models);
        }
    }
}
