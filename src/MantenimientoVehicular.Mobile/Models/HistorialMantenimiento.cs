namespace MantenimientoVehicular.Mobile.Models;

public sealed class HistorialMantenimiento
{
    public required IReadOnlyList<Mantenimiento> Registros { get; init; }
    public ProximoMantenimiento? ProximoMantenimiento { get; init; }
}
