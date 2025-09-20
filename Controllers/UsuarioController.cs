using Inmobiliaria.Web.Data;
using Inmobiliaria.Web.Models;
using Inmobiliaria.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Web.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly UsuarioRepository _repo;
        private readonly IWebHostEnvironment _env;

        public UsuariosController(UsuarioRepository repo, IWebHostEnvironment env)
        {
            _repo = repo; _env = env;
        }

        // ===== ABM (solo Admin) =====
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index() => View(await _repo.GetAllAsync());

        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create() => View(new Usuario{ Rol="Empleado" });

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Create(Usuario u, string password)
        {
            if (!ModelState.IsValid) return View(u);
            await _repo.InsertAsync(u, password);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int id)
        {
            var u = await _repo.GetAsync(id);
            return u == null ? NotFound() : View(u);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Edit(Usuario u)
        {
            if (!ModelState.IsValid) return View(u);
            await _repo.UpdateAsync(u);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _repo.GetAsync(id);
            return u == null ? NotFound() : View(u);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ===== Perfil (cualquier usuario) =====
        public async Task<IActionResult> Perfil()
        {
            var meId = AuthService.UserId(User);
            var me = await _repo.GetAsync(meId);
            return me == null ? NotFound() : View(me);
        }

        [HttpPost]
        public async Task<IActionResult> Perfil(Usuario me)
        {
            // El usuario NO puede cambiar su rol desde aquí: forzar a conservar el actual
            var current = await _repo.GetAsync(me.Id);
            if (current == null) return NotFound();
            me.Rol = current.Rol;

            if (!ModelState.IsValid) return View(me);
            await _repo.UpdateAsync(me);
            return RedirectToAction(nameof(Perfil));
        }

        [HttpPost]
        public async Task<IActionResult> CambiarPassword(int id, string nueva)
        {
            var meId = AuthService.UserId(User);
            if (meId != id && !AuthService.IsAdmin(User)) return Forbid();
            await _repo.UpdatePasswordAsync(id, nueva);
            TempData["Ok"] = "Contraseña actualizada.";
            return RedirectToAction(nameof(Perfil));
        }

        [HttpPost]
        public async Task<IActionResult> SubirAvatar(int id, IFormFile? avatar)
        {
            var meId = AuthService.UserId(User);
            if (meId != id && !AuthService.IsAdmin(User)) return Forbid();

            string? url = null;
            if (avatar != null && avatar.Length > 0)
            {
                var fileName = $"avatar_{id}{Path.GetExtension(avatar.FileName)}";
                var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                using var fs = new FileStream(path, FileMode.Create);
                await avatar.CopyToAsync(fs);
                url = $"/uploads/{fileName}";
            }
            await _repo.UpdateAvatarAsync(id, url);
            return RedirectToAction(nameof(Perfil));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarAvatar(int id)
        {
            var meId = AuthService.UserId(User);
            if (meId != id && !AuthService.IsAdmin(User)) return Forbid();
            await _repo.UpdateAvatarAsync(id, null);
            return RedirectToAction(nameof(Perfil));
        }
    }
}
