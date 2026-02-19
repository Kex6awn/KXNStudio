using Microsoft.AspNetCore.Mvc;

namespace photographyStudio.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
