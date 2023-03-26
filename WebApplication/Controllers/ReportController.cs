using iTextSharp.text.pdf;
using iTextSharp.text;
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
using System.Threading.Tasks;
using WebApplication.Controllers;
using WebApplication.Data;
using WebApplication.Models;
using Windows.System;
using FontFamily = iTextSharp.text.Font.FontFamily;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Windows.UI.Xaml.Documents;
using Paragraph = iTextSharp.text.Paragraph;
using Microsoft.Net.Http.Headers;
using Org.BouncyCastle.Asn1.X509;
using static Uno.UI.FeatureConfiguration;
using Windows.UI.Xaml;
using Uno.UI.Xaml;
using SeniorProject.ViewModels.Report;

namespace SeniorProject.Controllers
{
    public class ReportController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext DB;
        private readonly IWebHostEnvironment _environment;

        public ReportController(
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

        #region PDF

        [HttpGet]
        public async Task<IActionResult> ExportPDF(string strHeader)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            MemoryStream workStream = new MemoryStream();
            BaseFont bf = BaseFont.CreateFont(_environment.WebRootPath + "//fonts/THSarabun.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            //iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 0f, 0f, 0f, 0f);
            //pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
            Font FontNormal = new Font(bf, 14);
            Font FontNormalBold = new Font(bf, 14, Font.BOLD);
            Font FontMedium = new Font(bf, 14);
            Font FontBig = new Font(bf, 16);
            Font FontBigBold = new Font(bf, 16, Font.BOLD);
            iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 20f, 0f, 0f, 20f);
            doc.SetPageSize(iTextSharp.text.PageSize.A4);
            PdfWriter.GetInstance(doc, workStream).CloseStream = false;

            doc.Open();

            // Create a new table
            PdfPTable table = new PdfPTable(7);
            table.SetWidths(new float[] { 300, 600, 200, 200, 200, 200, 500 });
            table.HorizontalAlignment = Element.ALIGN_LEFT;

            //Header
            Paragraph title = new Paragraph("ใบลงเวลาปฏิบัติงานของนักศึกษา", FontNormalBold);
            title.Alignment = Element.ALIGN_CENTER;
            doc.Add(title);
            title = new Paragraph("โครงการสนับสนุนการหารายได้พิเศษระหว่างเรียนของนักศึกษา", FontNormalBold);
            title.Alignment = Element.ALIGN_CENTER;
            doc.Add(title);
            title = new Paragraph("หน่วยงาน ........................................................................................ มหาวิทยาลัยเทคโนโลยีราชมงคลธัญบุรี", FontNormalBold);
            title.Alignment = Element.ALIGN_CENTER;
            doc.Add(title);
            title = new Paragraph("ชื่อผู้ปฏิบัติงาน ...................................................................................................................", FontNormalBold);
            title.Alignment = Element.ALIGN_CENTER;
            doc.Add(title);
            title = new Paragraph("ประจำเดือน................................................................. พ.ศ...............", FontNormalBold);
            title.Alignment = Element.ALIGN_CENTER;
            doc.Add(title);
            doc.Add(new Phrase(""));

            // Add cells to the table
            PdfPCell cell = new PdfPCell(new Phrase("วัน เดือน ปี", FontNormal));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("รายละเอียดการปฏิบัติงาน", FontNormal));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("ลายมือชื่อ", FontNormal));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("เวลามา", FontNormal));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("ลายมือชื่อ", FontNormal));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("เวลากลับ", FontNormal));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("ปฏิบัติงานจำนวน", FontNormal));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            table.AddCell(new Phrase("1", FontNormal));
            table.AddCell(new Phrase("2", FontNormal));
            table.AddCell(new Phrase("3", FontNormal));
            table.AddCell(new Phrase("4", FontNormal));
            table.AddCell(new Phrase("5", FontNormal));
            table.AddCell(new Phrase("6", FontNormal));
            table.AddCell(new Phrase("7", FontNormal));
            doc.Add(table);

            //SignName
            PdfPTable SignName = new PdfPTable(4);
            SignName.SetWidths(new float[] { 100f, 100f, 850f, 500f });
            SignName.HorizontalAlignment = Element.ALIGN_LEFT;

            PdfPCell SN = new PdfPCell(new Phrase("", FontNormal));
            SN.Border = Rectangle.NO_BORDER;
            SN.HorizontalAlignment = 2;
            SignName.AddCell(SN);
            SN = new PdfPCell(new Phrase("(ลงชื่อ)", FontNormal));
            SN.Border = Rectangle.NO_BORDER;
            SN.HorizontalAlignment = 2;
            SignName.AddCell(SN);
            SN = new PdfPCell(new Phrase("............................................................................................................", FontNormal));
            SN.Border = Rectangle.NO_BORDER;
            SN.HorizontalAlignment = 1;
            SignName.AddCell(SN);
            SN = new PdfPCell(new Phrase("ผู้ควบคุมการปฏิบัติงาน", FontNormal));
            SN.Border = Rectangle.NO_BORDER;
            SN.HorizontalAlignment = 3;
            SignName.AddCell(SN);
            SN = new PdfPCell(new Phrase("", FontNormal));
            SN.Border = Rectangle.NO_BORDER;
            SN.HorizontalAlignment = 3;
            SignName.AddCell(SN);
            SN = new PdfPCell(new Phrase("", FontNormal));
            SN.Border = Rectangle.NO_BORDER;
            SN.HorizontalAlignment = 3;
            SignName.AddCell(SN);
            SN = new PdfPCell(new Phrase("(..........................................................................................................)", FontNormal));
            SN.Border = Rectangle.NO_BORDER;
            SN.HorizontalAlignment = 3;
            SignName.AddCell(SN);
            SN = new PdfPCell(new Phrase("", FontNormal));
            SN.Border = Rectangle.NO_BORDER;
            SN.HorizontalAlignment = 3;
            SignName.AddCell(SN);
            doc.Add(SignName);

            //FooterTable
            PdfPTable FooterTable = new PdfPTable(2);
            FooterTable.SetWidths(new float[] { 100f, 900f });
            FooterTable.HorizontalAlignment = Element.ALIGN_LEFT;

            PdfPCell celltext = new PdfPCell(new Phrase("หมายเหตุ", FontNormalBold));
            celltext.HorizontalAlignment = 3;
            celltext.Border = Rectangle.NO_BORDER;
            FooterTable.AddCell(celltext);
            celltext = new PdfPCell(new Phrase("ให้นักศึกษาลงลายมือชื่อและเวลาการปฏิบัติงานด้วยลายมือชื่อตนเองทุกครั้ง โดยให้นับเวลาการปฏิบัติงานดังนี้", FontNormal));
            celltext.Border = Rectangle.NO_BORDER;
            celltext.HorizontalAlignment = 3;
            FooterTable.AddCell(celltext);
            celltext = new PdfPCell(new Phrase("", FontNormal));
            celltext.HorizontalAlignment = 3;
            celltext.Border = Rectangle.NO_BORDER;
            FooterTable.AddCell(celltext);
            celltext = new PdfPCell(new Phrase("1. ให้นักศึกษาบันทึกรายละเอียดการปฏิบัติงานของนักศึกษาในแต่ละวันโดยละเอียด", FontNormal));
            celltext.Border = Rectangle.NO_BORDER;
            celltext.HorizontalAlignment = 3;
            FooterTable.AddCell(celltext);
            celltext = new PdfPCell(new Phrase("", FontNormal));
            celltext.HorizontalAlignment = 3;
            celltext.Border = Rectangle.NO_BORDER;
            FooterTable.AddCell(celltext);
            celltext = new PdfPCell(new Phrase("2. ให้ผู้ควบคุมการปฏิบัติงานที่ได้รับอนุมัติลงลายมือชื่อเพื่อยืนยันการปฏิบัติงานของนักศึกษา", FontNormal));
            celltext.Border = Rectangle.NO_BORDER;
            celltext.HorizontalAlignment = 3;
            FooterTable.AddCell(celltext);
            celltext = new PdfPCell(new Phrase("", FontNormal));
            celltext.HorizontalAlignment = 3;
            celltext.Border = Rectangle.NO_BORDER;
            FooterTable.AddCell(celltext);
            celltext = new PdfPCell(new Phrase("3. นักศึกษาปฏิบัติงานเต็มวัน จำนวน 7 ชั่วโมง ไม่รวมเวลาหยุดพัก ให้ได้รับค่าตอบแทน 300 บาท", FontNormal));
            celltext.Border = Rectangle.NO_BORDER;
            celltext.HorizontalAlignment = 3;
            FooterTable.AddCell(celltext);
            celltext = new PdfPCell(new Phrase("", FontNormal));
            celltext.HorizontalAlignment = 3;
            celltext.Border = Rectangle.NO_BORDER;
            FooterTable.AddCell(celltext);
            celltext = new PdfPCell(new Phrase("4. นักศึกษาปฏิบัติงานครึ่งวัน จำนวน 3 ชั่วโมงครึ่ง ให้ได้รับค่าตอบแทน 150 บาท", FontNormal));
            celltext.Border = Rectangle.NO_BORDER;
            celltext.HorizontalAlignment = 3;
            FooterTable.AddCell(celltext);
            celltext = new PdfPCell(new Phrase("", FontNormal));
            celltext.HorizontalAlignment = 3;
            celltext.Border = Rectangle.NO_BORDER;
            FooterTable.AddCell(celltext);
            doc.Add(FooterTable);
            doc.Add(new Phrase("                5. นักศึกษาปฏิบัติงานไม่เข้าตาม ข้อ1 และ 2 ให้ได้รับค่าตอบแทนชั่วโมงละ 40 บาท เศษของชั่วโมงให้ปัดทิ้งไม่นำมานับ", FontNormal));

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return File(workStream, "application/pdf");
            //"ExportPDF.pdf"
        }

        #endregion

        #region Excel
        public async Task<IActionResult> ExampleExcel(int status_id)
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var GetJob = await DB.TRANSACTION_JOB.ToListAsync();
            var GetRegister = await DB.TRANSACTION_REGISTER.ToListAsync();
            var GetUser = await DB.Users.ToListAsync();

            if (status_id == 0)
            {
                return Json(new { valid = false, message = "ไม่สามารถออกเอกสารได้" });
            }

            var Model = new List<Register>();
            int row = 1;

            foreach (var j in GetJob.Where(w => w.faculty_id == CurrentUser.faculty_id))
            {
                foreach (var r in GetRegister.Where(w => w.status_id == status_id && w.transaction_job_id == j.transaction_job_id))
                {
                    foreach (var u in GetUser.Where(w => w.Id == r.UserId))
                    {
                        var data = new Register();
                        var prefix = DB.MASTER_PREFIX.Where(w => w.prefix_id == u.prefix_id).Select(s => s.prefix_name).FirstOrDefault();
                        var Firstname = DB.Users.Where(w => w.Id == j.create_by).Select(s => s.FirstName).FirstOrDefault();
                        var LastName = DB.Users.Where(w => w.Id == j.create_by).Select(s => s.LastName).FirstOrDefault();
                        var GetPrefix = DB.Users.Where(w => w.Id == j.create_by).Select(s => s.prefix_id).FirstOrDefault();
                        var Pre = DB.MASTER_PREFIX.Where(w => w.prefix_id == GetPrefix).Select(s => s.prefix_name).FirstOrDefault();

                        data.Id = r.transaction_register_id;
                        data.Row = row;
                        data.Job_name = j.job_name;
                        data.Job_description = j.job_detail;
                        data.student_name = prefix + "" + u.FirstName + "" + u.LastName;
                        data.faculty = DB.MASTER_FACULTY.Where(w => w.faculty_id == CurrentUser.faculty_id).Select(s => s.faculty_name).FirstOrDefault();
                        data.student_id = u.UserName;
                        data.student_tel = r.tel_no;
                        data.Job_owner = Pre + "" + Firstname + "" + LastName;
                        data.Owner_tel = DB.Users.Where(w => w.Id == j.create_by).Select(s => s.PhoneNumber).FirstOrDefault();
                        data.bank_name = DB.MASTER_BANK.Where(w => w.banktype_id == r.banktype_id).Select(s => s.banktype_name).FirstOrDefault();
                        data.bank_branch = r.bank_store;
                        data.bank_no = r.bank_no;
                        Model.Add(data);
                        row++;
                    }
                }
            }

            // Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Create a new ExcelPackage
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                // Create a new worksheet
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("MySheet");

                // Add some data to the worksheet
                worksheet.Cells["A1:O1"].Merge = true;
                worksheet.Cells["A1:O1"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["A1:O1"].AutoFitColumns();
                worksheet.Cells["A1"].Value = "โครงการสนับสนุนการหารายได้พิเศษระหว่างเรียนของนักศึกษา ปีงบประมาณ";
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                

                worksheet.Cells["A2:A3"].Merge = true;
                worksheet.Cells["A2:A3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["A2"].Value = "ที่";
                worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["B2:B3"].Merge = true;
                worksheet.Cells["B2:B3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["B2:B3"].AutoFitColumns(26);
                worksheet.Cells["B2"].Value = "ชื่อหน่วยงาน (คณะ/วิทยาลัย/กอง/สำนักงาน)";
                worksheet.Cells["B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["B2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["C2:C3"].Merge = true;
                worksheet.Cells["C2:C3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["C2:C3"].AutoFitColumns(39);
                worksheet.Cells["C2"].Value = "ลักษณะงานที่ปฎิบัติ (โปรดใส่ให้ชัดเจนเพื่อการพิจารณางบประมาณ)";
                worksheet.Cells["C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["D2:I2"].Merge = true;
                worksheet.Cells["D2:I2"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["D2:I2"].AutoFitColumns();
                worksheet.Cells["D2"].Value = "ข้อมูลนักศึกษาปฎิบัติงาน (หากไม่มี นศ. ไม่ต้องระบุส่วนนี้ แต่ให้ใส่จำนวนที่ต้องการ ในช่องลำดับที่)";
                worksheet.Cells["D2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["D3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["D3"].AutoFitColumns();
                worksheet.Cells["D3"].Value = "ลำดับที่";
                worksheet.Cells["D3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["E3:F3"].Merge = true;
                worksheet.Cells["E3:F3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["E3:F3"].AutoFitColumns();
                worksheet.Cells["E3"].Value = "ชื่อ - สกุล";
                worksheet.Cells["E3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["G3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["G3"].AutoFitColumns(19);
                worksheet.Cells["G3"].Value = "คณะ";
                worksheet.Cells["G3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["H3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["H3"].AutoFitColumns(11);
                worksheet.Cells["H3"].Value = "รหัสนักศึกษา";
                worksheet.Cells["H3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["I3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["I3"].AutoFitColumns(13);
                worksheet.Cells["I3"].Value = "โทรศัพท์ (จำเป็น)";
                worksheet.Cells["I3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["J2:L2"].Merge = true;
                worksheet.Cells["J2:L2"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["J2:L2"].AutoFitColumns();
                worksheet.Cells["J2"].Value = "ชื่อผู้ควบคุมการปฎิบัติงาน";
                worksheet.Cells["J2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["J3:K3"].Merge = true;
                worksheet.Cells["J3:K3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["J3:K3"].AutoFitColumns();
                worksheet.Cells["J3"].Value = "ชื่อ - สกุล";
                worksheet.Cells["J3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["L3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["L3"].AutoFitColumns();
                worksheet.Cells["L3"].Value = "โทรศัพท์";
                worksheet.Cells["L3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["M2:O2"].Merge = true;
                worksheet.Cells["M2:O2"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["M2:O2"].AutoFitColumns();
                worksheet.Cells["M2"].Value = "ข้อมูลบัญชีนักศึกษา(ชื่อ นศ. เป็นเจ้าของ บช.)";
                worksheet.Cells["M2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["M3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["M3"].AutoFitColumns(13);
                worksheet.Cells["M3"].Value = "ธนาคาร";
                worksheet.Cells["M3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["N3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["N3"].AutoFitColumns(10);
                worksheet.Cells["N3"].Value = "ชื่อสาขาธนาคาร";
                worksheet.Cells["N3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["O3"].Style.Font.Name = "TH SarabunPSK";
                worksheet.Cells["O3"].AutoFitColumns(21);
                worksheet.Cells["O3"].Value = "เลขที่บัญชี (ไม่ต้องมีขีด/ไม่ต้องวรรค)";
                worksheet.Cells["O3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var r_min = Model.Min(n => n.Row);
                var r_max = Model.Max(x => x.Row);
                var Data = Model.ToList();
                int cel = 4;

                if (r_min == 0 && r_max == 0)
                {
                    return Json(new { valid = false, message = "ไม่สามารถออก Excel ได้!!!" });
                }

                foreach (var d in Data)
                {
                    worksheet.Cells["A" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["A" + cel].Value = d.Row; //"ที่"
                    worksheet.Cells["A" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A" + cel].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    worksheet.Cells["B" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["B" + cel].Value = d.Job_description; //"ชื่อหน่วยงาน (คณะ/วิทยาลัย/กอง/สำนักงาน)"
                    worksheet.Cells["B" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["B" + cel].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    worksheet.Cells["C" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["C" + cel].Value = d.Job_name; //"ลักษณะงานที่ปฎิบัติ (โปรดใส่ให้ชัดเจนเพื่อการพิจารณางบประมาณ)"
                    worksheet.Cells["C" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["C" + cel].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    worksheet.Cells["D" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["D" + cel].Value = d.Row; //"ลำดับที่"
                    worksheet.Cells["D" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    worksheet.Cells["E" + cel + ":" + "F" + cel].Merge = true;
                    worksheet.Cells["E" + cel + ":" + "F" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["E" + cel].Value = d.student_name; //"ชื่อ - สกุล"
                    worksheet.Cells["E" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells["G" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["G" + cel].Value = d.faculty; //"คณะ"
                    worksheet.Cells["G" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells["H" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["H" + cel].Value = d.student_id; //"รหัสนักศึกษา"
                    worksheet.Cells["H" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells["I" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["I" + cel].Value = d.student_tel; //"โทรศัพท์ (จำเป็น)"
                    worksheet.Cells["I" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells["J" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["J" + cel + ":" + "K" + cel].Merge = true;
                    worksheet.Cells["J" + cel].Value = d.Job_owner; //"ชื่อ - สกุล"
                    worksheet.Cells["J" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells["L" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["L" + cel].Value = d.Owner_tel; //"โทรศัพท์"
                    worksheet.Cells["L" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells["M" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["M" + cel].Value = d.bank_name; //"ธนาคาร"
                    worksheet.Cells["M" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells["N" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["N" + cel].Value = d.bank_branch; //"ชื่อสาขาธนาคาร"
                    worksheet.Cells["N" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells["O" + cel].Style.Font.Name = "TH SarabunPSK";
                    worksheet.Cells["O" + cel].Value = d.bank_no; //"เลขที่บัญชี (ไม่ต้องมีขีด/ไม่ต้องวรรค)"
                    worksheet.Cells["O" + cel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    cel++;
                }


                // Save the ExcelPackage to a MemoryStream
                var stream = new System.IO.MemoryStream();
                excelPackage.SaveAs(stream);

                // Return the MemoryStream as a FileStreamResult
                //return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "รายชื่อนักศึกษาที่สมัครงานภายในคณะ/หน่วยงาน.xlsx");
            }
        }

        #endregion
    }
}

