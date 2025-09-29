using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    [Authorize]
    public class HomeController() : Controller
    {
        public IActionResult Index()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            ViewData["Username"] = username;

            return View();
        }
    }
}
