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

            //Chart 2
            var Model = new List<dashboard>();

            foreach(var j in GetJob.Where(w => w.faculty_id == CurrentUser.faculty_id))
            {
                foreach(var b in GetBranch.Where(w => w.branch_id == j.branch_id))
                {
                    var data = new dashboard();
                    if(j.type_job_id == 3)
                    {
                        data.branchName = b.branch_name;
                        data.amount_register = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == j.transaction_job_id).Select(s => s.transaction_register_id).Count();
                        Model.Add(data);
                    }
                    else if(j.type_job_id == 2)
                    {
                        data.branchName = "หน่วยงานภายในคณะ";
                        data.amount_register = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == j.transaction_job_id).Select(s => s.transaction_register_id).Count();
                        Model.Add(data);
                    }
                }
            }

            //Chart 1
            ViewBag.Student = Student;
            ViewBag.Register = Register;

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
                            model.id = r.transaction_register_id;
                            model.student_name = r.fullname;
                            model.job_name = j.job_name;
                            model.s_id = r.student_id;
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

            ViewBag.bank = GetBank.Where(w => w.banktype_id == Get.banktype_id).Select(s => s.banktype_name).FirstOrDefault();

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
                    Get.fullname = model.fullname;
                    Get.student_id = model.student_id;
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
                    foreach (var u in GetUser.Where(w => w.UserName == r.student_id))
                    {

                        foreach (var p in GetPrefix.Where(w => w.prefix_id == u.prefix_id))
                        {
                            var wk = GetWorking.Where(w => w.transaction_register_id == r.transaction_register_id && w.transaction_job_id == j.transaction_job_id).Select(s => s.transaction_working_id).Count();
                            var data = new ListWorking();

                            if (wk != j.amount_date)
                            {
                                data.Id = r.transaction_register_id;
                                data.j_Id = r.transaction_job_id;
                                data.fullname = p.prefix_name + " " + r.fullname;
                                data.jobname = j.job_name;
                                data.s_id = r.student_id;
                                data.status = "ยังออกเอกสารไม่ได้เนื่องจากยังทำงานไม่ครบ";
                                data.approve = r.approve_date;
                                Models.Add(data);
                            }
                            else if (wk == j.amount_date)
                            {
                                data.Id = r.transaction_register_id;
                                data.j_Id = r.transaction_job_id;
                                data.fullname = p.prefix_name + " " + r.fullname;
                                data.jobname = j.job_name;
                                data.s_id = r.student_id;
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
                    string time_in = wk.start_work.ToString("HH:mm:ss");
                    string time_out = wk.end_work.ToString("HH:mm:ss");

                    if (s.status_working_id == 2)
                    {
                        data.Id = wk.transaction_working_id;
                        data.date = d;
                        data.check_in = time_in;
                        data.check_out = "00:00:00";
                        data.file_in = wk.file_work_start;
                        data.file_out = wk.file_work_end;
                        data.status = s.status_working_name;
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
                        data.file_in = wk.file_work_start;
                        data.file_out = wk.file_work_end;
                        data.status = s.status_working_name;
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
