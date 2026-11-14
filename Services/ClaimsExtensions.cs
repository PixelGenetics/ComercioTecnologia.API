using System.Security.Claims;

namespace ComercioTecnologia.Api.Services;

public static class ClaimsExtensions
{
    /// <summary>Id del usuario autenticado (claim NameIdentifier del JWT).</summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? throw new UnauthorizedAccessException("Token sin identificador de usuario."));
}
