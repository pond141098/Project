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
using SeniorProject.ViewModels.OfficerFaculty;
using SeniorProject.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Controllers;
using WebApplication.Data;
using WebApplication.Models;
using Windows.System;

namespace SeniorProject.Controllers
{
    public class OfficerFacultyController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;
        private readonly IWebHostEnvironment _environment;

        public OfficerFacultyController(
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

        public IActionResult Index()
        {
            return View();
        }


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

            var Models = new List<ListStudentRegisterF>();

            foreach (var j in GetJob.Where(w => w.create_by == CurrentUser.UserName))
            {
                foreach (var r in GetPerson.Where(w => w.transaction_job_id == j.transaction_job_id))
                {
                    foreach (var s in GetStatus.Where(w => w.status_id == r.status_id))
                    {
                        foreach (var b in GetBank.Where(w => w.banktype_id == r.banktype_id))
                        {
                            var model = new ListStudentRegisterF();
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
        public FileResult Download(string Name)
        {
            string path = Path.Combine(_environment.WebRootPath, "uploads/bookbank/") + Name;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", Name);
        }

        //ส่งอนุมัติ ส่งฝ่ายพัฒนานักศึกษา
        [HttpPost]
        public async Task<IActionResult> Approve(TRANSACTION_REGISTER model)
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
        public async Task<IActionResult> NotApprove(TRANSACTION_REGISTER model)
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

                Model.type_job_id = 3;
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
                Get.type_job_id = 3;
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

            var Models = new List<ListWorkingF>();

            foreach (var j in GetJob.Where(w => w.create_by == CurrentUser.UserName))
            {
                foreach (var r in GetRegis.Where(w => w.transaction_job_id == j.transaction_job_id && w.status_id == 5))
                {
                    foreach (var u in GetUser.Where(w => w.UserName == r.student_id))
                    {

                        foreach (var p in GetPrefix.Where(w => w.prefix_id == u.prefix_id))
                        {
                            var wk = GetWorking.Where(w => w.transaction_register_id == r.transaction_register_id && w.transaction_job_id == j.transaction_job_id).Select(s => s.transaction_working_id).Count();
                            var data = new ListWorkingF();

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

            var Models = new List<DetailWorkingF>();

            foreach (var wk in GetWorking.Where(w => w.transaction_register_id == transaction_register_id))
            {
                foreach (var s in GetStatus.Where(w => w.status_working_id == wk.status_working_id))
                {
                    var data = new DetailWorkingF();
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
                        data.laitude_in = wk.latitude_start;
                        data.longitude_in = wk.longitude_start;
                        data.laitude_out = wk.latitude_end;
                        data.longitude_out = wk.longitude_end;
                        data.status = s.status_working_name;
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
                        data.laitude_in = wk.latitude_start;
                        data.longitude_in = wk.longitude_start;
                        data.laitude_out = wk.latitude_end;
                        data.longitude_out = wk.longitude_end;
                        data.status = s.status_working_name;
                        Models.Add(data);
                    }
                }
            }

            return PartialView("DetailWorking", Models);
        }

        //ดาวน์โหลดหลักฐานการเข้างาน
        public FileResult Download2(string Name)
        {
            string path = Path.Combine(_environment.WebRootPath, "uploads/file_start_working/") + Name;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", Name);
        }

        //ดาวน์โหลดหลักฐานการทำงาน
        public FileResult Download3(string Name)
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

            var Models = new List<ProofpaymentF>();

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

                        var data = new ProofpaymentF();

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
    }
}
