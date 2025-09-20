using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Web.Security;

namespace Inmobiliaria.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _auth;
        public AuthController(AuthService auth) { _auth = auth; }

        [AllowAnonymous]
        [HttpGet] public IActionResult Login(string? returnUrl=null) => View(model: returnUrl);

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl=null)
        {
            if (await _auth.SignInAsync(HttpContext, email, password))
                return Redirect(returnUrl ?? Url.Action("Index","Home")!);
            ModelState.AddModelError("", "Credenciales inválidas");
            return View(model:returnUrl);
        }

        [Authorize]
        public async Task<IActionResult> Logout() { await _auth.SignOutAsync(HttpContext); return RedirectToAction("Login"); }

        [AllowAnonymous] public IActionResult Denied() => Content("Acceso denegado");
    }
}
