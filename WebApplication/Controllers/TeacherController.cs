﻿using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Server;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Engines;
using SeniorProject.Data;
using SeniorProject.Models;
using SeniorProject.ViewModels.Teacher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebApplication.Controllers;

namespace SeniorProject.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;
        private readonly IWebHostEnvironment _environment;

        public TeacherController(
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
        public IActionResult Index()
        {
            return View("Index");
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
                            model.s_id = r.s_id;
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
                if (CGet >= CJob)
                {
                    return Json(new { valid = false, message = "ไม่สามารถส่งอนุมัติได้ !!!" });
                }

                //เช็คว่าถ้าไม่ใช่ อนุมัติ หรือ ไม่อนุมัติ หรือ รออนุมัติ
                if (Get.status_id == 5 || Get.status_id == 6 || Get.status_id == 7 )
                {
                    return Json(new { valid = false, message = "ไม่สามารถส่งอนุมัติได้ !!!" });
                }
                else if (Get.status_id == 8)
                { 
                    Get.fullname = model.fullname;
                    Get.s_id = model.s_id;
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
                Get.s_id = model.s_id;
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
                if(Model.close_register_date < DateTime.Now)
                {
                    return Json(new { valid = false, message = "วันที่ปิดรับสมัครไม่ถูกต้อง" });
                }

                //ถ้าจำนวนวันที่ให้นักศึกษาทำงาน หรือ จำนวนนักศึกษาที่ต้องการ เท่ากับ 0 จะไม่สามารถบันทึกข้อมูลได้
                if(Model.amount_date == 0 || Model.amount_person == 0)
                {
                    return Json(new { valid = false, message = "จำนวนวันที่ให้นักศึกษาทำงาน หรือ จำนวนนักศึกษาที่ต้องการ เท่ากับ 0" });
                }

                Model.faculty_id = CurrentUser.faculty_id;
                Model.branch_id = CurrentUser.branch_id;
                Model.create_by = CurrentUser.UserName;
                Model.update_date = DateTime.Now;
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

                //ถ้าชื่อซ้ำกันจะไม่สามารถบันทึกได้
                if (Get.job_name == Model.job_name)
                {
                    return Json(new { valid = false, message = "ข้อมูลไม่ถูกต้อง กรุณาตรวจสอบดีๆ" });
                }

                //ถ้าวันที่ในการปิดรับสมัครเป็นวันที่ผ่านมาเเล้ว จะไม่สามารถบันทึกได้
                if (Model.close_register_date < DateTime.Now)
                {
                    return Json(new { valid = false, message = "วันที่ปิดรับสมัครไม่ถูกต้อง" });
                }

                //ถ้าจำนวนวันที่ให้นักศึกษาทำงาน หรือ จำนวนนักศึกษาที่ต้องการ เท่ากับ 0 จะไม่สามารถบันทึกข้อมูลได้
                if (Model.amount_date == 0 || Model.amount_person == 0)
                {
                    return Json(new { valid = false, message = "จำนวนวันที่ให้นักศึกษาทำงาน หรือ จำนวนนักศึกษาที่ต้องการ เท่ากับ 0" });
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
        public IActionResult CheckTime()
        {
            return View("CheckTime");
        }
        public IActionResult getCheckTime()
        {
            var Gets = DB.MASTER_BANK.ToList();
            return PartialView("GetCheckTime", Gets);
        }

        #endregion
    }
}
