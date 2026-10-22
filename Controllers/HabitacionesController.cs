using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio _repositorio;

    public HabitacionesController(HabitacionRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index(int? tipoId)
    {
        await CargarTiposAsync(tipoId);
        return View(await _repositorio.ObtenerActivasAsync(tipoId));
    }

    public async Task<IActionResult> Details(int id)
    {
        var habitacion = await _repositorio.ObtenerPorIdAsync(id);
        return habitacion is null ? NotFound() : View(habitacion);
    }

    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync();
        return View(new Habitacion { Activo = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Habitacion habitacion)
    {
        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(Habitacion.Reservas));
        if (await _repositorio.NumeroExisteAsync(habitacion.Numero))
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número de habitación ya existe.");

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        await _repositorio.CrearAsync(habitacion);
        TempData["Mensaje"] = "Habitación creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var habitacion = await _repositorio.ObtenerPorIdAsync(id);
        if (habitacion is null) return NotFound();
        await CargarTiposAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion) return NotFound();
        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(Habitacion.Reservas));
        if (await _repositorio.NumeroExisteAsync(habitacion.Numero, id))
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número de habitación ya existe.");

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        await _repositorio.ActualizarAsync(habitacion);
        TempData["Mensaje"] = "Habitación actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var habitacion = await _repositorio.ObtenerPorIdAsync(id);
        return habitacion is null ? NotFound() : View(habitacion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await _repositorio.BajaLogicaAsync(id))
            TempData["Mensaje"] = "Habitación dada de baja correctamente.";
        else
            TempData["Error"] = "No fue posible dar de baja la habitación.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarTiposAsync(int? seleccionado = null)
    {
        ViewBag.Tipos = new SelectList(await _repositorio.ObtenerTiposAsync(), "IdTipoHabitacion", "Nombre", seleccionado);
    }
}
