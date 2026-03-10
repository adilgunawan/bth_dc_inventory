
using Microsoft.AspNetCore.Mvc;

namespace bth_dc_inventory.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

    }
}
