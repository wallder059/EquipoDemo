using System.Globalization;

namespace MantenimientoVehicular.Mobile.Models;

public sealed class Mantenimiento
{
    public required DateTime Fecha { get; init; }
    public required string TipoServicio { get; init; }
    public int Kilometraje { get; init; }

    public string FechaTexto => Fecha.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-BO"));
    public string KilometrajeTexto => $"{Kilometraje:N0} km";
}
