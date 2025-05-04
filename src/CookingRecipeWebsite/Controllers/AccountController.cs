using Microsoft.AspNetCore.Mvc;

namespace CookingRecipeWebsite.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
