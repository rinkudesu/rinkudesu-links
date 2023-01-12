using System;
using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Gateways.Utils;

public static class StringUtils
{
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Design", "CA1054", MessageId = "URI-like parameters should not be strings")]
    public static Uri ToUri(this string uriString) => new Uri(uriString, UriKind.RelativeOrAbsolute);
}
