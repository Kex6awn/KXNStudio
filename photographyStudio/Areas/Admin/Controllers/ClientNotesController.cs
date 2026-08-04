using Microsoft.AspNetCore.Mvc;

namespace KxnPhotoStudio.Areas.Admin.Controllers
{
    public class ClientNotesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
