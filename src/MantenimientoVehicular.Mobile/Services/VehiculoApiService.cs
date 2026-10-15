using System.Globalization;
using System.Net;
using System.Text.Json;
using MantenimientoVehicular.Mobile.Models;

namespace MantenimientoVehicular.Mobile.Services;

public sealed class VehiculoApiService(HttpClient httpClient) : IVehiculoApiService
{
    private static readonly JsonDocumentOptions JsonOptions = new() { AllowTrailingCommas = true };

    public async Task<IReadOnlyList<Vehiculo>> GetVehiculosAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/vehiculos", cancellationToken);
        await EnsureSuccessAsync(response);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken), JsonOptions);
        return GetArray(document.RootElement).EnumerateArray().Select(ParseVehiculo).ToList();
    }

    public async Task<HistorialMantenimiento> GetHistorialAsync(int vehiculoId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/vehiculos/{vehiculoId}/mantenimientos", cancellationToken);
        await EnsureSuccessAsync(response);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken), JsonOptions);
        var root = UnwrapData(document.RootElement);
        var records = root.ValueKind == JsonValueKind.Array
            ? root.EnumerateArray().Select(ParseMantenimiento).OrderByDescending(item => item.Fecha).ToList()
            : GetPropertyArray(root, "mantenimientos", "historial", "items").EnumerateArray().Select(ParseMantenimiento).OrderByDescending(item => item.Fecha).ToList();
        var suggestion = root.ValueKind == JsonValueKind.Object && TryGetProperty(root, out var next, "proximo_mantenimiento", "proximoMantenimiento") && next.ValueKind == JsonValueKind.Object
            ? ParseProximoMantenimiento(next)
            : null;
        return new HistorialMantenimiento { Registros = records, ProximoMantenimiento = suggestion };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }
        var detail = await response.Content.ReadAsStringAsync();
        throw new ApiException(response.StatusCode, string.IsNullOrWhiteSpace(detail)
            ? "No fue posible obtener los datos solicitados."
            : $"La API respondió con un error ({(int)response.StatusCode}).");
    }

    private static Vehiculo ParseVehiculo(JsonElement item) => new()
    {
        Id = GetInt(item, "id"),
        Placa = GetString(item, "placa"),
        Marca = GetString(item, "marca"),
        Modelo = GetString(item, "modelo"),
    };

    private static Mantenimiento ParseMantenimiento(JsonElement item) => new()
    {
        Fecha = GetDate(item, "fecha", "fecha_servicio"),
        TipoServicio = GetString(item, "tipo_servicio", "tipoServicio", "servicio"),
        Kilometraje = GetInt(item, "kilometraje", "kilometraje_actual"),
    };

    private static ProximoMantenimiento ParseProximoMantenimiento(JsonElement item) => new()
    {
        TipoServicio = GetString(item, "tipo_servicio", "tipoServicio", "servicio"),
        KilometrajeSugerido = GetNullableInt(item, "kilometraje_sugerido", "kilometrajeSugerido"),
        FechaSugerida = GetNullableDate(item, "fecha_sugerida", "fechaSugerida"),
    };

    private static JsonElement UnwrapData(JsonElement root)
    {
        return root.ValueKind == JsonValueKind.Object && TryGetProperty(root, out var data, "data") ? data : root;
    }

    private static JsonElement GetArray(JsonElement root)
    {
        var data = UnwrapData(root);
        if (data.ValueKind == JsonValueKind.Array)
        {
            return data;
        }
        return GetPropertyArray(data, "vehiculos", "items", "data");
    }

    private static JsonElement GetPropertyArray(JsonElement item, params string[] names)
    {
        if (TryGetProperty(item, out var value, names) && value.ValueKind == JsonValueKind.Array)
        {
            return value;
        }
        throw new ApiException(HttpStatusCode.OK, "La API no devolvió una lista válida.");
    }

    private static bool TryGetProperty(JsonElement element, out JsonElement value, params string[] names)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (names.Any(name => string.Equals(name, property.Name, StringComparison.OrdinalIgnoreCase)))
            {
                value = property.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private static string GetString(JsonElement item, params string[] names)
    {
        return TryGetProperty(item, out var value, names) ? value.ToString() : string.Empty;
    }

    private static int GetInt(JsonElement item, params string[] names)
    {
        return GetNullableInt(item, names) ?? 0;
    }

    private static int? GetNullableInt(JsonElement item, params string[] names)
    {
        if (!TryGetProperty(item, out var value, names) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }
        return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number) ? number : null;
    }

    private static DateTime GetDate(JsonElement item, params string[] names) => GetNullableDate(item, names) ?? DateTime.MinValue;

    private static DateTime? GetNullableDate(JsonElement item, params string[] names)
    {
        if (!TryGetProperty(item, out var value, names) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }
        return DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date) ? date : null;
    }
}
