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

        //#region Excel

        //[HttpGet]
        //public async Task<IActionResult> ExportExcel()
        //{
        //    string msg = "";
        //    var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
        //    //var ComLicenses = DB.COM_LICENSE.Where(w => w.CustomerId == CurrentUser.CustomerId);
        //    try
        //    {
        //        var templateFilePath = Path.Combine(_environment.WebRootPath.ToString(), "excels/assets/template.xlsx");
        //        FileInfo templateFile = new FileInfo(templateFilePath);
        //        var newFilePath = Path.Combine(_environment.WebRootPath.ToString(), "excels/assets/new/template.xlsx");
        //        FileInfo newFile = new FileInfo(newFilePath);

        //        using (ExcelPackage package = new ExcelPackage(newFile, templateFile))
        //        {
        //            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

        //            int StartRow = 3;
        //            int CountFirstRow = 1;
        //            var Gets = DB.TRANSACTION_REGISTER;
        //            foreach (var Get in Gets)
        //            {
        //                worksheet.Cells["A" + StartRow].Value = CountFirstRow.ToString();
        //                worksheet.Cells["A" + StartRow + ":F" + StartRow].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //                worksheet.Cells["A" + StartRow + ":F" + StartRow].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //                worksheet.Cells["A" + StartRow + ":F" + StartRow].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        //                worksheet.Cells["A" + StartRow + ":F" + StartRow].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        //                StartRow++;
        //                CountFirstRow++;
        //            }

        //            package.Save();
        //        }

        //        var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        return new FileStreamResult(new FileStream(newFilePath, FileMode.Open), mimeType);
        //    }
        //    catch (Exception error)
        //    {
        //        msg = "Error is : " + error.Message;
        //        return Json(new { valid = false, message = msg });
        //    }
        //}

        //#endregion

        public IActionResult ExampleExcel()
        {
            // Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Create a new ExcelPackage
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                // Create a new worksheet
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("MySheet");

                // Add some data to the worksheet
                worksheet.Cells["A1:O1"].Merge = true ;
                worksheet.Cells["A1"].Value = "โครงการสนับสนุนการหารายได้พิเศษระหว่างเรียนของนักศึกษา ปีงบประมาณ";
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["A2:A3"].Merge = true;
                worksheet.Cells["A2"].Value = "ที่";
                worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["B2:B3"].Merge = true;
                worksheet.Cells["B2"].Value = "ชื่อหน่วยงาน (คณะ/วิทยาลัย/กอง/สำนักงาน)";
                worksheet.Cells["B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["C2:C3"].Merge = true;
                worksheet.Cells["C2"].Value = "ลักษณะงานที่ปฎิบัติ (โปรดใส่ให้ชัดเจนเพื่อการพิจารณางบประมาณ)";
                worksheet.Cells["C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["D2:I2"].Merge = true;
                worksheet.Cells["D2"].Value = "ข้อมูลนักศึกษาปฎิบัติงาน (หากไม่มี นศ. ไม่ต้องระบุส่วนนี้ แต่ให้ใส่จำนวนที่ต้องการ ในช่องลำดับที่)";
                worksheet.Cells["D2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["D3"].Value = "ลำดับที่";
                worksheet.Cells["D3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["E3:F3"].Merge = true;
                worksheet.Cells["E3"].Value = "ชื่อ - สกุล";
                worksheet.Cells["E3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["G3"].Value = "คณะ";
                worksheet.Cells["G3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["H3"].Value = "รหัสนักศึกษา";
                worksheet.Cells["H3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["I3"].Value = "โทรศัพท์ (จำเป็น)";
                worksheet.Cells["I3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["J2:L2"].Merge = true;
                worksheet.Cells["J2"].Value = "ชื่อผู้ควบคุมการปฎิบัติงาน";
                worksheet.Cells["J2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["J3:K3"].Merge = true;
                worksheet.Cells["J3"].Value = "ชื่อ - สกุล";
                worksheet.Cells["J3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["L3"].Value = "โทรศัพท์";
                worksheet.Cells["L3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["M2:O2"].Merge = true;
                worksheet.Cells["M2"].Value = "ข้อมูลบัญชีนักศึกษา(ชื่อ นศ. เป็นเจ้าของ บช.)";
                worksheet.Cells["M2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["M3"].Value = "ธนาคาร";
                worksheet.Cells["M3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["N3"].Value = "ชื่อสาขาธนาคาร";
                worksheet.Cells["N3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["O3"].Value = "เลขที่บัญชี (ไม่ต้องมีขีด/ไม่ต้องวรรค)";
                worksheet.Cells["O3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Save the ExcelPackage to a MemoryStream
                var stream = new System.IO.MemoryStream();
                excelPackage.SaveAs(stream);

                // Return the MemoryStream as a FileStreamResult
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                //return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MyExcelFile.xlsx");
            }
        }


    }
}

