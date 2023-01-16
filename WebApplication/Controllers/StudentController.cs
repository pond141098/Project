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
        public async Task<IActionResult> Index()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            if (CurrentUser.role_id == 2)
            {
                return RedirectToAction("Index", "Home");
            }
            //else if(CurrentUser.role_id == 2)
            //{
            //    return RedirectToAction("Index", "Home");
            //}
            //else if (CurrentUser.role_id == 3)
            //{
            //    return RedirectToAction("Index", "Devstudent");
            //}
            //else if (CurrentUser.role_id == 4)
            //{
            //    return RedirectToAction("Index", "Office");
            //}
            return View();
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
        public IActionResult HistoryRegister()
        {
            return View("HistoryRegister");
        }
        #region รายละเอียดงานที่รับสมัคร
        public IActionResult Job()
        {
            var Gets = DB.TRANSACTION_JOB.ToList();
            return PartialView("Job", Gets);
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
        [Obsolete]
        public async Task<IActionResult> FormRegisterJob(TRANSACTION_REGISTER Model, IFormFile[] fileUpload)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {

                Model.s_id = CurrentUser.UserName;
                Model.register_date = DateTime.Now;
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
