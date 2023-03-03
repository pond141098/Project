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
using SeniorProject.ViewModels.DepartmentDevStudent;
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
    public class DepartmentDevStudentController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;
        private readonly IWebHostEnvironment _environment;

        public DepartmentDevStudentController(
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

        #region Dashboard

        //แดชบอร์ด
        public IActionResult Index()
        {
            return View("Index");
        }

        #endregion

        #region นักศึกษาที่สมัครงาน

        //ข้อมมูลนศ.ที่สมัครงาน
        public IActionResult AllListStudent()
        {
            return View("AllListStudent");
        }
        public async Task<IActionResult> getAllListStudent()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetPerson = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var GetFaculty = await DB.MASTER_FACULTY.ToListAsync();
            var Gets = await DB.Users.ToListAsync();
            var Models = new List<AllListStudentRegister>();

            foreach (var j in GetJob)
            {
                foreach (var f in GetFaculty.Where(w => w.faculty_id == j.faculty_id))
                {
                    foreach (var data in GetPerson.Where(w => w.transaction_job_id == j.transaction_job_id))
                    {
                        foreach (var item in GetStatus.Where(w => w.status_id == data.status_id))
                        {
                            if (item.status_id == 7 || item.status_id == 6 || item.status_id == 5)
                            {
                                var Model = new AllListStudentRegister();
                                Model.id = data.transaction_register_id;
                                Model.student_name = data.fullname;
                                Model.student_id = data.student_id;
                                Model.faculty_name = f.faculty_name;
                                Model.status_name = item.status_name;
                                Model.job_name = j.job_name;
                                Models.Add(Model);
                            }
                        }
                    }
                }
            }
            return PartialView("getAllListStudent", Models);
        }

        //ดาวน์โหลดไฟล์
        public FileResult Download(string Name)
        {
            string path = Path.Combine(_environment.WebRootPath, "uploads/bookbank/") + Name;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", Name);
        }

        //ตรวจสอบ
        public IActionResult CheckRegisterAll(int transaction_register_id)
        {
            var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == transaction_register_id).FirstOrDefault();
            var GetBank = DB.MASTER_BANK.ToList();

            ViewBag.bank = GetBank.Where(w => w.banktype_id == Get.banktype_id).Select(s => s.banktype_name).FirstOrDefault();
            return View("CheckRegisterAll", Get);
        }

        //อนุมัติ
        [HttpPost]
        public async Task<IActionResult> Approve(TRANSACTION_REGISTER model)
        {
            try
            {
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.status_id == model.status_id).FirstOrDefault();

                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ
                if (Get.status_id == 5 || Get.status_id == 6)
                {
                    return Json(new { valid = false, message = "ไม่สามารถอนุมัติได้ !!!" });
                }

                Get.fullname = model.fullname;
                Get.student_id = model.student_id;
                Get.bank_file = model.bank_file;
                Get.bank_no = model.bank_no;
                Get.bank_store = model.bank_store;
                Get.because_job = model.because_job;
                Get.register_date = model.register_date;
                Get.status_id = 5;
                Get.approve_date = DateTime.Now;
                DB.TRANSACTION_REGISTER.Update(Get);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "อนุมัติสำเร็จ" });
        }

        //ไม่อนุมัติ
        [HttpPost]
        public async Task<IActionResult> NotApprove(TRANSACTION_REGISTER model)
        {
            try
            {
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.status_id == model.status_id).FirstOrDefault();

                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ
                if (Get.status_id == 5 || Get.status_id == 6 || Get.status_id == 7)
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
        #endregion

        #region จัดการผู้ใช้

        //หน้ารายการผู้ใช้ทั้งหมด
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
            foreach (var GetUser in GetUsers)
            {
                var Roles = DB.UserRoles.Where(w => w.UserId == GetUser.Id).Select(s => s.RoleId).FirstOrDefault();
                var ViewModel = new UserViewModels();

                if (Roles != "e5ce49ea-eaf4-431e-b7c6-50ac72ff505b" && Roles != "cfed75aa-4322-4f0a-ab5e-ea44e48d76e2" && Roles != "34cdaea1-7b6d-4a1e-9c97-3542403bcb09")
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

            ViewBag.faculty = new SelectList(DB.MASTER_FACULTY.ToList(), "faculty_id", "faculty_name");
            ViewBag.branch = new SelectList(DB.MASTER_BRANCH.Where(w => w.faculty_id == 13).ToList(), "branch_id", "branch_name"); ;
            ViewBag.prefix = new SelectList(DB.MASTER_PREFIX.ToList(), "prefix_id", "prefix_name");

            ViewBag.Role = new SelectList(DB.Roles.Where(w => w.Name != "กองพัฒนานักศึกษา" && w.Name != "นักศึกษา" && w.Name != "เจ้าหน้าที่หน่วยงานในคณะ" && w.Name != "อาจารย์/เจ้าหน้าที่สาขา").ToList(), "Id", "Name");

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
                if (regex.IsMatch(models.PhoneNumber))
                {
                    //เช็คชื่อผู้ใช้ซ้ำ
                    if (DB.Users.Where(w => w.UserName == models.UserName).Select(s => s.Id).Count() > 0)
                    {
                        return Json(new { valid = false, message = "มีชื่อผู้ใช้ในระบบอยู่เเล้ว!!!" });
                    }

                    //เช็คจำนวนฝ่ายพัฒนานักศึกษาห้ามเกิน 13 คณะ
                    if (DB.UserRoles.Where(w => w.RoleId == "42d5797d-0dce-412b-beea-9337f482e9e5").Select(s => s.UserId).Count() == 13)
                    {
                        return Json(new { valid = false, message = "มีสิทธิ์ของฝ่ายพัฒนานักศึกษาครบเเล้ว!!!" });
                    }

                    //ถ้าเลือกสิทธิ์เป็นฝ่ายพัฒนานักศึกษาเเล้วไม่ได้เลือก(มหาลัย/คณะ) = คณะ เเละไมได้เลือก(หน่วยงาน) = หน่วยงาน จะไม่สามารถบันทึกได้ 
                    if (models.RoleId == "42d5797d-0dce-412b-beea-9337f482e9e5" && models.faculty_id == 13 && models.branch_id != 86)
                    {
                        return Json(new { valid = false, message = "กรุณาตรวจสอบข้อมูลสิทธิ์ มหาวิทยาลัย/คณะ เเละ หน่วยงานใหม่ !!!" });
                    }

                    //ถ้าเลือกสิทธิ์เป็นเจ้าหน้าที่หน่วยงานเเล้วไม่ได้เลือก(มหาลัย/คณะ)=มหาลัย เเละได้เลือก(หน่วยงาน) = หน่วยงาน จะไม่สามารถบันทึกได้
                    var branchName = DB.MASTER_BRANCH.Where(w => w.branch_id == models.branch_id).Select(s => s.branch_name).FirstOrDefault();
                    if (models.RoleId == "cddaeb6d-62db-4f03-98e5-8c473a5ff64e" && models.faculty_id != 13 || models.faculty_id == 13 && branchName == "หน่วยงาน")
                    {
                        return Json(new { valid = false, message = "กรุณาตรวจสอบข้อมูลสิทธิ์ มหาวิทยาลัย/คณะ เเละ หน่วยงานใหม่ !!!" });
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
                else
                {
                    return Json(new { valid = false, message = "กรุณากรอกเบอร์โทรศัพท์ใหม่ !!!" });
                }

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "เพิ่มสมาชิกเรียบร้อย" });
        }

        //เเก้ไขข้อมูลผู้ใช้
        public IActionResult EditUser(string UserId)
        {
            var User = DB.Users.Where(w => w.Id == UserId).FirstOrDefault();
            var GetRoleId = DB.UserRoles.Where(w => w.UserId == User.Id).Select(s => s.RoleId).FirstOrDefault();
            var GetFaculty = DB.MASTER_FACULTY.Where(w => w.faculty_id == User.faculty_id).Select(s => s.faculty_id).FirstOrDefault();
            var GetBranch = DB.MASTER_BRANCH.Where(w => w.branch_id == User.branch_id).Select(s => s.branch_id).FirstOrDefault();
            var GetPrefix = DB.MASTER_PREFIX.Where(w => w.prefix_id == User.prefix_id).Select(s => s.prefix_id).FirstOrDefault();

            ViewBag.Role = new SelectList(DB.Roles.Where(w => w.Name != "นักศึกษา" && w.Name != "เจ้าหน้าที่หน่วยงานในคณะ" && w.Name != "อาจารย์/เจ้าหน้าที่สาขา").ToList(), "Id", "Name", GetRoleId);
            ViewBag.faculty = new SelectList(DB.MASTER_FACULTY.ToList(), "faculty_id", "faculty_name", GetFaculty);
            ViewBag.branch = new SelectList(DB.MASTER_BRANCH.ToList(), "branch_id", "branch_name", GetBranch); ;
            ViewBag.prefix = new SelectList(DB.MASTER_PREFIX.ToList(), "prefix_id", "prefix_name", GetPrefix);

            var ViewModel = new AddUserViewModels();
            ViewModel.Id = User.Id;
            ViewModel.FirstName = User.FirstName;
            ViewModel.LastName = User.LastName;
            ViewModel.Password = User.PasswordHash;
            ViewModel.UserName = User.UserName;
            ViewModel.PhoneNumber = User.PhoneNumber;
            ViewModel.prefix_id = User.prefix_id;
            ViewModel.faculty_id = User.faculty_id;
            ViewModel.branch_id = User.branch_id;

            return View("EditUser", ViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(AddUserViewModels model, string OldPassword)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            string Msg = "";
            string pattern = @"^(0[6|8|9]{1}[0-9]{8})$";
            Regex regex = new Regex(pattern);

            try
            {
                var CheckPhone = regex.IsMatch(model.PhoneNumber);
                if (CheckPhone == false)
                {
                    return Json(new { valid = false, message = "กรุณากรอกเบอร์โทรศัพท์ใหม่" });
                }

                //ถ้าเลือกสิทธิ์เป็นฝ่ายพัฒนานักศึกษาเเล้วไม่ได้เลือก(มหาลัย/คณะ) = คณะ เเละไมได้เลือก(หน่วยงาน) = หน่วยงาน จะไม่สามารถบันทึกได้ 
                if (model.RoleId == "42d5797d-0dce-412b-beea-9337f482e9e5" && model.faculty_id == 13 && model.branch_id != 86)
                {
                    return Json(new { valid = false, message = "กรุณาตรวจสอบข้อมูลสิทธิ์ มหาวิทยาลัย/คณะ เเละ หน่วยงานใหม่ !!!" });
                }

                //ถ้าเลือกสิทธิ์เป็นเจ้าหน้าที่หน่วยงานเเล้วไม่ได้เลือก(มหาลัย/คณะ)=มหาลัย เเละได้เลือก(หน่วยงาน) = หน่วยงาน จะไม่สามารถบันทึกได้
                var branchName = DB.MASTER_BRANCH.Where(w => w.branch_id == model.branch_id).Select(s => s.branch_name).FirstOrDefault();
                if (model.RoleId == "cddaeb6d-62db-4f03-98e5-8c473a5ff64e" && model.faculty_id != 13 || model.faculty_id == 13 && branchName == "หน่วยงาน")
                {
                    return Json(new { valid = false, message = "กรุณาตรวจสอบข้อมูลสิทธิ์ มหาวิทยาลัย/คณะ เเละ หน่วยงานใหม่ !!!" });
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
