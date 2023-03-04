using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SeniorProject.Data;
using SeniorProject.Models;
using SeniorProject.ViewModels.Devstudent;
using SeniorProject.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApplication.Controllers;
using WebApplication.Data;
using WebApplication.Models;
using Windows.System;

namespace SeniorProject.Controllers
{
    public class DevstudentController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;
        private readonly IWebHostEnvironment _environment;

        public DevstudentController(
             ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext db,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            DB = db;
            _environment = environment;
        }

        #region DashBoard

        //เเดชบอร์ด
        public async Task<IActionResult> Index()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetUsers = await DB.Users.FirstOrDefaultAsync();
            var GetBranch = await DB.MASTER_BRANCH.ToListAsync();
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();

            //จำนวนักศึกษาทั้งหมด
            var RoleStudent = await DB.UserRoles.Where(w => w.UserId == GetUsers.Id && w.RoleId == "e5ce49ea-eaf4-431e-b7c6-50ac72ff505b").Select(s => s.UserId).FirstOrDefaultAsync();
            var Student = await DB.Users.Where(w => w.faculty_id == CurrentUser.faculty_id && w.Id == RoleStudent).Select(s => s.Id).CountAsync();
            //จำนวนนักศึกษาที่สมัครงานทั้งหมด
            var Job = await DB.TRANSACTION_JOB.Where(w => w.faculty_id == CurrentUser.faculty_id).Select(s => s.transaction_job_id).FirstOrDefaultAsync();
            var Register = await DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == Job).Select(s => s.transaction_register_id).CountAsync();

            //จำนวนนักศึกษาที่สมัครงานกับหน่วยงานในคณะ
            var JobOfficeFaculty = await DB.TRANSACTION_JOB.Where(w => w.faculty_id == CurrentUser.faculty_id && w.type_job_id == 2).Select(s => s.transaction_job_id).FirstOrDefaultAsync();
            var RegisterOfficeFaculty = await DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobOfficeFaculty).Select(s => s.transaction_register_id).CountAsync();
            //จำนวนนักศึกษาที่สมัครงานในเเต่ละสาขา
            var Model = new List<dashboard>();

            foreach(var j in GetJob.Where(w => w.faculty_id == CurrentUser.faculty_id))
            {
                foreach(var b in GetBranch.Where(w => w.branch_id == j.branch_id))
                {
                    var data = new dashboard();
                    if(j.type_job_id != 1 && j.type_job_id != 2)
                    {
                        data.branchName = b.branch_name;
                        data.amount_register = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == j.transaction_job_id).Select(s => s.transaction_register_id).Count();
                        Model.Add(data);
                    }
                }
            }

            //Chart 1
            ViewBag.Student = Student;
            ViewBag.Register = Register;

            //Chart 2
            ViewBag.OfficeFaculty = RegisterOfficeFaculty;

            return PartialView("Index", Model);
        }

        #endregion

        #region รายชื่อนักศึกษาที่อาจารย์/เจ้าหน้าที่ในคณะส่งมา

        //ข้อมูลนศ.ที่สมัครงาน
        public IActionResult ListStudentFaculty()
        {
            return View("ListStudentFaculty");
        }

        public async Task<IActionResult> getListStudentFaculty()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetPerson = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var GetBranch = await DB.MASTER_BRANCH.ToListAsync();
            var GetPrefix = await DB.MASTER_PREFIX.ToListAsync();
            var Models = new List<ListStudentRegisterFaculty>();

            foreach(var j in GetJob.Where(w => w.faculty_id == CurrentUser.faculty_id))
            {
                foreach(var r in GetPerson.Where(w => w.transaction_job_id == j.transaction_job_id))
                {
                    foreach(var b in GetBranch.Where(w => w.branch_id == j.branch_id))
                    {
                        foreach(var s in GetStatus.Where(w => w.status_id == r.status_id))
                        {
                            var Model = new ListStudentRegisterFaculty();
                            if (s.status_id == 9 || s.status_id == 7 || s.status_id == 6 || s.status_id == 8 || s.status_id == 5)
                            {
                                Model.id = r.transaction_register_id;
                                Model.job_name = j.job_name;
                                Model.branch_name = b.branch_name;
                                Model.student_name = r.fullname;
                                Model.s_id = r.student_id;
                                Model.register_date = r.register_date;
                                Model.status_name = s.status_name;
                                Models.Add(Model);
                            }
                        }
                    }
                }
            }
            return PartialView("getListStudentFaculty", Models);
        }

        //ดาวน์โหลดไฟล์
        public FileResult Download(string Name)
        {
            string path = Path.Combine(_environment.WebRootPath, "uploads/bookbank/") + Name;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", Name);
        }

        //ตรวจสอบ
        public IActionResult CheckRegisterFaculty(int transaction_register_id)
        {
            var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == transaction_register_id).FirstOrDefault();
            var GetBank = DB.MASTER_BANK.ToList();

            ViewBag.bank = GetBank.Where(w => w.banktype_id == Get.banktype_id).Select(s => s.banktype_name).FirstOrDefault();
            return View("CheckRegisterFaculty", Get);
        }

        //อนุมัติ ส่งกองพัฒนานักศึกษา
        [HttpPost]
        public async Task<IActionResult> Approve(TRANSACTION_REGISTER model)
        {
            try
            {
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.status_id == model.status_id).FirstOrDefault();

                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ
                if (Get.status_id == 5 || Get.status_id == 6 || Get.status_id == 7)
                {
                    return Json(new { valid = false, message = "ไม่สามารถส่งอนุมัติได้ !!!" });
                }

                Get.fullname = model.fullname;
                Get.student_id = model.student_id;
                Get.bank_file = model.bank_file;
                Get.bank_no = model.bank_no;
                Get.bank_store = model.bank_store;
                Get.because_job = model.because_job;
                Get.register_date = model.register_date;
                Get.status_id = 7;
                Get.approve_devstudent_date = DateTime.Now;
                DB.TRANSACTION_REGISTER.Update(Get);
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
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.status_id == model.status_id).FirstOrDefault();

                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ 
                if (Get.status_id == 5 || Get.status_id == 6)
                {
                    return Json(new { valid = false, message = "ไม่สามารถไม่อนุมัติได้ !!!" });
                }

                Get.fullname = model.fullname;
                Get.student_id = model.student_id;
                Get.bank_file = model.bank_file;
                Get.bank_no = model.bank_no;
                Get.bank_store = model.bank_store;
                Get.because_job = model.because_job;
                Get.register_date = model.register_date;
                Get.status_id = 6;
                Get.notapprove_date = DateTime.Now;
                DB.TRANSACTION_REGISTER.Update(Get);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "ไม่อนุมัติสำเร็จ" });
        }

        //อนุมัติทั้งหมด
        public async Task<IActionResult> AllApprove()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "อนุมัติเรียบร้อย" });
        }

        #endregion

        #region จัดการผู้ใช้

        //รายการผู้ใช้ระบบ
        public IActionResult UserIndex()
        {
            return View("UserIndex");
        }

        public async Task<IActionResult> GetUserIndex()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Prefix = await DB.MASTER_PREFIX.ToListAsync();
            var Faculty = await DB.MASTER_FACULTY.ToListAsync();
            var Branch = await DB.MASTER_BRANCH.ToListAsync();
            

            var ViewModels = new List<UserViewModels>();
            var GetUsers = DB.Users.ToList();
            foreach (var GetUser in GetUsers.Where(w => w.faculty_id == CurrentUser.faculty_id))
            {
                var Roles = DB.UserRoles.Where(w => w.UserId == GetUser.Id).Select(s => s.RoleId).FirstOrDefault();
                var ViewModel = new UserViewModels();

                if (Roles != "e5ce49ea-eaf4-431e-b7c6-50ac72ff505b")
                {
                    ViewModel.UserId = GetUser.Id;
                    ViewModel.FullName = Prefix.Where(w => w.prefix_id == GetUser.prefix_id).Select(s => s.prefix_name).FirstOrDefault() + " " + GetUser.FirstName + " " + GetUser.LastName;
                    ViewModel.Username = GetUser.UserName;
                    ViewModel.FacultyName = Faculty.Where(w => w.faculty_id == GetUser.faculty_id).Select(s => s.faculty_name).FirstOrDefault();
                    ViewModel.BranchName = Branch.Where(w => w.branch_id == GetUser.branch_id).Select(s => s.branch_name).FirstOrDefault();
                    ViewModel.PhoneNumber = GetUser.PhoneNumber;
                    ViewModels.Add(ViewModel);
                }
            }

            return PartialView("GetUserIndex", ViewModels);
        }

        //เพิ่มผู้ใช้ระบบ
        public async Task<IActionResult> AddUser()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            ViewBag.faculty = new SelectList(DB.MASTER_FACULTY.Where(w => w.faculty_id == CurrentUser.faculty_id).ToList(), "faculty_id", "faculty_name");
            ViewBag.branch = new SelectList(DB.MASTER_BRANCH.Where(w => w.faculty_id == CurrentUser.faculty_id || w.faculty_id == 13).ToList(), "branch_id", "branch_name");
            ViewBag.prefix = new SelectList(DB.MASTER_PREFIX.ToList(), "prefix_id", "prefix_name");

            ViewBag.Role = new SelectList(DB.Roles.Where(w => w.Name != "กองพัฒนานักศึกษา" && w.Name != "ฝ่ายพัฒนานักศึกษา" && w.Name != "เจ้าหน้าที่หน่วยงาน").ToList(), "Id", "Name");

            return View("AddUser");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(AddUserViewModels models, string RoleId)
        {
            string Msg = "";
            string pattern = @"^(0[6|8|9]{1}[0-9]{8})$";
            Regex regex = new Regex(pattern);

            try
            {
                var CheckPhone = regex.IsMatch(models.PhoneNumber);
                if(CheckPhone == false)
                {
                    return Json(new { valid = false, message = "กรุณากรอกเบอร์โทรศัพท์ใหม่" });
                }

                var user = new ApplicationUser
                {
                    FirstName = models.FirstName,
                    LastName = models.LastName,
                    Email = models.UserName,
                    UserName = models.UserName,
                    PhoneNumber = models.PhoneNumber,
                    faculty_id = models.faculty_id,
                    branch_id = models.branch_id,
                    prefix_id = models.prefix_id
                };

                var result = await _userManager.CreateAsync(user, models.Password);
                if (result.Succeeded)
                {
                    var currentUser = await _userManager.FindByEmailAsync(user.Email);
                    var GetAllRoles = await _roleManager.Roles.Where(w => w.Id == RoleId).ToListAsync();

                    foreach (var GetAllRole in GetAllRoles)
                    {
                        var roleresult = await _userManager.AddToRoleAsync(currentUser, GetAllRole.NormalizedName);
                    }

                    // add log
                    var UserLogin = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
                }
                else
                {
                    foreach (var Error in result.Errors)
                    {
                        Msg = Error.Description + "<br/>";
                    }
                    return Json(new { valid = false, message = Msg });
                }
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "เพิ่มสมาชิกเรียบร้อย" });
        }

        //เเก้ไขข้อมูลผู้ใช้
        public async Task<IActionResult> EditUser(string UserId)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            var GetUser = DB.Users.Where(w => w.Id == UserId).FirstOrDefault();
            var GetRoleId = DB.UserRoles.Where(w => w.UserId == GetUser.Id).Select(s => s.RoleId).FirstOrDefault();
            var GetFaculty = DB.MASTER_FACULTY.Where(w => w.faculty_id == GetUser.faculty_id).Select(s => s.faculty_id).FirstOrDefault();
            var GetBranch = DB.MASTER_BRANCH.Where(w => w.branch_id == GetUser.branch_id).Select(s => s.branch_id).FirstOrDefault();
            var GetPrefix = DB.MASTER_PREFIX.Where(w => w.prefix_id == GetUser.prefix_id).Select(s => s.prefix_id).FirstOrDefault();

            ViewBag.Role = new SelectList(DB.Roles.Where(w => w.Name != "กองพัฒนานักศึกษา" && w.Name != "ฝ่ายพัฒนานักศึกษา" && w.Name != "เจ้าหน้าที่หน่วยงาน").ToList(), "Id", "Name",GetRoleId);
            ViewBag.faculty = new SelectList(DB.MASTER_FACULTY.Where(w => w.faculty_id == CurrentUser.faculty_id).ToList(), "faculty_id", "faculty_name", GetFaculty);
            ViewBag.branch = new SelectList(DB.MASTER_BRANCH.Where(w => w.faculty_id == CurrentUser.faculty_id).ToList(), "branch_id", "branch_name", GetBranch); ;
            ViewBag.prefix = new SelectList(DB.MASTER_PREFIX.ToList(), "prefix_id", "prefix_name", GetPrefix);

            var ViewModel = new AddUserViewModels();
            ViewModel.Id = GetUser.Id;
            ViewModel.FirstName = GetUser.FirstName;
            ViewModel.LastName = GetUser.LastName;
            ViewModel.Password = GetUser.PasswordHash;
            ViewModel.UserName = GetUser.UserName;
            ViewModel.PhoneNumber = GetUser.PhoneNumber;
            ViewModel.prefix_id = GetUser.prefix_id;
            ViewModel.faculty_id = GetUser.faculty_id;
            ViewModel.branch_id = GetUser.branch_id;

            return View("EditUser",ViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(AddUserViewModels model,string OldPassword)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            string Msg = "";
            string pattern = @"^(0[6|8|9]{1}[0-9]{8})$";
            Regex regex = new Regex(pattern);

            try
            {
                var CheckPhone = regex.IsMatch(model.PhoneNumber);
                if(CheckPhone == false)
                {
                    return Json(new { valid = false, message = "กรุณากรอกเบอร์โทรศัพท์ใหม่" });
                }

                var ThisUser = await _userManager.FindByIdAsync(model.Id);

                ThisUser.UserName = model.UserName;
                ThisUser.FirstName = model.FirstName;
                ThisUser.LastName = model.LastName;
                ThisUser.Email = model.UserName;
                ThisUser.faculty_id = model.faculty_id;
                ThisUser.branch_id = model.branch_id;
                ThisUser.prefix_id = model.prefix_id;
                ThisUser.LastName = model.LastName;
                ThisUser.PhoneNumber = model.PhoneNumber;

                string GeneratePassword = "";
                if (model.Password == null)
                {
                    ThisUser.PasswordHash = OldPassword;
                }
                else
                {
                    GeneratePassword = model.Password;
                    var Code = await _userManager.GeneratePasswordResetTokenAsync(ThisUser);
                    var ChangePassword = await _userManager.ResetPasswordAsync(ThisUser, Code, GeneratePassword);
                    if (!ChangePassword.Succeeded)
                    {
                        return Json(new { valid = false, message = ChangePassword.Errors.Select(s => s.Code + " " + s.Description).FirstOrDefault() });
                    }
                }

                var result = await _userManager.UpdateAsync(ThisUser);
                if (result.Succeeded)
                {
                    if (DB.UserRoles.Where(w => w.UserId == model.Id).Count() > 0)
                    {
                        var RoleThisUser = DB.UserRoles.Where(w => w.UserId == model.Id).FirstOrDefault();
                        DB.UserRoles.Remove(RoleThisUser);
                        DB.SaveChanges();

                        var currentUser = await _userManager.FindByEmailAsync(model.UserName);
                        var GetAllRoles = await _roleManager.Roles.Where(w => w.Id == model.RoleId).ToListAsync();
                        foreach (var GetAllRole in GetAllRoles)
                        {
                            var roleresult = await _userManager.AddToRoleAsync(currentUser, GetAllRole.NormalizedName);
                        }
                    }

                    var UserLogin = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

                    return Json(new { valid = true, Message = "บักทึกข้อมูลสำเร็จ" });
                }
                else
                {
                    foreach (var Error in result.Errors)
                    {
                        Msg = Error.Description + "<br/>";
                    }

                    return Json(new { valid = false, message = Msg });
                }
            }
            catch (Exception Error)
            {
                Msg = "Error is : " + Error.Message;
                return Json(new { valid = false, message = Msg });
            }
        }

        //ลบข้อมูลผู้ใช้
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string UserId)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            string Msg = "";
            try
            {
                // check administrator
                if (DB.UserRoles.Where(w => w.UserId == UserId).Count() > 0)
                {
                    var GetRoles = DB.UserRoles.Where(w => w.UserId == UserId);
                    DB.RemoveRange(GetRoles);
                    DB.SaveChanges();
                }

                var GetUser = DB.Users.Where(w => w.Id == UserId).FirstOrDefault();
                DB.Users.Remove(GetUser);
                DB.SaveChanges();

            }
            catch (Exception Error)
            {
                Msg = "Error is : " + Error.Message;
                return Json(new { valid = false, message = Msg });
            }
            return Json(new { valid = true, Message = "บันทึกรายการสำเร็จ" });
        }

        #endregion
    }
}
