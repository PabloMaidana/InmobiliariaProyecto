using Inmobiliaria.Web.Data;
using Inmobiliaria.Web.Models;
using Inmobiliaria.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Web.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly PagoRepository _repo;
        public PagosController(PagoRepository repo){ _repo=repo; }

        public async Task<IActionResult> Index(int contratoId){
            ViewBag.ContratoId = contratoId;
            var lista = await _repo.ListarPorContratoAsync(contratoId);
            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(int contratoId, DateOnly fecha, string concepto, decimal importe){
            await _repo.CrearPagoAsync(new Pago{ContratoId=contratoId, Fecha=fecha, Concepto=concepto, Importe=importe}, AuthService.UserId(User));
            return RedirectToAction(nameof(Index), new { contratoId });
        }

        [HttpPost]
        public async Task<IActionResult> EditarConcepto(int id, int contratoId, string concepto){
            await _repo.EditarConceptoAsync(id, concepto);
            return RedirectToAction(nameof(Index), new { contratoId });
        }

        [HttpPost]
        public async Task<IActionResult> Anular(int id, int contratoId){
            await _repo.AnularAsync(id, AuthService.UserId(User));
            return RedirectToAction(nameof(Index), new { contratoId });
        }
    }
}
