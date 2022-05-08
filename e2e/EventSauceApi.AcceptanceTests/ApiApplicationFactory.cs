using System.Net.Http;
using EventSauceApi.AcceptanceTests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EventSauceApi.AcceptanceTests;

public class ApiApplicationFactory : WebApplicationFactory<Program>
{
    public ApiApplicationFactory()
    {
        ApiClient = CreateClient();
        DbHelper = new DynamoHelper();
    }

    public HttpClient ApiClient { get; }
    public DynamoHelper DbHelper { get; }

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Acceptance");

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ApiClient?.Dispose();
            DbHelper.Dispose();
        }

        base.Dispose(disposing);
    }
}
