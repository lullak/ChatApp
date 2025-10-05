using ChatApp.Core.Interfaces.Services;
using ChatApp.Hubs;
using ChatApp.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    public class AuthController(ITokenService tokenService, ILogger<AuthController> logger, IHtmlSanitizer sanitizer) : Controller
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IHtmlSanitizer _sanitizer = sanitizer;

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

            var sanitizedUsername = _sanitizer.Sanitize(model.Username.Trim());

            if (string.IsNullOrWhiteSpace(sanitizedUsername))
            {
                ModelState.AddModelError("Username", "Username cannot be empty");
                return View(model);
            }

            if (ChatHub.IsUserOnline(sanitizedUsername))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            var token = _tokenService.GenerateToken(sanitizedUsername);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            };
            Response.Cookies.Append("token", token, cookieOptions);

            _logger.LogInformation($"Username: {sanitizedUsername} logged in");

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
