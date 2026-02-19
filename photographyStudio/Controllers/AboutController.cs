using Microsoft.AspNetCore.Mvc;

namespace photographyStudio.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
