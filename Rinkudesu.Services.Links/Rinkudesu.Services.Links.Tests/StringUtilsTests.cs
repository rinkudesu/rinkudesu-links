using Rinkudesu.Services.Links.Utilities;
using Xunit;

namespace Rinkudesu.Services.Links.Tests;

public class StringUtilsTests
{
    [Theory]
    [InlineData("OHJ5cDk4MzQgeTU5ODI0M3kgNXQyYTNydDktMjM0MHQgPTJhM3Qgb1t3J2E7ZWYuICdhXHdmIDthd2Ygd2FdLmYgbFt3YTtmIGwvYXdbZjsud2FsLTMuLyAwdzJhLi80M3RmbCB3ZSBhdg==", @"8ryp9834 y598243y 5t2a3rt9-2340t =2a3t o[w'a;ef. 'a\wf ;awf wa].f l[wa;f l/aw[f;.wal-3./ 0w2a./43tfl we av")]
    public void FromBase64_ConvertsCorrectly(string base64, string expected)
    {
        Assert.Equal(expected, base64.FromBase64());
    }
}