using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SeniorProject.Data;
using SeniorProject.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebApplication.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SeniorProject.ViewModels.Student;
using System.Collections.Generic;

namespace SeniorProject.Controllers
{

    public class StudentController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;
        private readonly IWebHostEnvironment _environment;

        public StudentController(
            //ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext db,
            IWebHostEnvironment environment)
        {
            //_logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            DB = db;
            _environment = environment;
        }
        #region หน้าหลัก
        public IActionResult Index()
        {
            return View("Index");
        }
        public async Task<IActionResult> Home()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Model = DB.TRANSACTION_JOB.ToList();
            var GetName = DB.Users.Where(w => w.Id == CurrentUser.Id).Select(s => s.FirstName).FirstOrDefault();
            var GetLastName = DB.Users.Where(w => w.Id == CurrentUser.Id).Select(s => s.LastName).FirstOrDefault();

            ViewBag.Name = GetName;
            ViewBag.LastName = GetLastName;

            return View("Home");
        }
        #endregion

        #region ประวัติส่วนตัว
        public async Task<IActionResult> Profile()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Model = DB.Users.Where(w => w.Id == CurrentUser.Id).FirstOrDefault();
            var GetFaculty = DB.MASTER_FACULTY.Where(w => w.faculty_id == CurrentUser.faculty_id).Select(s => s.faculty_name).FirstOrDefault();
            var GetBranch = DB.MASTER_BRANCH.Where(w => w.branch_id == CurrentUser.branch_id).Select(s => s.branch_name).FirstOrDefault();

            ViewBag.Faculty = GetFaculty;
            ViewBag.Branch = GetBranch;

            return View("Profile", Model);
        }
        #endregion

        #region ประวัติการสมัครงาน
        public async Task<IActionResult> HistoryRegister()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Gets = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var Model = new List<HistoryRegister>();

            foreach (var s in Gets.Where(w => w.s_id == CurrentUser.UserName))
            {
                foreach (var data in GetJob.Where(w => w.transaction_job_id == s.transaction_job_id))
                {
                    foreach (var item in GetStatus.Where(w => w.status_id == s.status_id))
                    {
                        var model = new HistoryRegister();
                        model.Id = s.transaction_register_id;
                        model.name = data.job_name;
                        model.detail = data.job_detail;
                        model.status = item.status_name;
                        model.register_date = s.register_date;
                        Model.Add(model);
                    }
                }
            }

            ViewBag.regisId = await DB.TRANSACTION_REGISTER.Select(s => s.transaction_register_id).FirstOrDefaultAsync();

            return PartialView("HistoryRegister", Model);
        }

        public async Task<IActionResult> DeleteRegisterJob(int id)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                var Model = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == id).FirstOrDefault();
                DB.TRANSACTION_REGISTER.Remove(Model);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "ยกเลิกการสมัครงานสำเร็จ/Delete Success" });
        }
        #endregion

        #region รายละเอียดงานที่รับสมัคร

        //หน้ารายละเอียดงาน
        public async Task<IActionResult> Job()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetPlace = await DB.MASTER_PLACE.ToListAsync();//ข้อมูลสถานที่
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();//ข้อมูลงาน
            var GetOwnerJob = await DB.Users.ToListAsync();//เจ้าของงาน

            //การ join table โดยนำค่าที่ต้องการมาเเสดง มาใส่ใน ViewsModels เเล้วไปเเสดงในหน้า Views
            var model = new List<ListJob>();


            foreach (var o in GetOwnerJob)
            {
                foreach (var j in GetJob.Where(w => w.create_by == o.UserName))
                {
                    foreach (var p in GetPlace.Where(w => w.place_id == j.place_id))
                    {
                        if(CurrentUser.faculty_id == o.faculty_id && CurrentUser.branch_id == o.branch_id)
                        {
                            var Model = new ListJob();
                            Model.id = j.transaction_job_id;
                            Model.jobname = j.job_name;
                            Model.jobplace = p.place_name;
                            Model.job_detail = j.job_detail;
                            Model.amount_person = j.amount_person;
                            Model.amount_working = j.amount_date;
                            Model.close_register = j.close_register_date;
                            model.Add(Model);
                        }
                    }
                }
            }

            return PartialView("Job", model);
        }

        //หน้าฟอร์มการสมัครงาน
        public IActionResult FormRegisterJob(int transaction_job_id)
        {
            var GetJobName = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == transaction_job_id).Select(s => s.job_name).FirstOrDefault();

            ViewBag.Bank = new SelectList(DB.MASTER_BANK.ToList(), "banktype_id", "banktype_name");
            ViewBag.jobname = GetJobName;

            return View("FormRegisterJob");
        }

        //นำข้อมูลใส่ดาต้าเบส
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormRegisterJob(TRANSACTION_REGISTER Model, IFormFile bank_file)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetOwner = await DB.TRANSACTION_JOB.ToListAsync();
            var GetRegister = await DB.TRANSACTION_REGISTER.ToListAsync();

            try
            {
                //อัพโหลดไฟล์สำเนาบัญชีธนาคาร
                var Uploads = Path.Combine(_environment.WebRootPath.ToString(), "uploads/bookbank/");
                string file = ContentDispositionHeaderValue.Parse(bank_file.ContentDisposition).FileName.Trim('"');
                string UniqueFileName = string.Format(@"{0}", Guid.NewGuid()) + file.ToString();

                using (var fileStream = new FileStream(Path.Combine(Uploads, UniqueFileName), FileMode.Create))
                {
                    await bank_file.CopyToAsync(fileStream);
                }

                Model.status_id = 8;
                Model.bank_file = UniqueFileName;
                Model.register_date = DateTime.Now;
                Model.fullname = CurrentUser.FirstName + " " + CurrentUser.LastName;
                Model.s_id = CurrentUser.UserName;
                Model.transaction_job_id = GetOwner.Select(s => s.transaction_job_id).FirstOrDefault();
                DB.TRANSACTION_REGISTER.Add(Model);
                await DB.SaveChangesAsync();

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }

            return RedirectToAction("HistoryRegister", "Student");

        }

        #endregion
    }
}
