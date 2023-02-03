using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _environment;

        public DevstudentController(
             ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext db,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            DB = db;
            _environment = environment;
        }

        //เเดชบอร์ด
        public IActionResult Index()
        {
            return View("Index");
        }

        //ข้อมูลนศ.ที่สมัครงาน
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
                    foreach(var stat in GetStatus.Where(w => w.status_id == item.status_id))
                    {
                        var Model = new ListStudentRegisterFaculty();
                        if(stat.status_id == 9 || stat.status_id == 7)
                        {
                            Model.id = item.transaction_register_id;
                            Model.job_name = data.job_name;
                            Model.student_name = item.fullname;
                            Model.s_id = item.s_id;
                            Model.register_date = item.register_date;
                            Model.status_name = stat.status_name;
                            Models.Add(Model);
                        }
                        
                    }
                }
            }
            return PartialView("getListStudentFaculty",Models);
        }

        //ตรวจสอบ
        public IActionResult CheckRegisterFaculty(int transaction_register_id)
        {
            var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == transaction_register_id).FirstOrDefault();
            return View("CheckRegisterFaculty",Get);
        }

        //อนุมัติ ส่งกองพัฒนานักศึกษา
        [HttpPost]
        public async Task<IActionResult> Approve(TRANSACTION_REGISTER model)
        {
            try
            {
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.status_id == model.status_id).FirstOrDefault();

                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ
                if (Get.status_id == 5 || Get.status_id == 6 || Get.status_id == 7) 
                {
                    return Json(new { valid = false, message = "ไม่สามารถส่งอนุมัติได้ !!!" });
                }

                Get.fullname = model.fullname;
                Get.s_id = model.s_id;
                Get.bank_file = model.bank_file;
                Get.bank_no = model.bank_no;
                Get.bank_store = model.bank_store;
                Get.because_job = model.because_job;
                Get.register_date = model.register_date;
                Get.status_id = 7;
                DB.TRANSACTION_REGISTER.Update(Get);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "ส่งอนุมัติสำเร็จ" });
        }

        //ไม่อนุมัติ
        [HttpPost]
        public async Task<IActionResult> NotApprove(TRANSACTION_REGISTER model)
        {
            try
            {
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.status_id == model.status_id).FirstOrDefault();

                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ 
                if (Get.status_id == 5 || Get.status_id == 6 )
                {
                    return Json(new { valid = false, message = "ไม่สามารถไม่อนุมัติได้ !!!" });
                }

                Get.fullname = model.fullname;
                Get.s_id = model.s_id;
                Get.bank_file = model.bank_file;
                Get.bank_no = model.bank_no;
                Get.bank_store = model.bank_store;
                Get.because_job = model.because_job;
                Get.register_date = model.register_date;
                Get.status_id = 6;
                DB.TRANSACTION_REGISTER.Update(Get);
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
