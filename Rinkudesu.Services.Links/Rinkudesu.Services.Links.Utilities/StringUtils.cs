using System;
using System.Text;

namespace Rinkudesu.Services.Links.Utilities;

public static class StringUtils
{
    public static string FromBase64(this string encoded)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
    }
}