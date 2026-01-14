using Microsoft.AspNetCore.Mvc;

namespace bth_dc_inventory.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail()
        {
            return View();
        }
    }


}