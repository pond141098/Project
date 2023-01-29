using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public StudentController(
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
        public async Task<IActionResult> HistoryRegister()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Gets = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var Model = new List<HistoryRegister>();

            foreach (var s in Gets.Where(w => w.s_id == CurrentUser.UserName))
            {
                foreach(var data in GetJob.Where(w => w.transaction_job_id == s.transaction_job_id))
                {
                    foreach(var item in GetStatus.Where(w => w.status_id == s.status_id))
                    {
                        var model = new HistoryRegister();
                        model.name = data.job_name;
                        model.detail = data.job_detail;
                        model.status = item.status_name;
                        model.register_date = s.register_date;
                        Model.Add(model);
                    }
                }
            }

            ViewBag.regisId = await DB.TRANSACTION_REGISTER.Select(s => s.transaction_register_id).FirstOrDefaultAsync();

            return PartialView("HistoryRegister",Model);
        }
        public async Task<IActionResult>DeleteRegisterJob(int transaction_register_id)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                var Model = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == transaction_register_id).FirstOrDefault();
                DB.TRANSACTION_REGISTER.Remove(Model);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {

                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "ยกเลิกการสมัครงานสำเร็จ/Delete Success" });
        }
        #region รายละเอียดงานที่รับสมัคร
        public async Task<IActionResult> Job()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Gets = await DB.TRANSACTION_JOB.Where(w => w.faculty_id == CurrentUser.faculty_id && w.branch_id == CurrentUser.branch_id).ToListAsync();
          
            return PartialView("Job",Gets);
        }
        public IActionResult FormRegisterJob(int transaction_job_id)
        {
            var GetJobName = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == transaction_job_id).Select(s => s.job_name).FirstOrDefault();
            var GetBank = DB.MASTER_BANK.Select(s => s.banktype_name).ToList();

            ViewBag.Bank = GetBank;
            ViewBag.jobname = GetJobName;

            return View("FormRegisterJob");
        }

        [HttpPost]
        public async Task<IActionResult> FormRegisterJob(TRANSACTION_REGISTER Model, IFormFile[] fileUpload)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetOwner = await DB.TRANSACTION_JOB.ToListAsync();
            var GetRegister = await DB.TRANSACTION_REGISTER.ToListAsync();

            try
            {
                //เช็คว่านักศึกษาคนนี้ได้ทำการสมัครงานนี้ไปเเล้วหรือยัง
                if(GetRegister.Where(w =>w.s_id == CurrentUser.UserName && w.transaction_job_id >= 1).Count() >= 1)
                {
                    return Json(new { valid = true, message = "You Rigistered Gone !!!" });
                }
                Model.fullname = CurrentUser.FirstName + " " + CurrentUser.LastName;
                Model.s_id = CurrentUser.UserName;
                Model.register_date = DateTime.Now;
                Model.status_id = 8;
                Model.transaction_job_id = GetOwner.Select(s => s.transaction_job_id).FirstOrDefault();
                DB.TRANSACTION_REGISTER.Add(Model);
                await DB.SaveChangesAsync();

                //อัพโหลดไฟล์เอกสารสำเนาบัญชีธนาคารพร้อมเซ็นสำเนา
                if (fileUpload != null && fileUpload.Length > 0)
                {
                    var Uploads = Path.Combine(_environment.WebRootPath.ToString(), "uploads");
                    foreach (var file in fileUpload)
                    {
                        string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        string UniqueFileName = string.Format(@"{0}", Guid.NewGuid()) + fileName.ToString();
                        using (var fileStream = new FileStream(Path.Combine(Uploads, UniqueFileName), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // create file 
                        Model.bank_file = UniqueFileName;
                        DB.TRANSACTION_REGISTER.Add(Model);
                        await DB.SaveChangesAsync();
                    }
                }
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            //return Json(new { valid = true, message = "hello" });
            return RedirectToAction("HistoryRegister","Student");
        }

        #endregion
    }
}
