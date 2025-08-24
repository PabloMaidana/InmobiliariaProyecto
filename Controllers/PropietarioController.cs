using Inmobiliaria.Web.Data;
using Inmobiliaria.Web.Models;
using Inmobiliaria.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Web.Controllers
{
    [Authorize]
    public class PropietariosController : Controller
    {
        private readonly PropietarioRepository _repo;
        public PropietariosController(PropietarioRepository repo) { _repo = repo; }

        public async Task<IActionResult> Index(string? q) =>
            View(string.IsNullOrWhiteSpace(q)? await _repo.GetAllAsync() : await _repo.BuscarPorNombreAsync(q!));

        public IActionResult Create() => View(new Propietario());
        [HttpPost]
        public async Task<IActionResult> Create(Propietario p){
            if(!ModelState.IsValid) return View(p);
            await _repo.InsertAsync(p, AuthService.UserId(User));
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id){ var m=await _repo.GetAsync(id); return m==null? NotFound(): View(m); }
        [HttpPost]
        public async Task<IActionResult> Edit(Propietario p){
            if(!ModelState.IsValid) return View(p);
            await _repo.UpdateAsync(p);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id){ var m=await _repo.GetAsync(id); return m==null? NotFound(): View(m); }

        [Authorize(Policy="AdminOnly")]
        public async Task<IActionResult> Delete(int id){ var m=await _repo.GetAsync(id); return m==null? NotFound(): View(m); }

        [Authorize(Policy="AdminOnly")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id){
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
