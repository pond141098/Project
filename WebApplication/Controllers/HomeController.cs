using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SeniorProject.Data;
using SeniorProject.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;

        public HomeController(
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

        #region รายชื่อนักศึกษาที่สมัครงาน
        public IActionResult Index()
        {
            return View("Index");
        }
        #endregion

        #region ขอรับนักศึกษามาปฎิบัติงาน
        public IActionResult Job()
        {
            return View("Job");
        }
        public IActionResult getJob()
        {
            var Gets = DB.TRANSACTION_JOB.ToList();
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
                if (DB.TRANSACTION_JOB.Where(w=>w.transaction_job_id == Model.transaction_job_id).Select(s=>s.job_name).FirstOrDefault()==Model.job_name)
                {
                    Model.update_date = DateTime.Now;   
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

                return Json (new {valid = false, message = Error.Message});
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
            return PartialView("GetCheckTime",Gets);
        }
       
        #endregion



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
