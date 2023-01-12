using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Rinkudesu.Services.Links.Utils;

public static class HttpContextExtensions
{
    [ExcludeFromCodeCoverage]
    public static async Task<string?> GetJwt(this HttpContext context)
    {
        return await context.GetTokenAsync("access_token");
    }
}
