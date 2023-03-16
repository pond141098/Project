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
            var Faculty =  DB.MASTER_FACULTY.FirstOrDefault();
            var Role =  DB.UserRoles.FirstOrDefault();
            var TypeJob = DB.MASTER_TYPEJOB.FirstOrDefault();

            //นักศึกษาทั้งหมด
            var Student = DB.UserRoles.Where(w => w.RoleId == "e5ce49ea-eaf4-431e-b7c6-50ac72ff505b").Select(s => s.UserId).Count();
            //นักศึกษาที่สมัครงาน
            var Register = DB.TRANSACTION_REGISTER.Select(s => s.transaction_register_id).Count();

            //จำนวนนักศึกษาที่สมัครงานกับหน่วยงานของมหาลัย
            var Job = DB.TRANSACTION_JOB.Where(w => w.type_job_id == 1).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterOffice =  DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == Job).Count();
            //คณะศิลปศาสตร์
            var JobLib = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 1).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterLib = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobLib).Count();
            //คณะครุ
            var JobTech = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 2).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterTech = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobTech).Count();
            //คณะการเกษตร
            var JobAgr = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 3).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterAgr = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobAgr).Count();
            //คณะวิศวะ
            var JobEngineer = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 4).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterEngineer = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobEngineer).Count();
            //คณะบริหาร
            var JobBus = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 5).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterBus = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobBus).Count();
            //คณะคหกรรม
            var JobHet = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 6).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterHet = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobHet).Count();
            //คณะศิลปกรรม
            var JobFa = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 7).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterFa = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobFa).Count();
            //คณะสื่อสาร
            var JobMass = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 8).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterMass = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobMass).Count();
            //คณะวิทย์
            var JobSci = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 9).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterSci = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobSci).Count();
            //คณะสถาปัต
            var JobArch = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 10).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterArch = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobArch).Count();
            //คณะเเพทย์
            var JobIm = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 11).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterIm = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobIm).Count();
            //คณะพยาบาล
            var JobNurse = DB.TRANSACTION_JOB.Where(w => w.faculty_id == 12).Select(s => s.transaction_job_id).FirstOrDefault();
            var RegisterNurse = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == JobNurse).Count();


            //Chart 1
            ViewBag.Student = Student;
            ViewBag.Register = Register;

            //Chart 2
            ViewBag.Office = RegisterOffice;
            ViewBag.Lib = RegisterLib;
            ViewBag.Tech = RegisterTech;
            ViewBag.Agr = RegisterAgr;
            ViewBag.Engineer = RegisterEngineer;
            ViewBag.Bus = RegisterBus;
            ViewBag.Het = RegisterHet;
            ViewBag.Fa = RegisterFa;
            ViewBag.Mass = RegisterMass;
            ViewBag.Sci = RegisterSci;
            ViewBag.Arch = RegisterArch;
            ViewBag.Im = RegisterIm;
            ViewBag.Nurse = RegisterNurse;

            return View("Index");
        }

        #endregion

        #region นักศึกษาที่สมัครงานทั้งหมดในมหาวิทยาลัย

        //ข้อมมูลนศ.ที่สมัครงาน
        public IActionResult AllListStudent()
        {
            var Id = DB.TRANSACTION_REGISTER.Where(w => w.status_id == 7).Select(s => s.transaction_register_id).FirstOrDefault();
            ViewBag.id = Id;

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
                    foreach (var r in GetPerson.Where(w => w.transaction_job_id == j.transaction_job_id))
                    {
                        foreach (var item in GetStatus.Where(w => w.status_id == r.status_id))
                        {
                            if (item.status_id == 7 || item.status_id == 6 || item.status_id == 5)
                            {
                                var Model = new AllListStudentRegister();
                                var P = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.prefix_id).FirstOrDefault();
                                var FirstName = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.FirstName).FirstOrDefault();
                                var LastName = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.LastName).FirstOrDefault();
                                var Prefix = DB.MASTER_PREFIX.Where(w => w.prefix_id == P).Select(s => s.prefix_name).FirstOrDefault();

                                Model.id = r.transaction_register_id;
                                Model.student_name = Prefix + "" + FirstName + "" + LastName;
                                Model.student_id = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.UserName).FirstOrDefault();
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
            var r = DB.TRANSACTION_REGISTER.FirstOrDefault();
            var P = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.prefix_id).FirstOrDefault();
            var FirstName = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.FirstName).FirstOrDefault();
            var LastName = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.LastName).FirstOrDefault();
            var Prefix = DB.MASTER_PREFIX.Where(w => w.prefix_id == P).Select(s => s.prefix_name).FirstOrDefault();
            var S_id = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.UserName).FirstOrDefault();

            ViewBag.bank = GetBank.Where(w => w.banktype_id == Get.banktype_id).Select(s => s.banktype_name).FirstOrDefault();
            ViewBag.Name = Prefix + "" + FirstName + "" + LastName;
            ViewBag.StudentId = S_id;
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

                Get.UserId = model.UserId;
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

                Get.UserId = model.UserId;
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

        //อนุมัตินักศึกษาที่สมัครงานทั้งหมด
        public async Task<IActionResult> AllApprove(int id)
        {
            try
            {
                var latestId = await DB.TRANSACTION_REGISTER.MaxAsync(x => x.transaction_register_id);

                for (int i = id; i <= latestId; i++)
                {
                    var data = await DB.TRANSACTION_REGISTER.FindAsync(i);

                    if (data.status_id != 7)
                    {
                        continue;
                    }

                    if (data.status_id == 7)
                    {
                        data.status_id = 5;
                        DB.TRANSACTION_REGISTER.Update(data);
                    }
                }

                await DB.SaveChangesAsync();
            }
            catch (Exception error)
            {
                return Json(new { valid = false, message = error });
            }
            return Json(new { valid = true, message = "อนุมัติการสมัครงานทั้งหมดสำเร็จ" });
        }


        #endregion

        #region รายชื่อนักศึกษาที่สมัครงาน

        //หน้ารายการนักศึกษาที่มาสมัครงาน
        public async Task<IActionResult> ListStudent()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            return View("ListStudent");
        }

        //ข้อมูลนักศึกษาที่มาสมัครงาน
        public async Task<IActionResult> getListStudent()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetPerson = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var GetBank = await DB.MASTER_BANK.ToListAsync();

            var Models = new List<ListStudentRegister>();

            foreach (var j in GetJob.Where(w => w.create_by == CurrentUser.UserName))
            {
                foreach (var r in GetPerson.Where(w => w.transaction_job_id == j.transaction_job_id))
                {
                    foreach (var s in GetStatus.Where(w => w.status_id == r.status_id))
                    {
                        foreach (var b in GetBank.Where(w => w.banktype_id == r.banktype_id))
                        {
                            var model = new ListStudentRegister();
                            var P = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.prefix_id).FirstOrDefault();
                            var FirstName = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.FirstName).FirstOrDefault();
                            var LastName = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.LastName).FirstOrDefault();
                            var Prefix = DB.MASTER_PREFIX.Where(w => w.prefix_id == P).Select(s => s.prefix_name).FirstOrDefault();

                            model.id = r.transaction_register_id;
                            model.student_name = Prefix + "" + FirstName + "" + LastName;
                            model.job_name = j.job_name;
                            model.s_id = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.UserName).FirstOrDefault();
                            model.register_date = r.register_date;
                            model.status_name = s.status_name;
                            model.because_working = r.because_job;
                            model.file = r.bank_file;
                            model.bank = r.bank_no + "(" + b.banktype_name + ")";
                            Models.Add(model);
                        }
                    }
                }
            }
            return PartialView("getListStudent", Models);
        }

        //หน้าฟอร์มการตรวจสอบ
        public IActionResult CheckRegister(int transaction_register_id)
        {
            var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == transaction_register_id).FirstOrDefault();
            var GetBank = DB.MASTER_BANK.ToList();
            var r = DB.TRANSACTION_REGISTER.FirstOrDefault();
            var P = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.prefix_id).FirstOrDefault();
            var FirstName = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.FirstName).FirstOrDefault();
            var LastName = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.LastName).FirstOrDefault();
            var Prefix = DB.MASTER_PREFIX.Where(w => w.prefix_id == P).Select(s => s.prefix_name).FirstOrDefault();
            var S_id = DB.Users.Where(w => w.Id == r.UserId).Select(s => s.UserName).FirstOrDefault();

            ViewBag.bank = GetBank.Where(w => w.banktype_id == Get.banktype_id).Select(s => s.banktype_name).FirstOrDefault();
            ViewBag.Name = Prefix + "" + FirstName + "" + LastName;
            ViewBag.StudentId = S_id;

            return View("CheckRegister", Get);
        }

        //ดาวน์โหลดไฟล์
        public FileResult Download2(string Name)
        {
            string path = Path.Combine(_environment.WebRootPath, "uploads/bookbank/") + Name;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", Name);
        }

        //ส่งอนุมัติ ส่งฝ่ายพัฒนานักศึกษา
        [HttpPost]
        public async Task<IActionResult> Approve2(TRANSACTION_REGISTER model)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == model.transaction_register_id).FirstOrDefault();
                var CGet = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == model.transaction_job_id).Select(s => s.transaction_register_id).Count();
                var CJob = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == model.transaction_job_id).Select(s => s.amount_person).FirstOrDefault();

                //เช็คว่ามีอนุมัติครบตามจำนวนนักศึกษาที่ต้องการหรือยัง ถ้าเกินจะไม่สามารถบันทึกได้
                if (CGet > CJob)
                {
                    return Json(new { valid = false, message = "ไม่สามารถส่งอนุมัติได้ เนื่องจากส่งรายชื่อครบเเล้ว" });
                }

                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ
                if (Get.status_id == 5 || Get.status_id == 6 || Get.status_id == 7 || Get.status_id == 9)
                {
                    return Json(new { valid = false, message = "ไม่สามารถส่งอนุมัติได้ !!!" });
                }
                else if (Get.status_id == 8)
                {
                    Get.UserId = model.UserId;
                    Get.bank_file = model.bank_file;
                    Get.bank_no = model.bank_no;
                    Get.bank_store = model.bank_store;
                    Get.because_job = model.because_job;
                    Get.register_date = model.register_date;
                    Get.status_id = 9;
                    Get.approve_teacher_date = DateTime.Now;
                    DB.TRANSACTION_REGISTER.Update(Get);
                    await DB.SaveChangesAsync();
                }
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "ส่งอนุมัติสำเร็จ" });
        }

        //ไม่อนุมัติ
        [HttpPost]
        public async Task<IActionResult> NotApprove2(TRANSACTION_REGISTER model)
        {
            try
            {
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == model.transaction_register_id).FirstOrDefault();

                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ  
                if (Get.status_id == 5 || Get.status_id == 6 || Get.status_id == 7)
                {
                    return Json(new { valid = false, message = "ไม่สามารถไม่อนุมัติได้ !!!" });
                }

                Get.UserId = model.UserId;
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
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "ไม่อนุมัติสำเร็จ" });
        }
        #endregion

        #region ขอรับนักศึกษามาปฎิบัติงาน

        //หน้าเเสดงข้อมูลงาน
        public IActionResult Job()
        {
            return View("Job");
        }

        //ดึงข้อมูลงานที่สร้างมาเเสดง
        public async Task<IActionResult> getJob()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Gets = DB.TRANSACTION_JOB.Where(w => w.create_by == CurrentUser.UserName).ToList();

            return PartialView("getJob", Gets);
        }

        //ฟอร์มสร้างงาน
        public IActionResult FormAddJob()
        {
            ViewBag.Place = new SelectList(DB.MASTER_PLACE.ToList(), "place_id", "place_name");

            return View("FormAddJob");
        }

        //นำข้อมูลงานลงดาต้าเบส
        [HttpPost]
        public async Task<IActionResult> FormAddJob(TRANSACTION_JOB Model)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                //ถ้ามีชื่อซ้ำกันในดาต้าเบส ก็จะไม่สมารถบันทึกได้
                if (DB.TRANSACTION_JOB.Where(w => w.job_name == Model.job_name).Count() > 0)
                {
                    return Json(new { valid = false, message = "กรุณาตรวจสอบข้อมูล" });
                }

                //ถ้าวันที่ในการปิดรับสมัครเป็นวันที่ผ่านมาเเล้ว จะไม่สามารถบันทึกได้
                if (Model.close_register_date < DateTime.Now)
                {
                    return Json(new { valid = false, message = "วันที่ปิดรับสมัครไม่ถูกต้อง" });
                }

                Model.type_job_id = 2;
                Model.faculty_id = CurrentUser.faculty_id;
                Model.branch_id = CurrentUser.branch_id;
                Model.create_by = CurrentUser.UserName;
                Model.update_date = DateTime.Now;
                Model.create_date = DateTime.Now;
                DB.TRANSACTION_JOB.Add(Model);
                await DB.SaveChangesAsync();

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "บันทึกข้อมูลสำเร็จ" });
        }

        //ฟอร์มเเก้ไขงาน
        public IActionResult FormEditJob(int transaction_job_id)
        {
            var Get = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == transaction_job_id).FirstOrDefault();
            ViewBag.Place = new SelectList(DB.MASTER_PLACE.ToList(), "place_id", "place_name");

            return View("FormEditJob", Get);
        }

        //นำข้อมูลที่เเก้ลงดาต้าเบส
        [HttpPost]
        public async Task<IActionResult> FormEditJob(TRANSACTION_JOB Model)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                var Get = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == Model.transaction_job_id).FirstOrDefault();

                //ถ้าวันที่ในการปิดรับสมัครเป็นวันที่ผ่านมาเเล้ว จะไม่สามารถบันทึกได้
                if (Model.close_register_date < DateTime.Now)
                {
                    return Json(new { valid = false, message = "วันที่ปิดรับสมัครไม่ถูกต้อง" });
                }

                Get.job_name = Model.job_name;
                Get.place_id = Model.place_id;
                Get.job_detail = Model.job_detail;
                Get.amount_date = Model.amount_date;
                Get.amount_person = Model.amount_person;
                Get.close_register_date = Model.close_register_date;
                Get.faculty_id = CurrentUser.faculty_id;
                Get.branch_id = CurrentUser.branch_id;
                Get.update_date = DateTime.Now;
                Get.create_by = CurrentUser.UserName;
                Get.create_date = Model.create_date;
                Get.type_job_id = Model.type_job_id;
                DB.TRANSACTION_JOB.Update(Get);
                await DB.SaveChangesAsync();

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "บันทึกข้อมูลสำเร็จ" });
        }

        //ลบงาน
        public async Task<IActionResult> DeleteJob(int transaction_job_id)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                var Get = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == transaction_job_id).FirstOrDefault();
                var Check = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == transaction_job_id).Select(s => s.transaction_register_id).Count() > 0;
                if (Check == true)
                {
                    return Json(new { valid = false, message = "ไม่สามารถลบรายการงานนี้ได้" });
                }

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
        public IActionResult ListStudentWorking()
        {
            return View("ListStudentWorking");
        }
        public async Task<IActionResult> getListStudentWorking()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetRegis = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetPrefix = await DB.MASTER_PREFIX.ToListAsync();
            var GetUser = await DB.Users.ToListAsync();
            var GetWorking = await DB.TRANSACTION_WORKING.ToListAsync();

            var Models = new List<ListWorking>();

            foreach (var j in GetJob.Where(w => w.create_by == CurrentUser.UserName))
            {
                foreach (var r in GetRegis.Where(w => w.transaction_job_id == j.transaction_job_id && w.status_id == 5))
                {
                    foreach (var u in GetUser.Where(w => w.Id == r.UserId))
                    {

                        foreach (var p in GetPrefix.Where(w => w.prefix_id == u.prefix_id))
                        {
                            var wk = GetWorking.Where(w => w.transaction_register_id == r.transaction_register_id && w.transaction_job_id == j.transaction_job_id).Select(s => s.transaction_working_id).Count();
                            var data = new ListWorking();

                            if (wk != j.amount_date)
                            {
                                data.Id = r.transaction_register_id;
                                data.j_Id = r.transaction_job_id;
                                data.fullname = p.prefix_name + "" + u.FirstName + "" + u.LastName;
                                data.jobname = j.job_name;
                                data.s_id = u.UserName;
                                data.status = "ยังออกเอกสารไม่ได้เนื่องจากยังทำงานไม่ครบ";
                                data.approve = r.approve_date;
                                Models.Add(data);
                            }
                            else if (wk == j.amount_date)
                            {
                                data.Id = r.transaction_register_id;
                                data.j_Id = r.transaction_job_id;
                                data.fullname = p.prefix_name + "" + u.FirstName + "" + u.LastName;
                                data.jobname = j.job_name;
                                data.s_id = u.UserName;
                                data.status = "ออกเอกสารได้";
                                data.approve = r.approve_date;
                                Models.Add(data);
                            }
                        }
                    }
                }
            }

            return PartialView("getListStudentWorking", Models);
        }

        public IActionResult DetailWorking(int transaction_register_id)
        {
            var GetWorking = DB.TRANSACTION_WORKING.ToList();
            var GetStatus = DB.MASTER_STATUS_WORKING.ToList();

            var Models = new List<DetailWorking>();

            foreach (var wk in GetWorking.Where(w => w.transaction_register_id == transaction_register_id))
            {
                foreach (var s in GetStatus.Where(w => w.status_working_id == wk.status_working_id))
                {
                    var data = new DetailWorking();
                    string d = wk.start_work.ToString("yyyy-MM-dd");
                    string time_in = wk.start_work.ToString("HH:mm");
                    string time_out = wk.end_work.ToString("HH:mm");
                    string time_out_correct = wk.end_work_correct.ToString("HH:mm");

                    if (s.status_working_id == 2)
                    {
                        data.Id = wk.transaction_working_id;
                        data.date = d;
                        data.check_in = time_in;
                        data.check_out = "00:00";
                        data.check_out_correct = time_out_correct;
                        data.file_in = wk.file_work_start;
                        data.file_out = wk.file_work_end;
                        data.status = DB.MASTER_STATUS.Where(w => w.status_id == wk.status_id).Select(s => s.status_name).FirstOrDefault();
                        data.status_of_working = s.status_working_name;
                        data.laitude_in = wk.latitude_start;
                        data.longitude_in = wk.longitude_start;
                        data.laitude_out = wk.latitude_end;
                        data.longitude_out = wk.longitude_end;
                        Models.Add(data);
                    }
                    else if (s.status_working_id == 3)
                    {
                        data.Id = wk.transaction_working_id;
                        data.date = d;
                        data.check_in = time_in;
                        data.check_out = time_out;
                        data.check_out_correct = time_out_correct;
                        data.file_in = wk.file_work_start;
                        data.file_out = wk.file_work_end;
                        data.status = DB.MASTER_STATUS.Where(w => w.status_id == wk.status_id).Select(s => s.status_name).FirstOrDefault();
                        data.status_of_working = s.status_working_name;
                        data.laitude_in = wk.latitude_start;
                        data.longitude_in = wk.longitude_start;
                        data.laitude_out = wk.latitude_end;
                        data.longitude_out = wk.longitude_end;
                        Models.Add(data);
                    }
                }
            }

            return PartialView("DetailWorking", Models);
        }

        //ดาวน์โหลดหลักฐานการเข้างาน
        public FileResult Download3(string Name)
        {
            string path = Path.Combine(_environment.WebRootPath, "uploads/file_start_working/") + Name;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", Name);
        }

        //ดาวน์โหลดหลักฐานการทำงาน
        public FileResult Download4(string Name)
        {
            string path = Path.Combine(_environment.WebRootPath, "uploads/file_end_working") + Name;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream");
        }

        public IActionResult Pass(int id)
        {
            try
            {
                var GetWorking = DB.TRANSACTION_WORKING.Where(w => w.transaction_working_id == id).FirstOrDefault();

                if (GetWorking.status_working_id == 2)
                {
                    return Json(new { valid = false, message = "ไม่สามารถเปลี่ยนผลการตรวจสอบเป็นผ่านได้" });
                }

                if (GetWorking.status_id == 1)
                {
                    return Json(new { valid = false, message = "คุณได้ทำการให้งานนี้ผ่านไปเเล้ว" });
                }

                GetWorking.status_id = 1;
                DB.TRANSACTION_WORKING.Update(GetWorking);
                DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "เปลี่ยนผลการตรวจสอบเป็นผ่านสำเร็จ" });
        }

        public IActionResult Failed(int id)
        {
            try
            {
                var GetWorking = DB.TRANSACTION_WORKING.Where(w => w.transaction_working_id == id).FirstOrDefault();

                if (GetWorking.status_working_id == 2)
                {
                    return Json(new { valid = false, message = "ไม่สามารถเปลี่ยนผลการตรวจสอบเป็นไม่ผ่านได้" });
                }

                if (GetWorking.status_id == 2)
                {
                    return Json(new { valid = false, message = "คุณได้ทำการให้งานนี้ไม่ผ่านไปเเล้ว" });
                }

                GetWorking.status_id = 2;
                DB.TRANSACTION_WORKING.Update(GetWorking);
                DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error });
            }
            return Json(new { valid = true, message = "เปลี่ยนผลการตรวจสอบเป็นไม่ผ่านสำเร็จ" });
        }

        #endregion

        #region เอกสารเบิกจ่ายค่าตอบเเทน

        public IActionResult ProofPayment()
        {
            return View("ProofPayment");
        }

        public async Task<IActionResult> getProofPayment()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Gets = await DB.TRANSACTION_JOB.Where(w => w.create_by == CurrentUser.UserName).ToListAsync();
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetRegister = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetWorking = await DB.TRANSACTION_WORKING.ToListAsync();

            var Models = new List<Proofpayment>();

            foreach (var j in GetJob.Where(w => w.create_by == CurrentUser.UserName))
            {
                foreach (var r in GetRegister.Where(w => w.transaction_job_id == j.transaction_job_id))
                {
                    foreach (var wk in GetWorking.Where(w => w.transaction_register_id == r.transaction_register_id))
                    {
                        var check = GetWorking.Where(w => w.transaction_job_id == j.transaction_job_id && w.transaction_register_id == r.transaction_register_id && w.status_working_id == 3).Select(s => s.transaction_working_id).Count();
                        var check2 = GetWorking.Where(w => w.transaction_job_id == j.transaction_job_id && w.transaction_register_id == r.transaction_register_id && w.status_working_id == 3).Select(s => s.transaction_register_id).Count();

                        var check3 = j.amount_date * j.amount_person;
                        var check4 = check * check2;

                        var data = new Proofpayment();

                        //เช็คจำนวนวันที่ทำงานของนักศึกษาว่าเท่ากับตามวันที่ต้องทำไหม เเละเช็คว่าจำนวนนักศึกษาเท่ากับจำนวนที่รับไหม
                        if (check3 != check4)
                        {
                            data.Id = j.transaction_job_id;
                            data.Job_name = j.job_name;
                            data.status_name = "ยังไม่สามารถออกเอกสารได้";
                            Models.Add(data);
                        }
                        else if (check3 == check4)
                        {
                            data.Id = j.transaction_job_id;
                            data.Job_name = j.job_name;
                            data.status_name = "สามารถออกเอกสารได้";
                            Models.Add(data);
                        }
                    }
                }
            }

            return PartialView("getProofPayment", Models);
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
