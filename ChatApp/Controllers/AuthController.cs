using ChatApp.Core.Interfaces.Services;
using ChatApp.Hubs;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    public class AuthController(ITokenService tokenService) : Controller
    {
        private readonly ITokenService _tokenService = tokenService;

        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("Username", "Username cannot be empty");
                return View(model);
            }

            if (ChatHub.IsUserOnline(model.Username.Trim()))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            var token = _tokenService.GenerateToken(model.Username.Trim());
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            };
            Response.Cookies.Append("token", token, cookieOptions);

            return LocalRedirect("/");
        }
    }
}
