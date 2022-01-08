using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using Rinkudesu.Services.Links.Utilities;

namespace Rinkudesu.Services.Links.Utils;

[ExcludeFromCodeCoverage]
#pragma warning disable CS1591
public static class ClaimsPrincipalExtensions
#pragma warning restore CS1591
{
    /// <summary>
    /// Returns the id of the current user
    /// </summary>
    public static string GetId(this ClaimsPrincipal user) => user.Claims.FirstOrDefault(c =>
        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? string.Empty;

    /// <summary>
    /// Tries to parse current user id as <see cref="Guid"/>
    /// </summary>
    public static bool TryGetIdAsGuid(this ClaimsPrincipal user, out Guid id) =>
        Guid.TryParse(user.GetId(), out id);

    /// <summary>
    /// Parses current user id as <see cref="Guid"/>
    /// </summary>
    public static Guid GetIdAsGuid(this ClaimsPrincipal user) => Guid.Parse(user.GetId());

    /// <summary>
    /// Transforms <see cref="ClaimsPrincipal"/> into <see cref="UserInfo"/>
    /// </summary>
    public static UserInfo GetUserInfo(this ClaimsPrincipal user) => new UserInfo(user.GetIdAsGuid());
}
