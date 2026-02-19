using Microsoft.AspNetCore.Mvc;

namespace photographyStudio.Controllers
{
    public class PortfolioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
