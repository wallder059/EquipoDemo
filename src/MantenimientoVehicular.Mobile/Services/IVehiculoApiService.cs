using MantenimientoVehicular.Mobile.Models;

namespace MantenimientoVehicular.Mobile.Services;

public interface IVehiculoApiService
{
    Task<IReadOnlyList<Vehiculo>> GetVehiculosAsync(CancellationToken cancellationToken = default);
    Task<HistorialMantenimiento> GetHistorialAsync(int vehiculoId, CancellationToken cancellationToken = default);
}
