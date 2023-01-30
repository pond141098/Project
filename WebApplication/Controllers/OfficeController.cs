using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeniorProject.Data;
using SeniorProject.Models;
using SeniorProject.ViewModels.Office;
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
    public class OfficeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;

        public OfficeController(
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
            return View();
        }
        public IActionResult AllListStudent()
        {
            return View("AllListStudent");
        }
        public async Task<IActionResult> getAllListStudent()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetPerson = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var GetFaculty = await DB.MASTER_FACULTY.ToListAsync();
            var Gets = await DB.Users.ToListAsync();
            var Models = new List<AllListStudentRegister>();

            foreach (var j in GetJob)
            {
                foreach (var f in GetFaculty.Where(w => w.faculty_id == j.faculty_id))
                {
                    foreach (var data in GetPerson.Where(w => w.transaction_job_id == j.transaction_job_id))
                    {
                        foreach (var item in GetStatus.Where(w => w.status_id == data.status_id))
                        {
                            if (item.status_id == 7 || item.status_id == 6 || item.status_id == 5)
                            {
                                var Model = new AllListStudentRegister();
                                Model.id = data.transaction_register_id;
                                Model.student_name = data.fullname;
                                Model.student_id = data.s_id;
                                Model.faculty_name = f.faculty_name;
                                Model.status_name = item.status_name;
                                Model.job_name = j.job_name;
                                Models.Add(Model);
                            }
                        }
                    }
                }
            }
            return PartialView("getAllListStudent", Models);
        }

        //ตรวจสอบ
        public IActionResult CheckRegisterAll(int transaction_register_id)
        {
            var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == transaction_register_id).FirstOrDefault();
            return View("CheckRegisterAll", Get);
        }

        //อนุมัติ
        [HttpPost]
        public async Task<IActionResult> Approve(TRANSACTION_REGISTER model)
        {
            try
            {
                model.status_id = 5;
                DB.TRANSACTION_REGISTER.Update(model);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = false, message = "อนุมัติสำเร็จ" });
        }

        //ไม่อนุมัติ
        [HttpPost]
        public async Task<IActionResult> NotApprove(TRANSACTION_REGISTER model)
        {
            try
            {
                model.status_id = 6;
                DB.TRANSACTION_REGISTER.Update(model);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "ไม่อนุมัติสำเร็จ" });
        }
    }
}
