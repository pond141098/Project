using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SeniorProject.Data;
using SeniorProject.Models;
using SeniorProject.ViewModels.Devstudent;
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
        public async Task<IActionResult> Index()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var U = await DB.Users.Where(w => w.faculty_id == CurrentUser.faculty_id && w.role_id == 1).CountAsync();
            var B = await DB.MASTER_BRANCH.Where(w => w.faculty_id == CurrentUser.faculty_id ).Select(s => s.branch_id).CountAsync();

            ViewBag.User = U;
            ViewBag.Branch = B;

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
            var GetBranch = await DB.MASTER_BRANCH.ToListAsync();
            var GetPrefix = await DB.MASTER_PREFIX.ToListAsync();
            var Models = new List<ListStudentRegisterFaculty>();

            foreach(var j in GetJob.Where(w => w.faculty_id == CurrentUser.faculty_id))
            {
                foreach(var r in GetPerson.Where(w => w.transaction_job_id == j.transaction_job_id))
                {
                    foreach(var b in GetBranch.Where(w => w.branch_id == j.branch_id))
                    {
                        foreach(var s in GetStatus.Where(w => w.status_id == r.status_id))
                        {
                            var Model = new ListStudentRegisterFaculty();
                            if (s.status_id == 9 || s.status_id == 7 || s.status_id == 6 || s.status_id == 8 || s.status_id == 5)
                            {
                                Model.id = r.transaction_register_id;
                                Model.job_name = j.job_name;
                                Model.branch_name = b.branch_name;
                                Model.student_name = r.fullname;
                                Model.s_id = r.student_id;
                                Model.register_date = r.register_date;
                                Model.status_name = s.status_name;
                                Models.Add(Model);
                            }
                        }
                    }
                }
            }
            return PartialView("getListStudentFaculty", Models);
        }

        //ดาวน์โหลดไฟล์
        public FileResult Download(string Name)
        {
            string path = Path.Combine(_environment.WebRootPath, "uploads/bookbank/") + Name;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", Name);
        }

        //ตรวจสอบ
        public IActionResult CheckRegisterFaculty(int transaction_register_id)
        {
            var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == transaction_register_id).FirstOrDefault();
            var GetBank = DB.MASTER_BANK.ToList();

            ViewBag.bank = GetBank.Where(w => w.banktype_id == Get.banktype_id).Select(s => s.banktype_name).FirstOrDefault();
            return View("CheckRegisterFaculty", Get);
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
                Get.student_id = model.student_id;
                Get.bank_file = model.bank_file;
                Get.bank_no = model.bank_no;
                Get.bank_store = model.bank_store;
                Get.because_job = model.because_job;
                Get.register_date = model.register_date;
                Get.status_id = 7;
                Get.approve_devstudent_date = DateTime.Now;
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
                if (Get.status_id == 5 || Get.status_id == 6)
                {
                    return Json(new { valid = false, message = "ไม่สามารถไม่อนุมัติได้ !!!" });
                }

                Get.fullname = model.fullname;
                Get.student_id = model.student_id;
                Get.bank_file = model.bank_file;
                Get.bank_no = model.bank_no;
                Get.bank_store = model.bank_store;
                Get.because_job = model.because_job;
                Get.register_date = model.register_date;
                Get.status_id = 6;
                Get.notapprove_date = DateTime.Now;
                DB.TRANSACTION_REGISTER.Update(Get);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "ไม่อนุมัติสำเร็จ" });
        }

        //อนุมัติทั้งหมด
        public async Task<IActionResult> AllApprove()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "อนุมัติเรียบร้อย" });
        }
    }
}
