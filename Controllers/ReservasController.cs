using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio _repositorio;

    public ReservasController(ReservaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        ViewBag.Ingreso = ingreso;
        ViewBag.Salida = salida;
        if (!ingreso.HasValue || !salida.HasValue) return View(new List<Hotel.Web.ViewModels.DisponibilidadViewModel>());
        if (salida <= ingreso)
        {
            ViewBag.Error = "La fecha de salida debe ser posterior al ingreso.";
            return View(new List<Hotel.Web.ViewModels.DisponibilidadViewModel>());
        }
        return View(await _repositorio.BuscarDisponiblesAsync(ingreso.Value, salida.Value));
    }

    public async Task<IActionResult> Index() => View(await _repositorio.ObtenerTodasAsync());

    public async Task<IActionResult> Create(int? idHabitacion, DateOnly? ingreso, DateOnly? salida)
    {
        var reserva = new Reserva
        {
            IdHabitacion = idHabitacion ?? 0,
            FechaIngreso = ingreso ?? DateOnly.FromDateTime(DateTime.Today),
            FechaSalida = salida ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };
        await CargarListasAsync(reserva.IdHuesped, reserva.IdHabitacion);
        return View(reserva);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reserva reserva)
    {
        ModelState.Remove(nameof(Reserva.IdHuespedNavigation));
        ModelState.Remove(nameof(Reserva.IdHabitacionNavigation));
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        if (reserva.FechaSalida <= reserva.FechaIngreso)
            ModelState.AddModelError(nameof(Reserva.FechaSalida), "La fecha de salida debe ser posterior al ingreso.");
        if (reserva.FechaIngreso < hoy)
            ModelState.AddModelError(nameof(Reserva.FechaIngreso), "La fecha de ingreso no puede ser anterior a hoy.");
        if (reserva.FechaSalida > reserva.FechaIngreso && await _repositorio.EstaDisponibleAsync(reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida))
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación ya está reservada en el rango seleccionado.");

        if (!ModelState.IsValid)
        {
            await CargarListasAsync(reserva.IdHuesped, reserva.IdHabitacion);
            return View(reserva);
        }

        await _repositorio.CrearAsync(reserva);
        TempData["Mensaje"] = "Reserva registrada con estado Pendiente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var cancelada = await _repositorio.CancelarAsync(id);
        TempData[cancelada ? "Mensaje" : "Error"] = cancelada
            ? "Reserva cancelada correctamente."
            : "La reserva no puede cancelarse.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync(int? huespedSeleccionado = null, int? habitacionSeleccionada = null)
    {
        var huespedes = await _repositorio.ObtenerHuespedesActivosAsync();
        var habitaciones = await _repositorio.ObtenerHabitacionesActivasAsync();
        ViewBag.Huespedes = new SelectList(huespedes.Select(h => new { h.IdHuesped, Nombre = $"{h.Apellidos}, {h.Nombres} ({h.Dni})" }), "IdHuesped", "Nombre", huespedSeleccionado);
        ViewBag.Habitaciones = new SelectList(habitaciones.Select(h => new { h.IdHabitacion, Nombre = $"{h.Numero} · Piso {h.Piso}" }), "IdHabitacion", "Nombre", habitacionSeleccionada);
    }
}
