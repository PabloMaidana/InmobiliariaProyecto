using Inmobiliaria.Web.Data;
using Inmobiliaria.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Web.Controllers
{
    [Authorize]
    public class TiposInmuebleController : Controller
    {
        private readonly TipoInmuebleRepository _repo;
        public TiposInmuebleController(TipoInmuebleRepository repo){ _repo=repo; }

        public async Task<IActionResult> Index()=>View(await _repo.GetAllAsync());
        public IActionResult Create()=>View(new TipoInmueble());
        [HttpPost] public async Task<IActionResult> Create(TipoInmueble m){ if(!ModelState.IsValid) return View(m); await _repo.InsertAsync(m); return RedirectToAction(nameof(Index)); }
        public async Task<IActionResult> Edit(int id){ var m=await _repo.GetAsync(id); return m==null? NotFound(): View(m); }
        [HttpPost] public async Task<IActionResult> Edit(TipoInmueble m){ if(!ModelState.IsValid) return View(m); await _repo.UpdateAsync(m); return RedirectToAction(nameof(Index)); }
        [Authorize(Policy="AdminOnly")] public async Task<IActionResult> Delete(int id){ var m=await _repo.GetAsync(id); return m==null? NotFound(): View(m); }
        [Authorize(Policy="AdminOnly")] [HttpPost, ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id){ await _repo.DeleteAsync(id); return RedirectToAction(nameof(Index)); }
    }
}
