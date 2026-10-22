namespace Hotel.Web.ViewModels;

public class DisponibilidadViewModel
{
    public int IdHabitacion { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Piso { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public decimal PrecioNoche { get; set; }
    public int Noches { get; set; }
    public decimal TotalEstimado { get; set; }
}
