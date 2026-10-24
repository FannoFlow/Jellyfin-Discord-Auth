using System;
using System.Linq;
using JellyfinDiscordAuth.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JellyfinDiscordAuth.Tests;

public class DiscordAuthSessionServiceTests
{
    [Fact]
    public void CreateSession_ProducesHighEntropyHexCode()
    {
        var service = new DiscordAuthSessionService(NullLogger<DiscordAuthSessionService>.Instance);

        var code = service.CreateSession("access-token", Guid.NewGuid(), "server-id", "test-user");

        Assert.Equal(64, code.Length);
        Assert.All(code, character => Assert.True(
            character is >= '0' and <= '9' or >= 'a' and <= 'f',
            $"Unexpected character in exchange code: {character}"));
    }

    [Fact]
    public void CreateSession_GeneratesDistinctCodes()
    {
        var service = new DiscordAuthSessionService(NullLogger<DiscordAuthSessionService>.Instance);

        var codes = Enumerable.Range(0, 100)
            .Select(_ => service.CreateSession("access-token", Guid.NewGuid(), "server-id", "test-user"))
            .ToArray();

        Assert.Equal(codes.Length, codes.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void RetrieveAndRemoveSession_IsSingleUse()
    {
        var service = new DiscordAuthSessionService(NullLogger<DiscordAuthSessionService>.Instance);
        var userId = Guid.NewGuid();
        var code = service.CreateSession("test-access-token", userId, "test-server", "test-user");

        var session = service.RetrieveAndRemoveSession(code);

        Assert.NotNull(session);
        Assert.Equal("test-access-token", session.AccessToken);
        Assert.Equal(userId, session.UserId);
        Assert.Equal("test-server", session.ServerId);
        Assert.Equal("test-user", session.Username);
        Assert.Null(service.RetrieveAndRemoveSession(code));
    }

    [Fact]
    public void RetrieveAndRemoveSession_RejectsUnknownCode()
    {
        var service = new DiscordAuthSessionService(NullLogger<DiscordAuthSessionService>.Instance);

        Assert.Null(service.RetrieveAndRemoveSession("not-a-valid-code"));
    }
}
