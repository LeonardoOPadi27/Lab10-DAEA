using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio
{
    private readonly HotelContext _context;

    public HabitacionRepositorio(HotelContext context)
    {
        _context = context;
    }

    public async Task<List<Habitacion>> ObtenerActivasAsync(int? tipoId)
    {
        var query = _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo);

        if (tipoId.HasValue)
            query = query.Where(h => h.IdTipoHabitacion == tipoId.Value);

        return await query.OrderBy(h => h.Numero).ToListAsync();
    }

    public Task<Habitacion?> ObtenerPorIdAsync(int id) =>
        _context.Habitacions.AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id && h.Activo);

    public Task<List<TipoHabitacion>> ObtenerTiposAsync() =>
        _context.TipoHabitacions.AsNoTracking().Where(t => t.Activo).OrderBy(t => t.Nombre).ToListAsync();

    public Task<bool> NumeroExisteAsync(string numero, int? excluirId = null) =>
        _context.Habitacions.AnyAsync(h => h.Numero == numero && (!excluirId.HasValue || h.IdHabitacion != excluirId.Value));

    public async Task CrearAsync(Habitacion habitacion)
    {
        habitacion.Activo = true;
        _context.Habitacions.Add(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Habitacion habitacion)
    {
        var actual = await _context.Habitacions.FindAsync(habitacion.IdHabitacion);
        if (actual is null) return;
        actual.Numero = habitacion.Numero;
        actual.Piso = habitacion.Piso;
        actual.IdTipoHabitacion = habitacion.IdTipoHabitacion;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> BajaLogicaAsync(int id)
    {
        var habitacion = await _context.Habitacions.FindAsync(id);
        if (habitacion is null || !habitacion.Activo) return false;
        habitacion.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
