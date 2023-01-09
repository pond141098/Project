using Microsoft.AspNetCore.Mvc;

namespace SeniorProject.Controllers
{
    public class OfficeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
