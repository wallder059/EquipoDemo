namespace MantenimientoVehicular.Mobile.Models;

public sealed class Vehiculo
{
    public required int Id { get; init; }
    public required string Placa { get; init; }
    public string Marca { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;

    public string Descripcion => string.IsNullOrWhiteSpace(Marca) && string.IsNullOrWhiteSpace(Modelo)
        ? Placa
        : $"{Placa} · {Marca} {Modelo}".Trim();
}
