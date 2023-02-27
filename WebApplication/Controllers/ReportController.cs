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
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
        public async Task<IActionResult> ExportPDF()
        {
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            //var ComLicenses = DB.COM_LICENSE.Where(w => w.CustomerId == CurrentUser.CustomerId);
            MemoryStream workStream = new MemoryStream();
            string Msg = string.Empty;
            try
            {
                BaseFont bf = BaseFont.CreateFont(_environment.WebRootPath + "//fonts/cordia.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);
                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                Font FontNormal = new Font(bf, 10);
                Font FontNormalBold = new Font(bf, 10, Font.BOLD);
                Font FontMedium = new Font(bf, 14);
                Font FontBig = new Font(bf, 16);
                Font FontBigBold = new Font(bf, 16, Font.BOLD);
                iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);
                PdfWriter.GetInstance(document, workStream).CloseStream = false;

                string imagePath = Path.Combine(_environment.WebRootPath.ToString(), "img/kodelogo.jpg");
                Image img = Image.GetInstance(imagePath);
                img.Alignment = Element.ALIGN_LEFT;
                img.SetAbsolutePosition(60f, 790f);
                img.ScaleToFit(45f, 70f);

                document.Open();

                PdfPTable Tabless = new PdfPTable(6);
                Tabless.TotalWidth = 530f;
                Tabless.HorizontalAlignment = 1;
                Tabless.SpacingAfter = 0;
                Tabless.DefaultCell.Border = Rectangle.NO_BORDER;
                Tabless.HeaderRows = 6;
                float[] ColWidthss = new float[6];
                ColWidthss[0] = 40f;
                ColWidthss[1] = 170f;
                ColWidthss[2] = 80f;
                ColWidthss[3] = 80f;
                ColWidthss[4] = 80f;
                ColWidthss[5] = 80f;
                Tabless.SetWidths(ColWidthss);

                PdfPCell TableCellss = new PdfPCell(img);
                TableCellss.Colspan = 6;
                TableCellss.Border = Rectangle.NO_BORDER;
                Tabless.AddCell(TableCellss);

                TableCellss = new PdfPCell(new Phrase("License Management Report", FontBig));
                TableCellss.HorizontalAlignment = Element.ALIGN_CENTER;
                TableCellss.VerticalAlignment = Element.ALIGN_CENTER;
                TableCellss.Border = Rectangle.NO_BORDER;
                TableCellss.Colspan = 6;
                Tabless.AddCell(TableCellss);

                TableCellss = new PdfPCell(new Phrase("Issue Date " , FontNormal));
                TableCellss.HorizontalAlignment = Element.ALIGN_RIGHT;
                TableCellss.VerticalAlignment = Element.ALIGN_RIGHT;
                TableCellss.Border = Rectangle.NO_BORDER;
                TableCellss.Colspan = 6;
                Tabless.AddCell(TableCellss);

                TableCellss = new PdfPCell(new Phrase(" ", FontNormal));
                TableCellss.HorizontalAlignment = Element.ALIGN_RIGHT;
                TableCellss.VerticalAlignment = Element.ALIGN_RIGHT;
                TableCellss.Border = Rectangle.NO_BORDER;
                TableCellss.Colspan = 6;
                Tabless.AddCell(TableCellss);

                /* table  header */

                TableCellss = new PdfPCell(new Phrase("NO.", FontNormal));
                TableCellss.HorizontalAlignment = Element.ALIGN_CENTER;
                TableCellss.VerticalAlignment = Element.ALIGN_CENTER;
                TableCellss.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
                Tabless.AddCell(TableCellss);

                TableCellss = new PdfPCell(new Phrase("Name", FontNormal));
                TableCellss.HorizontalAlignment = Element.ALIGN_LEFT;
                TableCellss.VerticalAlignment = Element.ALIGN_LEFT;
                TableCellss.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
                Tabless.AddCell(TableCellss);

                TableCellss = new PdfPCell(new Phrase("Partial product Key", FontNormal));
                TableCellss.HorizontalAlignment = Element.ALIGN_LEFT;
                TableCellss.VerticalAlignment = Element.ALIGN_LEFT;
                TableCellss.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
                Tabless.AddCell(TableCellss);

                TableCellss = new PdfPCell(new Phrase("Used/Total", FontNormal));
                TableCellss.HorizontalAlignment = Element.ALIGN_LEFT;
                TableCellss.VerticalAlignment = Element.ALIGN_LEFT;
                TableCellss.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
                Tabless.AddCell(TableCellss);

                TableCellss = new PdfPCell(new Phrase("Start Date", FontNormal));
                TableCellss.HorizontalAlignment = Element.ALIGN_CENTER;
                TableCellss.VerticalAlignment = Element.ALIGN_CENTER;
                TableCellss.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
                Tabless.AddCell(TableCellss);

                TableCellss = new PdfPCell(new Phrase("Expiry Date", FontNormal));
                TableCellss.HorizontalAlignment = Element.ALIGN_CENTER;
                TableCellss.VerticalAlignment = Element.ALIGN_CENTER;
                TableCellss.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
                Tabless.AddCell(TableCellss);


                /* data row */

                int CountFirstRow = 1;
                //var ComLicense = DB.COM_LICENSE.Where(w => w.CustomerId == CurrentUser.CustomerId);
                var Gets = DB.TRANSACTION_WORKING;
                foreach (var Get in Gets)
                {
                    //int CountItem = ComLicenses.Where(w => w.PartialProductKey == Get.PartialproductKey).Count();

                    TableCellss = new PdfPCell(new Phrase(CountFirstRow.ToString(), FontNormal));
                    TableCellss.HorizontalAlignment = Element.ALIGN_CENTER;
                    TableCellss.VerticalAlignment = Element.ALIGN_CENTER;
                    TableCellss.Border = Rectangle.NO_BORDER;
                    Tabless.AddCell(TableCellss);

                    TableCellss = new PdfPCell(new Phrase(Get.detail_working, FontNormal));
                    TableCellss.HorizontalAlignment = Element.ALIGN_LEFT;
                    TableCellss.VerticalAlignment = Element.ALIGN_CENTER;
                    TableCellss.Border = Rectangle.NO_BORDER;
                    Tabless.AddCell(TableCellss);

                    CountFirstRow++;
                }


                document.Add(Tabless);

                document.Close();
                byte[] byteInfo = workStream.ToArray();
                workStream.Write(byteInfo, 0, byteInfo.Length);
                workStream.Position = 0;
            }
            catch (Exception error)
            {
                string msg = "Error is : " + error.Message;
            }
            return new FileStreamResult(workStream, "application/pdf");
        }

        #endregion

        #region Excel

        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            string msg = "";
            var CurrentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            //var ComLicenses = DB.COM_LICENSE.Where(w => w.CustomerId == CurrentUser.CustomerId);
            try
            {
                var templateFilePath = Path.Combine(_environment.WebRootPath.ToString(), "excels/assets/template.xlsx");
                FileInfo templateFile = new FileInfo(templateFilePath);
                var newFilePath = Path.Combine(_environment.WebRootPath.ToString(), "excels/assets/new/template.xlsx");
                FileInfo newFile = new FileInfo(newFilePath);

                using (ExcelPackage package = new ExcelPackage(newFile, templateFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    int StartRow = 3;
                    int CountFirstRow = 1;
                    var Gets = DB.TRANSACTION_REGISTER;
                    foreach (var Get in Gets)
                    {
                        worksheet.Cells["A" + StartRow].Value = CountFirstRow.ToString();
                        worksheet.Cells["B" + StartRow].Value = Get.fullname;

                        worksheet.Cells["A" + StartRow + ":F" + StartRow].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells["A" + StartRow + ":F" + StartRow].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells["A" + StartRow + ":F" + StartRow].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells["A" + StartRow + ":F" + StartRow].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        StartRow++;
                        CountFirstRow++;
                    }

                    package.Save();
                }

                var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return new FileStreamResult(new FileStream(newFilePath, FileMode.Open), mimeType);
            }
            catch (Exception error)
            {
                msg = "Error is : " + error.Message;
                return Json(new { valid = false, message = msg });
            }
        }

        #endregion

    }
}
