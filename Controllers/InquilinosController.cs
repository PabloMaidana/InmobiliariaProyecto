using Inmobiliaria.Web.Data;
using Inmobiliaria.Web.Models;
using Inmobiliaria.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Web.Controllers
{
    [Authorize]
    public class InquilinosController : Controller
    {
        private readonly InquilinoRepository _repo;
        public InquilinosController(InquilinoRepository repo){ _repo=repo; }

        public async Task<IActionResult> Index()=>View(await _repo.GetAllAsync());
        public IActionResult Create()=>View(new Inquilino());
        [HttpPost] public async Task<IActionResult> Create(Inquilino m){ if(!ModelState.IsValid) return View(m); await _repo.InsertAsync(m, AuthService.UserId(User)); return RedirectToAction(nameof(Index)); }
        public async Task<IActionResult> Edit(int id){ var m=await _repo.GetAsync(id); return m==null? NotFound(): View(m); }
        [HttpPost] public async Task<IActionResult> Edit(Inquilino m){ if(!ModelState.IsValid) return View(m); await _repo.UpdateAsync(m); return RedirectToAction(nameof(Index)); }
        public async Task<IActionResult> Details(int id){ var m=await _repo.GetAsync(id); return m==null? NotFound(): View(m); }
        [Authorize(Policy="AdminOnly")] public async Task<IActionResult> Delete(int id){ var m=await _repo.GetAsync(id); return m==null? NotFound(): View(m); }
        [Authorize(Policy="AdminOnly")] [HttpPost, ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id){ await _repo.DeleteAsync(id); return RedirectToAction(nameof(Index)); }
    }
}
