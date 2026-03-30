using Microsoft.AspNetCore.Mvc;

namespace HazelnutVeb.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
