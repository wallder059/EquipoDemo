using System.Net;

namespace MantenimientoVehicular.Mobile.Services;

public sealed class ApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
