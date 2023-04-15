using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories;
using Rinkudesu.Services.Links.Repositories.Exceptions;
using Rinkudesu.Services.Links.Utilities;
using Xunit;

namespace Rinkudesu.Services.Links.Tests;

public class SharedLinkRepositoryTests : ContextTests
{
    private readonly SharedLinkRepository _repository;
    private readonly UserInfo _userInfo = new(Guid.NewGuid());
    private readonly List<Link> _links;

    public SharedLinkRepositoryTests()
    {
        _repository = new(_context);
        _links = GetLinks();
        _context.Links.AddRange(_links);
        _context.SaveChanges();
        _context.ClearTracked();
    }

    [Fact]
    public async Task GetLink_UserInfoNotSet_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.ShareLinkById(Guid.NewGuid()));
    }

    [Fact]
    public async Task ShareLinkById_LinkNotFound_Throws()
    {
        await Assert.ThrowsAsync<DataNotFoundException>(() => _repository.SetUserInfo(_userInfo).ShareLinkById(Guid.NewGuid()));
    }

    [Fact]
    public async Task ShareLinkById_UserNotAuthorised_Throws()
    {
        await Assert.ThrowsAsync<DataNotFoundException>(() => _repository.SetUserInfo(new UserInfo(Guid.NewGuid())).ShareLinkById(_links[0].Id));

        Assert.Null(await _context.Links.Where(l => l.Id == _links[0].Id).Select(l => l.ShareableKey).FirstAsync());
    }

    [Fact]
    public async Task ShareLinkById_LinkAlreadyShared_Throws()
    {
        await Assert.ThrowsAsync<DataAlreadyExistsException>(() =>
            _repository.SetUserInfo(_userInfo).ShareLinkById(_links[1].Id));

        Assert.NotNull(await _context.Links.Where(l => l.Id == _links[1].Id).Select(l => l.ShareableKey).FirstAsync());
    }

    [Fact]
    public async Task ShareLinkById_AbleToShare_Shared()
    {
        var result = await _repository.SetUserInfo(_userInfo).ShareLinkById(_links[0].Id);

        var fromDb = await _context.Links.Where(l => l.Id == _links[0].Id).Select(s => s.ShareableKey).FirstAsync();
        Assert.NotNull(fromDb);
        Assert.Equal(fromDb, result);
    }

    [Fact]
    public async Task UnshareLinkById_LinkNotFound_Throws()
    {
        await Assert.ThrowsAsync<DataNotFoundException>(() => _repository.SetUserInfo(_userInfo).UnshareLinkById(Guid.NewGuid()));
    }

    [Fact]
    public async Task UnshareLinkById_UserNotAuthorised_Throws()
    {
        await Assert.ThrowsAsync<DataNotFoundException>(() => _repository.SetUserInfo(new UserInfo(Guid.NewGuid())).UnshareLinkById(_links[1].Id));

        Assert.NotNull(await _context.Links.Where(l => l.Id == _links[1].Id).Select(l => l.ShareableKey).FirstAsync());
    }

    [Fact]
    public async Task UnshareLinkById_LinkNotShared_Throws()
    {
        await Assert.ThrowsAsync<DataAlreadyExistsException>(() =>
            _repository.SetUserInfo(_userInfo).UnshareLinkById(_links[0].Id));

        Assert.Null(await _context.Links.Where(l => l.Id == _links[0].Id).Select(l => l.ShareableKey).FirstAsync());
    }

    [Fact]
    public async Task UnshareLinkById_AbleToUnshare_Unshared()
    {
        await _repository.SetUserInfo(_userInfo).UnshareLinkById(_links[1].Id);

        var fromDb = await _context.Links.Where(l => l.Id == _links[1].Id).Select(s => s.ShareableKey).FirstAsync();
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task GetKey_LinkNotFound_Throws()
    {
        await Assert.ThrowsAsync<DataNotFoundException>(() => _repository.SetUserInfo(_userInfo).GetKey(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetKey_UserNotAuthorised_Throws()
    {
        await Assert.ThrowsAsync<DataNotFoundException>(() => _repository.SetUserInfo(new UserInfo(Guid.NewGuid())).GetKey(_links[1].Id));
    }

    [Fact]
    public async Task GetKey_LinkNotShared_Throws()
    {
        await Assert.ThrowsAsync<DataInvalidException>(() => _repository.SetUserInfo(_userInfo).GetKey(_links[0].Id));
    }

    [Fact]
    public async Task GetKey_LinkSharedUserAuthorised_ReturnsLink()
    {
        var link = await _repository.SetUserInfo(_userInfo).GetKey(_links[1].Id);

        Assert.Equal(_links[1].ShareableKey, link);
    }

    private List<Link> GetLinks() => new()
    {
        new() { CreatingUserId = _userInfo.UserId, LinkUrl = Guid.NewGuid().ToString() },
        new() { CreatingUserId = _userInfo.UserId, ShareableKey = "test", LinkUrl = Guid.NewGuid().ToString() },
        new() { CreatingUserId = Guid.NewGuid(), LinkUrl = Guid.NewGuid().ToString() },
        new() { CreatingUserId = Guid.NewGuid(), ShareableKey = "test", LinkUrl = Guid.NewGuid().ToString() },
    };
}
