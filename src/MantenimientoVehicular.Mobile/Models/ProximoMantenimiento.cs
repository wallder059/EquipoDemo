using System.Globalization;

namespace MantenimientoVehicular.Mobile.Models;

public sealed class ProximoMantenimiento
{
    public required string TipoServicio { get; init; }
    public int? KilometrajeSugerido { get; init; }
    public DateTime? FechaSugerida { get; init; }

    public string Detalle
    {
        get
        {
            var details = new List<string>();
            if (KilometrajeSugerido is not null)
            {
                details.Add($"Al llegar a {KilometrajeSugerido:N0} km");
            }
            if (FechaSugerida is not null)
            {
                details.Add($"hasta el {FechaSugerida.Value.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-BO"))}");
            }
            return details.Count == 0 ? "Revisa el plan de mantenimiento del vehículo." : string.Join(" · ", details);
        }
    }
}
