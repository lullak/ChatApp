using ChatApp.Core.Interfaces.Services;
using ChatApp.Hubs;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    public class AuthController(ITokenService tokenService, ILogger<AuthController> logger) : Controller
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            _logger.LogInformation($"Username: {model.Username.Trim()} logged in");

            return LocalRedirect("/");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _logger.LogInformation($"Username: {User.FindFirst(ClaimTypes.Name)?.Value} logged out");
            Response.Cookies.Delete("token");
            return RedirectToAction("Index");
        }
    }
}
