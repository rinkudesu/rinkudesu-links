using System;
using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Services.Links.Utilities;

[ExcludeFromCodeCoverage]
public static class DateTimeExtensions
{
    public static DateTime SetKindUtc(this DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}