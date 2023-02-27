using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SeniorProject.ViewModels.Student;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using System.Net.Http;
using Newtonsoft.Json;

namespace SeniorProject.Controllers
{
    public class StudentController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;
        private readonly IWebHostEnvironment _environment;

        public StudentController(
            //ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext db,
            IWebHostEnvironment environment)
        {
            //_logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            DB = db;
            _environment = environment;
        }

        #region หน้าหลัก
        public IActionResult Index()
        {
            return View("Index");
        }
        public async Task<IActionResult> Home()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Model = DB.TRANSACTION_JOB.ToList();
            var GetName = DB.Users.Where(w => w.Id == CurrentUser.Id).Select(s => s.FirstName).FirstOrDefault();
            var GetLastName = DB.Users.Where(w => w.Id == CurrentUser.Id).Select(s => s.LastName).FirstOrDefault();
            var GetPrefix = DB.MASTER_PREFIX.Where(w => w.prefix_id == CurrentUser.prefix_id).Select(s => s.prefix_name).FirstOrDefault();

            ViewBag.Name = GetName;
            ViewBag.LastName = GetLastName;
            ViewBag.Prefix = GetPrefix;

            return View("Home");
        }
        #endregion

        #region ประวัติส่วนตัว
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
        #endregion

        #region ประวัติการสมัครงาน
        public async Task<IActionResult> HistoryRegister()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var Gets = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var GetPlace = await DB.MASTER_PLACE.ToListAsync();
            var Model = new List<HistoryRegister>();

            foreach (var r in Gets.Where(w => w.student_id == CurrentUser.UserName))
            {
                foreach (var j in GetJob.Where(w => w.transaction_job_id == r.transaction_job_id && w.faculty_id == CurrentUser.faculty_id && w.branch_id == CurrentUser.branch_id))
                {
                    foreach (var p in GetPlace.Where(w => w.place_id == j.place_id))
                    {
                        foreach (var s in GetStatus.Where(w => w.status_id == r.status_id))
                        {
                            var model = new HistoryRegister();
                            model.Id = r.transaction_register_id;
                            model.name = j.job_name;
                            model.place = p.place_name;
                            model.detail = j.job_detail;
                            model.status = s.status_name;
                            model.status_id = r.status_id;
                            model.register_date = r.register_date;
                            model.amount_date_working = j.amount_date;
                            Model.Add(model);
                        }
                    }
                }
            }
            return PartialView("HistoryRegister", Model);
        }

        //ฟอร์มเเก้ไขการสมัครงาน
        public IActionResult FormEditRegister(int id)
        {
            var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == id).FirstOrDefault();

            ViewBag.Bank = new SelectList(DB.MASTER_BANK.ToList(), "banktype_id", "banktype_name");

            return View("FormEditRegister", Get);
        }

        //นำข้อมูลลงดาต้าเบส
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormEditRegister(TRANSACTION_REGISTER model, IFormFile bank_file)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                var Get = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == model.transaction_register_id).FirstOrDefault();

                //ถ้าสถานะเป็นอนุมัติไม่สามารถเเก้ไขได้
                if (model.status_id == 5)
                {
                    return Json(new { valid = true, message = "ไม่สามารถเเก้ไขข้อมูลการสมัครงานเนื่องจากสถานะเป็น อนุมัติ" });
                }

                //เป็นการลบไฟล์เก่าที่มีอยู่ เเล้วนำไฟล์ใหม่ไปบันทึกเเทน
                if (bank_file != null)
                {
                    var Uploads = Path.Combine(_environment.WebRootPath.ToString(), "uploads/bookbank");
                    var GetOldFile = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == model.transaction_register_id).Select(s => s.bank_file).FirstOrDefault();
                    if (GetOldFile != null)
                    {
                        string Oldfilepath = Path.Combine(Uploads, GetOldFile);
                        if (System.IO.File.Exists(Oldfilepath))
                        {
                            System.IO.File.Delete(Oldfilepath);
                        }
                    }
                    string newfile = ContentDispositionHeaderValue.Parse(bank_file.ContentDisposition).FileName.Trim('"');
                    string UniqueFileName = newfile.ToString();
                    using (var fileStream = new FileStream(Path.Combine(Uploads, UniqueFileName), FileMode.Create))
                    {
                        await bank_file.CopyToAsync(fileStream);
                    }

                    Get.bank_file = UniqueFileName;
                }

                Get.because_job = model.because_job;
                Get.banktype_id = model.banktype_id;
                Get.bank_no = model.bank_no;
                Get.bank_store = model.bank_store;
                Get.fullname = model.fullname;
                Get.register_date = model.register_date;
                Get.status_id = model.status_id;
                Get.student_id = model.student_id;
                Get.transaction_job_id = model.transaction_job_id;
                Get.notapprove_date = model.notapprove_date;
                Get.approve_date = model.approve_date;
                Get.approve_devstudent_date = model.approve_devstudent_date;
                Get.approve_teacher_date = model.approve_teacher_date;

                DB.TRANSACTION_REGISTER.Update(Get);
                await DB.SaveChangesAsync();

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return RedirectToAction("HistoryRegister", "Student");
        }

        //ลบการสมัครงาน
        public async Task<IActionResult> DeleteRegisterJob(int id)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            try
            {
                var Model = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == id).FirstOrDefault();
                var check = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == id).Select(s => s.status_id).FirstOrDefault();
                var check2 = DB.TRANSACTION_REGISTER.Where(w => w.transaction_register_id == id).Select(s => s.bank_file).FirstOrDefault();

                //ถ้าสถานะเท่ากับอนุมัติไม่สามารถลบได้
                if (check == 5)
                {
                    return Json(new { valid = false, message = "ไม่สามารถยกเลิกการสมัครงานได้เนื่องจากสถานะเป็น อนุมัติ" });
                }

                //ลบไฟล์สำเนาสมุดบัญชีธนาคารใน wwwroot
                string fullPath = Path.Combine(_environment.WebRootPath.ToString(), ("uploads/bookbank"), check2);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                DB.TRANSACTION_REGISTER.Remove(Model);
                await DB.SaveChangesAsync();
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "ยกเลิกการสมัครงานสำเร็จ" });
        }
        #endregion

        #region รายละเอียดงานที่รับสมัคร

        //หน้ารายละเอียดงาน
        public async Task<IActionResult> Job()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetPlace = await DB.MASTER_PLACE.ToListAsync();
            var GetUser = await DB.Users.ToListAsync();
            var GetPrefix = await DB.MASTER_PREFIX.ToListAsync();

            //การ join table โดยนำค่าที่ต้องการมาเเสดง มาใส่ใน ViewsModels เเล้วไปเเสดงในหน้า Views
            var model = new List<ListJob>();

            foreach (var j in  DB.TRANSACTION_JOB.Where(w => w.faculty_id == CurrentUser.faculty_id && w.branch_id == CurrentUser.branch_id))
            {
                foreach (var p in GetPlace.Where(w => w.place_id == j.place_id))
                {
                    foreach (var u in GetUser.Where(w => w.UserName == j.create_by))
                    {
                        foreach (var pr in GetPrefix.Where(w => w.prefix_id == u.prefix_id))
                        {
                            var Model = new ListJob();
                            var Check = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == j.transaction_job_id && CurrentUser.UserName == w.student_id).Count() < 1;
                            var regis = DB.TRANSACTION_REGISTER.Where(w => w.transaction_job_id == j.transaction_job_id).Select(s => s.transaction_register_id).Count();

                            //ถ้าวันที่ปิดรับสมัครเท่ากับหรือมากกว่าวันที่ปัจจุบัน เเละ ถ้าในตารางการสมัครงานมี ไอดีงาน เเละ ไอดีผู้สมัครอยู่เเล้วเป็นจริง ให้ทำกรบันทึกข้อมูลลง Viewmodel
                            if (j.close_register_date >= DateTime.Now && Check == true)
                            {
                                Model.id = j.transaction_job_id;
                                Model.jobname = j.job_name;
                                Model.job_owner_name = pr.prefix_name + "" + u.FirstName + " " + u.LastName;
                                Model.job_detail = j.job_detail + " ณ " + p.place_name;
                                Model.amount_person = j.amount_person;
                                Model.amount_working = j.amount_date;
                                Model.close_register = j.close_register_date;
                                Model.amount_register = regis;
                                model.Add(Model);
                            }
                        }
                    }
                }

            }
            return PartialView("Job", model);
        }

        public IActionResult DetailJob(int transaction_job_id)
        {
            var Get = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == transaction_job_id).FirstOrDefault();
            var GetPlace = DB.MASTER_PLACE.Where(w => w.place_id == Get.place_id).Select(s => s.place_name).FirstOrDefault();

            ViewBag.Place = GetPlace;

            return PartialView("DetailJob", Get);
        }

        //หน้าฟอร์มการสมัครงาน
        public IActionResult FormRegisterJob(int transaction_job_id)
        {
            var GetJobName = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == transaction_job_id).Select(s => s.job_name).FirstOrDefault();

            ViewBag.Bank = new SelectList(DB.MASTER_BANK.ToList(), "banktype_id", "banktype_name");
            ViewBag.jobname = GetJobName;

            return View("FormRegisterJob");
        }

        //นำข้อมูลใส่ดาต้าเบส
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormRegisterJob(TRANSACTION_REGISTER Model, IFormFile bank_file)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetRegister = await DB.TRANSACTION_REGISTER.ToListAsync();

            try
            {
                if (ModelState.IsValid)
                {
                    if (Model.banktype_id == 7 && Model.bank_no.Length < 12 || Model.bank_no.Length > 15)
                    {
                        return Json(new { valid = false, message = "เลขที่บัญชีธนาคารออมสินไม่ถูกต้อง" });
                    }

                    if (Model.banktype_id == 8 || Model.banktype_id == 9 && Model.bank_no.Length < 12)
                    {
                        return Json(new { valid = false, message = "เลขที่บัญชีธนาคาร ธอส. หรือ ธกส. ไม่ถูกต้อง" });
                    }

                    if (Model.banktype_id != 7 && Model.banktype_id != 8 && Model.banktype_id != 9 && Model.bank_no.Length < 10)
                    {
                        return Json(new { valid = false, message = "เลขที่บัญชีธนาคารไม่ถูกต้อง" });
                    }

                    //อัพโหลดไฟล์สำเนาบัญชีธนาคาร
                    var Uploads = Path.Combine(_environment.WebRootPath.ToString(), "uploads/bookbank/");
                    string file = ContentDispositionHeaderValue.Parse(bank_file.ContentDisposition).FileName.Trim('"');
                    string fileExtension = Path.GetExtension(file);
                    string UniqueFileName = file.ToString();

                    //เช็คนามสกุลไฟล์
                    if (fileExtension.ToLower() != ".pdf")
                    {
                        return Json(new { valid = false, message = "โปรดอัปโหลดไฟล์ที่เป็น PDF" });
                    }

                    using (var fileStream = new FileStream(Path.Combine(Uploads, UniqueFileName), FileMode.Create))
                    {
                        await bank_file.CopyToAsync(fileStream);
                    }

                    Model.status_id = 8;
                    Model.bank_file = UniqueFileName;
                    Model.register_date = DateTime.Now;
                    Model.fullname = CurrentUser.FirstName + " " + CurrentUser.LastName;
                    Model.student_id = CurrentUser.UserName;
                    DB.TRANSACTION_REGISTER.Add(Model);
                    await DB.SaveChangesAsync();
                }
            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "สมัครงานสำเร็จ" });
        }

        #endregion

        #region งานที่ได้รับอนุมัติ

        //งานที่ได้รับการอนุมัติ
        public async Task<IActionResult> JobApprove()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetRegister = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS.ToListAsync();
            var GetPlace = await DB.MASTER_PLACE.ToListAsync();
            var GetUser = await DB.Users.ToListAsync();
            var GetPrefix = await DB.MASTER_PREFIX.ToListAsync();
            var GetWorking = await DB.TRANSACTION_WORKING.ToListAsync();

            var models = new List<ListJobApprove>();


            foreach (var r in GetRegister.Where(w => w.student_id == CurrentUser.UserName))
            {
                foreach (var j in GetJob.Where(w => w.transaction_job_id == r.transaction_job_id && w.faculty_id == CurrentUser.faculty_id && w.branch_id == CurrentUser.branch_id))
                {
                    foreach (var s in GetStatus.Where(w => w.status_id == r.status_id))
                    {
                        foreach (var p in GetPlace.Where(w => w.place_id == j.place_id))
                        {
                            foreach (var u in GetUser.Where(w => w.UserName == j.create_by))
                            {
                                foreach (var pre in GetPrefix.Where(w => w.prefix_id == u.prefix_id))
                                {
                                    var data = new ListJobApprove();

                                    //เช็คว่าในตารางการทำงาน โดยการนับไอดีว่าเท่ากับจำนวนวันที่ต้องทำงานเเละสถานะของงานต้องเป็นทำงานเสร็จเเล้ว
                                    var check = DB.TRANSACTION_WORKING.Where(w => w.transaction_job_id == j.transaction_job_id && w.transaction_register_id == r.transaction_register_id && w.status_working_id == 3).Select(s => s.transaction_working_id).Count() == j.amount_date;

                                    //ถ้าสถานะการสมัครงานเท่ากับอนุมัติ
                                    if (r.status_id == 5 && check != true)
                                    {
                                        data.id = r.transaction_register_id;
                                        data.j_id = r.transaction_job_id;
                                        data.job_name = j.job_name;
                                        data.job_owner = pre.prefix_name + "" + u.FirstName + " " + u.LastName;
                                        data.job_detail = j.job_detail + " ณ " + p.place_name;
                                        data.job_status = s.status_name;
                                        data.confirm_approve = r.approve_date;
                                        models.Add(data);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return PartialView("JobApprove", models);
        }

        #endregion

        #region ลงเวลาการทำงาน

        //รายการวันที่ทำงาน
        public async Task<IActionResult> ListWorking(int j_id, int id)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetWorking = await DB.TRANSACTION_WORKING.ToListAsync();
            var GetRegis = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS_WORKING.ToListAsync();

            ViewBag.job = j_id;
            ViewBag.register = id;

            DateTime dateTime = DateTime.Now;

            var Models = new List<HistoryWorking>();



            foreach (var r in GetRegis.Where(w => w.transaction_register_id == id && w.student_id == CurrentUser.UserName))
            {
                foreach (var j in GetJob.Where(w => w.transaction_job_id == j_id))
                {
                    foreach (var jwk in GetWorking.Where(w => w.transaction_register_id == r.transaction_register_id && w.transaction_job_id == j.transaction_job_id))
                    {
                        foreach (var s in GetStatus.Where(w => w.status_working_id == jwk.status_working_id))
                        {
                            var model = new HistoryWorking();
                            string check_out = jwk.end_work.ToString("0000-00-00 00:00:00");
                            string check_in = jwk.start_work.ToString("yyyy-MM-dd HH:mm:ss");
                            string check_out2 = jwk.end_work.ToString("yyyy-MM-dd HH:mm:ss");

                            if (s.status_working_id == 2)
                            {
                                model.Id = jwk.transaction_working_id;
                                model.job_name = j.job_name;
                                model.status_name = s.status_working_name;
                                model.check_in = check_in;
                                model.check_out = check_out;
                                Models.Add(model);
                            }
                            else if (s.status_working_id == 3)
                            {
                                model.Id = jwk.transaction_working_id;
                                model.job_name = j.job_name;
                                model.status_name = s.status_working_name;
                                model.check_in = check_in;
                                model.check_out = check_out2;
                                Models.Add(model);
                            }
                        }
                    }

                }
            }


            return PartialView("ListWorking", Models);
        }

        //ฟอร์มลงเวลาการเริ่มทำงาน
        public IActionResult FormStartWorking(int transaction_register_id, int transaction_job_id)
        {
            ViewBag.rid = transaction_register_id;
            ViewBag.jid = transaction_job_id;

            return View("FormStartWorking");
        }

        //บันทึกข้อมูลลงดาต้าเบส
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormStartWorking(TRANSACTION_WORKING Model, IFormFile file_start)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetRegis = await DB.TRANSACTION_REGISTER.FirstOrDefaultAsync();
            var GetJob = await DB.TRANSACTION_JOB.FirstOrDefaultAsync();
            var GetWork = await DB.TRANSACTION_WORKING.FirstOrDefaultAsync();

            //var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={Model.latitude_start},{Model.longitude_start}&key=YOUR_API_KEY";
            //var client = new HttpClient();
            //var response = await client.GetAsync(url);
            //var result = await response.Content.ReadAsAsync<GoogleMapsResult>();
            //var address = result.Results.FirstOrDefault()?.FormattedAddress;

            DateTime CurrentDate = DateTime.Now.Date;

            try
            {
                //ถ้ามีรายการงานเท่ากับจำนวนวันที่ต้องทำงาน ต้องไม่สามารถบันทึกได้
                var amount = DB.TRANSACTION_JOB.Where(w => w.transaction_job_id == Model.transaction_job_id).Select(s => s.amount_date).FirstOrDefault();
                var check2 = DB.TRANSACTION_WORKING.Where(w => w.transaction_register_id == Model.transaction_register_id && w.transaction_job_id == Model.transaction_job_id).Select(s => s.transaction_working_id).Count();

                if (check2 >= amount)
                {
                    return Json(new { valid = false, message = "ไม่สามารถลงเวลาเข้างานได้ เนื่องจากลงเวลาเข้างานไปเเล้วในวันนี้" });
                }

                //ถ้าผู้ใช้ระบบได้ทำการลงเวลาเข้างานไปเเล้วในวันนี้ จะไม่สามารถลงเวลาในงานอื่นๆ ได้อีก = ให้ทำการลงเวลาทำงานได้เเค่วันละ1ครั้ง
                var check = DB.TRANSACTION_WORKING.Where(w => w.transaction_register_id == Model.transaction_register_id && w.transaction_job_id == Model.transaction_job_id && w.start_work.Date == CurrentDate).Select(s => s.transaction_working_id).Count() > 0;

                if (check == true)
                {
                    return Json(new { valid = false, message = "ไม่สามารถลงเวลาเข้างานได้ เนื่องจากลงเวลาเข้างานไปเเล้วในวันนี้" });
                }

                //อัพโหลดไฟล์ในการเริ่มทำงาน
                var Uploads = Path.Combine(_environment.WebRootPath.ToString(), "uploads/file_start_working/");
                string file = ContentDispositionHeaderValue.Parse(file_start.ContentDisposition).FileName.Trim('"');
                string UniqueFileName = file.ToString();

                using (var fileStream = new FileStream(Path.Combine(Uploads, UniqueFileName), FileMode.Create))
                {
                    await file_start.CopyToAsync(fileStream);
                }

                Model.start_work = DateTime.Now;
                Model.end_work = DateTime.Now;
                Model.status_working_id = 2;
                Model.status_id = 10;
                Model.file_work_start = UniqueFileName;
                DB.TRANSACTION_WORKING.Add(Model);
                await DB.SaveChangesAsync();

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "ลงเวลาเข้างานสำเร็จ" });
        }

        //ฟอร์มออกการทำงาน
        public IActionResult FormEndWorking(int transaction_working_id)
        {
            var Gets = DB.TRANSACTION_WORKING.Where(w => w.transaction_working_id == transaction_working_id).FirstOrDefault();

            return View("FormEndWorking", Gets);
        }

        //บันทึกข้อมูลลงดาต้าเบส
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormEndWorking(TRANSACTION_WORKING Model, IFormFile file_end)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            try
            {
                var Get = await DB.TRANSACTION_WORKING.Where(w => w.transaction_working_id == Model.transaction_working_id).FirstOrDefaultAsync();

                if (Get.status_working_id == 3)
                {
                    return Json(new { valid = false, message = "ไม่สามารถลงเวลาออกงานได้เนื่องจากสถานะงานเป็น ทำงานสำเร็จ" });
                }

                //อัพโหลดไฟล์สิ้นสุดงาน
                var Uploads = Path.Combine(_environment.WebRootPath.ToString(), "uploads/file_end_working/");
                string file = ContentDispositionHeaderValue.Parse(file_end.ContentDisposition).FileName.Trim('"');
                string UniqueFileName = file.ToString();

                using (var fileStream = new FileStream(Path.Combine(Uploads, UniqueFileName), FileMode.Create))
                {
                    await file_end.CopyToAsync(fileStream);
                }

                Get.detail_working = Model.detail_working;
                Get.start_work = Model.start_work;
                Get.end_work = DateTime.Now;
                Get.file_work_start = Model.file_work_start;
                Get.file_work_end = UniqueFileName;
                Get.status_working_id = 3;
                Get.transaction_job_id = Model.transaction_job_id;
                Get.transaction_register_id = Model.transaction_register_id;
                DB.TRANSACTION_WORKING.Update(Get);
                await DB.SaveChangesAsync();

            }
            catch (Exception Error)
            {
                return Json(new { valid = false, message = Error.Message });
            }
            return Json(new { valid = true, message = "ลงเวลาออกงานสำเร็จ" });
        }
        #endregion

        #region ประวัติการทำงาน
        public async Task<IActionResult> HistoryWorking()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetWorking = await DB.TRANSACTION_WORKING.ToListAsync();
            var GetRegis = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetStatus = await DB.MASTER_STATUS_WORKING.ToListAsync();

            var Models = new List<HistoryWorking>();


            foreach (var r in GetRegis.Where(w => w.student_id == CurrentUser.UserName))
            {
                foreach (var j in GetJob.Where(w => w.transaction_job_id == r.transaction_job_id))
                {
                    foreach (var wk in GetWorking.Where(w => w.transaction_register_id == r.transaction_register_id))
                    {
                        foreach (var s in GetStatus.Where(w => w.status_working_id == wk.status_working_id))
                        {
                            var model = new HistoryWorking();
                            string check_out = wk.end_work.ToString("0000-00-00 00:00:00");
                            string check_in = wk.start_work.ToString("yyyy-MM-dd HH:mm:ss");
                            string check_out2 = wk.end_work.ToString("yyyy-MM-dd HH:mm:ss");

                            if (s.status_working_id == 2)
                            {
                                model.Id = wk.transaction_working_id;
                                model.job_name = j.job_name;
                                model.status_name = s.status_working_name;
                                model.check_in = check_in;
                                model.check_out = check_out;
                                Models.Add(model);
                            }
                            else if (s.status_working_id == 3)
                            {
                                model.Id = wk.transaction_working_id;
                                model.job_name = j.job_name;
                                model.status_name = s.status_working_name;
                                model.check_in = check_in;
                                model.check_out = check_out2;
                                Models.Add(model);
                            }
                        }
                    }

                }
            }


            return PartialView("HistoryWorking", Models);
        }

        #endregion
    }
}
