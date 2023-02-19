using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Web.Middleware;
using Xunit;

namespace Middleware.Tests;

public class AuthenticationTests : IAsyncLifetime
{
    IHost? host;

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services => { })
                    .Configure(app =>
                    {
                        app.UseMiddleware<Authenticator>();
                        app.Run(async context =>
                        {
                            await context.Response.WriteAsync("User authenticated.");
                        });
                    });
            })
            .StartAsync();
    }

    [Fact]
    public async Task MiddlewareTest_FailWhenNotAuthenticated()
    {
        HttpResponseMessage response = await host.GetTestClient().GetAsync("/");
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        string result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Authentication failed.", result);
    }

    [Fact]
    public async Task MiddlewareTest_Authenticated()
    {
        HttpResponseMessage response = await host.GetTestClient().GetAsync("/?username=user1&password=password1");
        string result = await response.Content.ReadAsStringAsync();
        Assert.Equal("User authenticated.", result);
    }

    [Fact]
    public async Task MiddlewareTest_NoPassword()
    {
        HttpResponseMessage response = await host.GetTestClient().GetAsync("/?username=user1");
        string result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Authentication failed.", result);
    }

    [Fact]
    public async Task MiddlewareTest_WrongCredentials()
    {
        HttpResponseMessage response = await host.GetTestClient().GetAsync("/?username=user5&password=password2");
        string result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Authentication failed.", result);
    }
}
