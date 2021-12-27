﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace Rinkudesu.Services.Links.Utils;

[ExcludeFromCodeCoverage]
public static class ClaimsPrincipalExtensions
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
}
