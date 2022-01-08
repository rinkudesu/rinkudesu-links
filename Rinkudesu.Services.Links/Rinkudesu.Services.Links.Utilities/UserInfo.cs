using System;
using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Services.Links.Utilities;

/// <summary>
/// Stores information about the user
/// </summary>
[ExcludeFromCodeCoverage]
public class UserInfo
{
    public Guid UserId { get; }

    public UserInfo(string userId) : this(Guid.Parse(userId))
    {
    }

    public UserInfo(Guid userId)
    {
        UserId = userId;
    }
}