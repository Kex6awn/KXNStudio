using Microsoft.AspNetCore.Mvc;

namespace photographyStudio.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
