using Microsoft.AspNetCore.Mvc;

namespace HabitTrackerAspNetMVCWebApp.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
