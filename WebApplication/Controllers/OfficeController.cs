using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
using System.IO;
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
        private readonly IWebHostEnvironment _environment;

        public OfficeController(
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

        //แดชบอร์ด
        public IActionResult Index()
        {
            return View();
        }

        //ข้อมมูลนศ.ที่สมัครงาน
        public IActionResult AllListStudent()
        {
            return View("AllListStudent");
        }
        //public async Task<IActionResult> getAllListStudent()
        //{
        //    var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
        //    var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
        //    var GetPerson = await DB.TRANSACTION_REGISTER.ToListAsync();
        //    var GetStatus = await DB.MASTER_STATUS.ToListAsync();
        //    var GetFaculty = await DB.MASTER_FACULTY.ToListAsync();
        //    var Gets = await DB.Users.ToListAsync();
        //    var Models = new List<AllListStudentRegister>();

        //    foreach (var j in GetJob)
        //    {
        //        foreach (var f in GetFaculty.Where(w => w.faculty_id == j.faculty_id))
        //        {
        //            foreach (var data in GetPerson.Where(w => w.transaction_job_id == j.transaction_job_id))
        //            {
        //                foreach (var item in GetStatus.Where(w => w.status_id == data.status_id))
        //                {
        //                    if (item.status_id == 7 || item.status_id == 6 || item.status_id == 5)
        //                    {
        //                        var Model = new AllListStudentRegister();
        //                        Model.id = data.transaction_register_id;
        //                        Model.student_name = data.fullname;
        //                        Model.student_id = data.s_id;
        //                        Model.faculty_name = f.faculty_name;
        //                        Model.status_name = item.status_name;
        //                        Model.job_name = j.job_name;
        //                        Models.Add(Model);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return PartialView("getAllListStudent", Models);
        //}

        //ดาวน์โหลดไฟล์
        public FileResult Download(string Name)
        {
            string path = Path.Combine(_environment.WebRootPath, "uploads/bookbank/") + Name;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", Name);
        }

        //ตรวจสอบ
        public IActionResult CheckRegisterAll(int transaction_register_id)
        {
            var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == transaction_register_id).FirstOrDefault();
            var GetBank = DB.MASTER_BANK.ToList();

            ViewBag.bank = GetBank.Where(w => w.banktype_id == Get.banktype_id).Select(s => s.banktype_name).FirstOrDefault();
            return View("CheckRegisterAll", Get);
        }

        //อนุมัติ
        [HttpPost]
        public async Task<IActionResult> Approve(TRANSACTION_REGISTER model)
        {
            try
            {
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.status_id == model.status_id).FirstOrDefault();
                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ
                if (Get.status_id == 5 || Get.status_id == 6 || Get.status_id == 7)
                {
                    return Json(new { valid = false, message = "ไม่สามารถอนุมัติได้ !!!" });
                }

                Get.fullname = model.fullname;
                Get.s_id = model.s_id;
                Get.bank_file = model.bank_file;
                Get.bank_no = model.bank_no;
                Get.bank_store = model.bank_store;
                Get.because_job = model.because_job;
                Get.register_date = model.register_date;
                Get.status_id = 5;
                DB.TRANSACTION_REGISTER.Update(Get);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "อนุมัติสำเร็จ" });
        }

        //ไม่อนุมัติ
        [HttpPost]
        public async Task<IActionResult> NotApprove(TRANSACTION_REGISTER model)
        {
            try
            {
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.status_id == model.status_id).FirstOrDefault();
                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ
                if (Get.status_id == 5 || Get.status_id == 6 || Get.status_id == 7)
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
