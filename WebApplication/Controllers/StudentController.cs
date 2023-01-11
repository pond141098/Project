using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SeniorProject.Data;
using SeniorProject.Models;
using System.Linq;
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
        #region รายละเอียดงานที่รับสมัคร
        public IActionResult Job()
        {
            return View("Job");
        }
        public IActionResult getJob()
        {
            var Get = DB.TRANSACTION_JOB.ToList();
            return PartialView("getJob", Get);
        }
        public IActionResult FormRegisterJob()
        {
            return View("FormRegister");
        }
        
        #endregion
    }
}
