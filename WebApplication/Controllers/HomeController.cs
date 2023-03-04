using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        //เเยกสิทธิ์ผ้ใช้งานระบบ
        public async Task<IActionResult> Index()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var CheckRoles = await DB.UserRoles.Where(w => w.UserId == CurrentUser.Id).Select(s => s.RoleId).FirstOrDefaultAsync();

            if (CheckRoles == "cfed75aa-4322-4f0a-ab5e-ea44e48d76e2") //อาจารย์เเละเจ้าหน้าที่สาขา
            {
                return RedirectToAction("Job", "Teacher");
            }
            else if (CheckRoles == "42d5797d-0dce-412b-beea-9337f482e9e5") //ฝ่ายพัฒนานักศึกษา
            {
                return RedirectToAction("Index", "Devstudent");
            }
            else if (CheckRoles == "34cdaea1-7b6d-4a1e-9c97-3542403bcb09") //เจ้าหน้าที่หน่วยงานในคณะ
            {
                return RedirectToAction("Job", "OfficerFaculty");
            }
            else if (CheckRoles == "e5ce49ea-eaf4-431e-b7c6-50ac72ff505b") //นักศึกษา
            {
                return RedirectToAction("Home", "Student");
            }
            else if(CheckRoles == "d93d333d-1a16-4707-b06e-eb14b89050f0") //กองพัฒนานักศึกษา
            {
                return RedirectToAction("Index", "DepartmentDevStudent");
            }
            else if(CheckRoles == "cddaeb6d-62db-4f03-98e5-8c473a5ff64e") //เจ้าหน้าที่หน่วยงาน
            {
                return RedirectToAction("Job", "Office");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
