using Microsoft.AspNetCore.Mvc;

namespace SeniorProject.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
