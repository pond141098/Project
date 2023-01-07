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

                //Model.VehicleDiscountRate = 0;
                //Model.UpdateDate = DateTime.Now;
                //Model.UpdateBy = CurrentUser.Id;
                DB.TRANSACTION_JOB.Add(Model);
                await DB.SaveChangesAsync();
                // add log
                //Helper.AddLogs(CurrentUser.Id, "เพิ่มข้อมูลงาน", HttpContext, SystemName, CurrentUser.OrgCode);
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "บันทึกข้อมูลสำเร็จ" });
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
