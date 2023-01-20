using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Engines;
using SeniorProject.Data;
using SeniorProject.Models;
using SeniorProject.ViewModels.Teacher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebApplication.Controllers;

namespace SeniorProject.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;

        public TeacherController(
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

        #region รายชื่อนักศึกษาที่สมัครงาน
        public async Task<IActionResult> ListStudent(int transaction_job_id) 
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            //ViewBag.Jobname = new SelectList(DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == transaction_job_id ).ToList(), "transaction_job_id", "job_name");

            return View("ListStudent");
        }
        public async Task<IActionResult> getListStudent()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            
            var GetJobName = await DB.TRANSACTION_JOB.ToListAsync();
            var GetPerson = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.Where(w => w.status_id == 5 && w.status_id == 6 && w.status_id == 7).FirstOrDefaultAsync();
            var Models = new List<ListStudentRegister>();

            foreach(var data in GetPerson.Where(w => w.owner_job_id == CurrentUser.Id))
            {
                var ViewModel = new ListStudentRegister();
                ViewModel.job_name = GetJobName.Where(w => w.create_by == CurrentUser.Id).Select(s => s.job_name).FirstOrDefault();
                ViewModel.student_name = GetPerson.Select(s => s.fullname).FirstOrDefault();
                ViewModel.s_id = GetPerson.Select(s => s.s_id).FirstOrDefault();
                ViewModel.register_date = GetPerson.Select(s => s.register_date).FirstOrDefault();
                //ViewModel.status_name = GetPerson.Where(w=>w.status_id =
                Models.Add(ViewModel);

            }

            //var Gets = await DB.TRANSACTION_REGISTER.Where(w => w.owner_job_id == CurrentUser.Id).ToListAsync();
            
            return PartialView("getListStudent",Models);
        }
        #endregion

        #region ขอรับนักศึกษามาปฎิบัติงาน
        public IActionResult Job()
        {
            return View("Job");
        }
        public async Task<IActionResult> getJob()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Gets = DB.TRANSACTION_JOB.Where(w => w.create_by == CurrentUser.Id).ToList();
            return PartialView("getJob", Gets);
        }

        //เพิ่ม
        public IActionResult FormAddJob()
        {
            return View("FormAddJob");
        }
        [HttpPost]
        public async Task<IActionResult> FormAddJob(TRANSACTION_JOB Model)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                if (DB.TRANSACTION_JOB.Where(w => w.job_name == Model.job_name).Count() > 0)
                {
                    return Json(new { valid = false, message = "กรุณาตรวจสอบข้อมูล" });
                }

                Model.owner_job = CurrentUser.FirstName + " " + CurrentUser.LastName;
                Model.create_by = CurrentUser.Id;
                Model.update_date = DateTime.Now;
                Model.faculty_id = CurrentUser.faculty_id;
                Model.branch_id = CurrentUser.branch_id;
                DB.TRANSACTION_JOB.Add(Model);
                await DB.SaveChangesAsync();

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "บันทึกข้อมูลสำเร็จ" });
        }

        //เเก้ไข
        public IActionResult FormEditJob(int transaction_job_id)
        {
            var Get = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == transaction_job_id).FirstOrDefault();
            return View("FormEditJob", Get);
        }
        [HttpPost]
        public async Task<IActionResult> FormEditJob(TRANSACTION_JOB Model)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                if (DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == Model.transaction_job_id).Select(s => s.job_name).FirstOrDefault() == Model.job_name)
                {
                    Model.update_date = DateTime.Now;
                    Model.faculty_id= CurrentUser.faculty_id;
                    Model.branch_id= CurrentUser.branch_id;
                    DB.TRANSACTION_JOB.Update(Model);
                    await DB.SaveChangesAsync();
                }
                else
                {
                    if (DB.TRANSACTION_JOB.Where(w => w.job_name == Model.job_name).Count() > 0)
                    {
                        return Json(new { valid = false, message = "ข้อมูลไม่ถูกต้อง กรุณาตรวจสอบดีๆ" });
                    }
                }
            }
            catch (Exception Error)
            {

                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "บันทึกข้อมูลสำเร็จ" });
        }

        //ลบ
        public async Task<IActionResult> DeleteJob(int transaction_job_id)
        {
            //var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                var Get = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == transaction_job_id).FirstOrDefault();
                DB.TRANSACTION_JOB.Remove(Get);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {

                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "ลบข้อมูลสำเร็จ" });
        }
        #endregion

        #region เวลาการทำงานนักศึกษา
        public IActionResult CheckTime()
        {
            return View("CheckTime");
        }
        public IActionResult getCheckTime()
        {
            var Gets = DB.MASTER_BANK.ToList();
            return PartialView("GetCheckTime", Gets);
        }

        #endregion
    }
}
