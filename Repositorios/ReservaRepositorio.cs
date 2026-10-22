using Hotel.Web.Models;
using Hotel.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio
{
    private readonly HotelContext _context;

    public ReservaRepositorio(HotelContext context)
    {
        _context = context;
    }

    public Task<List<Huesped>> ObtenerHuespedesActivosAsync() =>
        _context.Huespeds.AsNoTracking().Where(h => h.Activo).OrderBy(h => h.Apellidos).ThenBy(h => h.Nombres).ToListAsync();

    public Task<List<Habitacion>> ObtenerHabitacionesActivasAsync() =>
        _context.Habitacions.AsNoTracking().Where(h => h.Activo).OrderBy(h => h.Numero).ToListAsync();

    public Task<bool> EstaDisponibleAsync(int idHabitacion, DateOnly ingreso, DateOnly salida) =>
        _context.Reservas.AsNoTracking().AnyAsync(r =>
            r.IdHabitacion == idHabitacion && r.Estado != "Cancelada" &&
            r.FechaIngreso < salida && r.FechaSalida > ingreso);

    public Task<List<DisponibilidadViewModel>> BuscarDisponiblesAsync(DateOnly ingreso, DateOnly salida) =>
        _context.Habitacions.AsNoTracking()
            .Where(h => h.Activo && !h.Reservas.Any(r => r.Estado != "Cancelada" && r.FechaIngreso < salida && r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .Select(h => new DisponibilidadViewModel
            {
                IdHabitacion = h.IdHabitacion,
                Numero = h.Numero,
                Piso = h.Piso,
                Tipo = h.IdTipoHabitacionNavigation.Nombre,
                Capacidad = h.IdTipoHabitacionNavigation.Capacidad,
                PrecioNoche = h.IdTipoHabitacionNavigation.PrecioNoche,
                Noches = salida.DayNumber - ingreso.DayNumber,
                TotalEstimado = h.IdTipoHabitacionNavigation.PrecioNoche * (salida.DayNumber - ingreso.DayNumber)
            }).ToListAsync();

    public async Task CrearAsync(Reserva reserva)
    {
        reserva.Estado = "Pendiente";
        reserva.FechaRegistro = DateTime.Now;
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
    }

    public Task<List<Reserva>> ObtenerTodasAsync() =>
        _context.Reservas.AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation).ThenInclude(h => h.IdTipoHabitacionNavigation)
            .OrderByDescending(r => r.FechaRegistro).ToListAsync();

    public async Task<bool> CancelarAsync(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva is null || (reserva.Estado != "Pendiente" && reserva.Estado != "Confirmada")) return false;
        reserva.Estado = "Cancelada";
        await _context.SaveChangesAsync();
        return true;
    }
}
