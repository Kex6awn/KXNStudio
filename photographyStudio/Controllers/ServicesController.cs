using Microsoft.AspNetCore.Mvc;

namespace photographyStudio.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
