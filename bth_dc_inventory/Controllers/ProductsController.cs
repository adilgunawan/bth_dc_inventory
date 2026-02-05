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

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Data_center_1()
        {
            return View();
        }
        public IActionResult Data_center_2()
        {
            return View();
        }
        public IActionResult Data_center_3()
        {
            return View();
        }
        public IActionResult Settings()
        {
            return View();
        }
        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult Category()
        {
            return View();
        }
    }


}