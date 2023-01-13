using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SeniorProject.Data;
using SeniorProject.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Controllers;

namespace SeniorProject.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;

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
        public IActionResult Home()
        {
            return View("Home");
        }
        public IActionResult Profile()
        {
            return View("Profile");   
        }
        public IActionResult HistoryRegister()
        {
            return View("HistoryRegister");
        }
        #region รายละเอียดงานที่รับสมัคร
        public IActionResult Job()
        {
            var Gets = DB.TRANSACTION_JOB.ToList(); 
            return PartialView("Job",Gets);
        }
        public IActionResult FormRegisterJob(int transaction_job_id)
        {
            var Jobid = DB.TRANSACTION_JOB.ToList();
            var Gets = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == transaction_job_id);
            ViewBag.Bank = new SelectList(DB.MASTER_BANK.ToList(),"banktype_id","bank_name"); 
            ViewBag.jobid = transaction_job_id;
            return View("FormRegisterJob");
        }
        [HttpPost]
        public async Task<IActionResult> FormRegisterJob(TRANSACTION_REGISTER Model)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                if (DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == Model.transaction_register_id).Count() > 0)
                {
                    return Json(new { valid = false, message = "กรุณาตรวจสอบข้อมูล" });
                }
                Model.register_date= DateTime.Now;
                Model.status_id = 7;
                DB.TRANSACTION_REGISTER.Add(Model);
                await DB.SaveChangesAsync();

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "บันทึกข้อมูลสำเร็จ" });
        }

        #endregion
    }
}
