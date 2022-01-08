using Rinkudesu.Services.Links.Models;
using Xunit;

namespace Rinkudesu.Services.Links.Tests;

public class LinkTests
{
    [Fact]
    public void Link_GenerateShareableKey_ReturnsKeyOfRequiredLength()
    {
        var link = new Link();

        var key = link.GenerateShareableKey();

        Assert.Equal(48, key.Length);
    }

    [Fact]
    public void LinkGenerateShareableKey_LinkGeneratedRegenerateFalse_ReturnsExistingLink()
    {
        var link = new Link();
        var original = link.GenerateShareableKey();

        var newLink = link.GenerateShareableKey(false);
        Assert.Same(original, newLink);
    }

    [Fact]
    public void LinkGenerateShareableKey_LinkGeneratedRegenerateTrue_ReturnsNewLink()
    {
        var link = new Link();
        var original = link.GenerateShareableKey();

        var newLink = link.GenerateShareableKey(true);
        Assert.NotEqual(original, newLink);
    }
}