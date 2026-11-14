using System.Text.Json;
using ComercioTecnologia.Api.Dtos;

namespace ComercioTecnologia.Api.Services;

/// <summary>
/// Lee y escribe la columna JSONB productos.imagenes:
/// [{ "url": "https://...", "orden": 1, "principal": true }, ...]
/// </summary>
public static class ImagenesProducto
{
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);

    /// <summary>Lista ordenada; si el JSON está vacío o mal formado devuelve una lista vacía.</summary>
    public static List<ImagenDto> Leer(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            return (JsonSerializer.Deserialize<List<ImagenDto>>(json, Opciones) ?? new())
                .Where(i => !string.IsNullOrWhiteSpace(i.Url))
                .OrderBy(i => i.Orden)
                .ToList();
        }
        catch (JsonException)
        {
            return new();
        }
    }

    /// <summary>Renumera el orden (1..n), garantiza una sola principal y serializa.</summary>
    public static string? Escribir(List<ImagenDto> imagenes)
    {
        if (imagenes.Count == 0) return null;

        var indicePrincipal = Math.Max(imagenes.FindIndex(i => i.Principal), 0);
        var normalizadas = imagenes
            .Select((img, i) => img with { Orden = i + 1, Principal = i == indicePrincipal })
            .ToList();
        return JsonSerializer.Serialize(normalizadas, Opciones);
    }

    public static string? Principal(string? json)
    {
        var lista = Leer(json);
        return (lista.FirstOrDefault(i => i.Principal) ?? lista.FirstOrDefault())?.Url;
    }

    /// <summary>Solo URLs absolutas http/https.</summary>
    public static bool UrlValida(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
