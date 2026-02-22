using Microsoft.AspNetCore.Mvc;

namespace KxnPhotoStudio.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
