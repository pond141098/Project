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
        #region Dashboard
        public IActionResult Index()
        {
            return View("Index");
        }
        #endregion

        #region รายชื่อนักศึกษาที่สมัครงาน
        public async Task<IActionResult> ListStudent() 
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
           
            return View("ListStudent");
        }
        public async Task<IActionResult> getListStudent()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetPerson = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var GetBank = await DB.MASTER_BANK.ToListAsync();
            var Models = new List<ListStudentRegister>();

            ViewBag.transaction_register_id = GetPerson.Select(s => s.transaction_register_id).FirstOrDefault();

            foreach(var data in GetJob.Where(w => w.create_by == CurrentUser.Id))
            {
                foreach(var regis in GetPerson.Where(w => w.transaction_job_id == data.transaction_job_id))
                {
                    foreach (var stat in GetStatus.Where(w => w.status_id == regis.status_id))
                    {
                        foreach(var b in GetBank.Where(w => w.banktype_id == regis.banktype_id))
                        {
                            var model = new ListStudentRegister();
                            model.id = regis.transaction_register_id;
                            model.student_name = regis.fullname;
                            model.job_name = data.job_name;
                            model.s_id = regis.s_id;
                            model.register_date = regis.register_date;
                            model.status_name = stat.status_name;
                            model.because_working = regis.because_job;
                            model.file = regis.bank_file;
                            model.bank = regis.bank_no+"("+b.banktype_name+")";
                            Models.Add(model);
                        }
                    }
                }
            }
            return PartialView("getListStudent",Models);
        }

        //ตรวจสอบ
        public IActionResult CheckRegister(int transaction_register_id)
        {
            var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == transaction_register_id).FirstOrDefault();
            return View("CheckRegister",Get);
        }

        //ส่งอนุมัติ ส่งฝ่ายพัฒนานักศึกษา
        [HttpPost]
        public async Task<IActionResult> Approve(TRANSACTION_REGISTER model)
        {
            try
            {
                var GetStat = await DB.TRANSACTION_REGISTER.Where(w => w.status_id == model.status_id).FirstOrDefaultAsync();

                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ
                if (GetStat.status_id == 5 || GetStat.status_id == 6 || GetStat.status_id == 7)
                {
                    return Json(new { valid = false, message = "ไม่สามารถส่งอนุมัติได้ !!!" });
                }
                model.status_id = 9;
                DB.TRANSACTION_REGISTER.Update(model);
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
                var GetStat = await DB.TRANSACTION_REGISTER.Where(w => w.status_id == model.status_id).FirstOrDefaultAsync();
                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ  
                if(GetStat.status_id == 5 || GetStat.status_id == 6 || GetStat.status_id == 7 )
                {
                    return Json(new { valid = false, message = "ไม่สามารถไม่อนุมัติได้ !!!" });
                }
                model.status_id = 6;
                DB.TRANSACTION_REGISTER.Update(model);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "ไม่อนุมัติสำเร็จ" });
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

        #region เวลาการทำงานนักศึกษา/ออกรายงาน
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
