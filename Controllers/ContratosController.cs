using Inmobiliaria.Web.Data;
using Inmobiliaria.Web.Models;
using Inmobiliaria.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Inmobiliaria.Web.Controllers
{
    [Authorize]
    public class ContratosController : Controller
    {
        private readonly ContratoRepository _repo;
        private readonly InmuebleRepository _inmRepo;
        private readonly InquilinoRepository _inqRepo;
        public ContratosController(ContratoRepository repo, InmuebleRepository inmRepo, InquilinoRepository inqRepo){ _repo=repo; _inmRepo=inmRepo; _inqRepo=inqRepo; }

        public async Task<IActionResult> Index()=>View(await _repo.GetAllAsync());

        public async Task<IActionResult> Create(){
            ViewBag.Inmuebles = await _inmRepo.DisponiblesAsync();
            ViewBag.Inquilinos = await _inqRepo.GetAllAsync();
            return View(new Contrato());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Contrato m){
            if(m.FechaInicio >= m.FechaFin) ModelState.AddModelError("", "La fecha fin debe ser posterior al inicio");
            if(!ModelState.IsValid){ ViewBag.Inmuebles = await _inmRepo.DisponiblesAsync(); ViewBag.Inquilinos = await _inqRepo.GetAllAsync(); return View(m); }
            if(await _repo.HaySolapamientoAsync(m.InmuebleId, m.FechaInicio, m.FechaFin)){
                ModelState.AddModelError("", "El inmueble está ocupado en esas fechas");
                ViewBag.Inmuebles = await _inmRepo.DisponiblesAsync(); ViewBag.Inquilinos = await _inqRepo.GetAllAsync(); return View(m);
            }
            await _repo.InsertAsync(m, AuthService.UserId(User));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Terminar(int id, DateOnly fechaEfectiva)
        {
            var cto = await _repo.GetAsync(id); if (cto==null) return NotFound();
            var totalMeses = ((cto.FechaFin.Year - cto.FechaInicio.Year) * 12) + (cto.FechaFin.Month - cto.FechaInicio.Month);
            var transcurridos = ((fechaEfectiva.Year - cto.FechaInicio.Year) * 12) + (fechaEfectiva.Month - cto.FechaInicio.Month);
            var mesesMulta = transcurridos < totalMeses/2.0 ? 2 : 1;
            var multa = mesesMulta * cto.Monto;
            await _repo.TerminarAnticipadoAsync(id, fechaEfectiva, multa, AuthService.UserId(User));
            TempData["Multa"] = multa.ToString(CultureInfo.InvariantCulture);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> PorVencer(int dias=30){
            var hasta = DateOnly.FromDateTime(DateTime.Today.AddDays(dias));
            var lista = await _repo.VencenAntesDeAsync(hasta);
            return View(lista);
        }
    }
}
