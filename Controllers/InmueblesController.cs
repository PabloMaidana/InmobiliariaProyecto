using Inmobiliaria.Web.Data;
using Inmobiliaria.Web.Models;
using Inmobiliaria.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Web.Controllers
{
    [Authorize]
    public class InmueblesController : Controller
    {
        private readonly InmuebleRepository _repo;
        private readonly PropietarioRepository _propRepo;
        private readonly TipoInmuebleRepository _tipoRepo;
        public InmueblesController(InmuebleRepository repo, PropietarioRepository propRepo, TipoInmuebleRepository tipoRepo){ _repo=repo; _propRepo=propRepo; _tipoRepo=tipoRepo; }

        public async Task<IActionResult> Index()=>View(await _repo.GetAllAsync());
        public async Task<IActionResult> Disponibles()=>View("Index", await _repo.DisponiblesAsync());

        public async Task<IActionResult> Create(){
            ViewBag.Propietarios = await _propRepo.GetAllAsync();
            ViewBag.Tipos = await _tipoRepo.GetAllAsync();
            return View(new Inmueble{Uso="Residencial", Disponible=true});
        }
        [HttpPost]
        public async Task<IActionResult> Create(Inmueble m){
            if(!ModelState.IsValid){ ViewBag.Propietarios=await _propRepo.GetAllAsync(); ViewBag.Tipos=await _tipoRepo.GetAllAsync(); return View(m); }
            await _repo.InsertAsync(m, AuthService.UserId(User)); return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id){
            var m=await _repo.GetAsync(id); if(m==null) return NotFound();
            ViewBag.Propietarios = await _propRepo.GetAllAsync();
            ViewBag.Tipos = await _tipoRepo.GetAllAsync();
            return View(m);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Inmueble m){
            if(!ModelState.IsValid){ ViewBag.Propietarios=await _propRepo.GetAllAsync(); ViewBag.Tipos=await _tipoRepo.GetAllAsync(); return View(m); }
            await _repo.UpdateAsync(m); return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Suspender(int id, bool disponible){ await _repo.SetDisponibleAsync(id, disponible); return RedirectToAction(nameof(Index)); }

        public IActionResult BuscarLibres()=>View();
        [HttpPost]
        public async Task<IActionResult> BuscarLibres(DateOnly desde, DateOnly hasta)=>View("Index", await _repo.LibresEntreAsync(desde, hasta));
    }
}
