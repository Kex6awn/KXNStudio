using Microsoft.AspNetCore.Mvc;

namespace photographyStudio.Controllers
{
    public class ServicesController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
