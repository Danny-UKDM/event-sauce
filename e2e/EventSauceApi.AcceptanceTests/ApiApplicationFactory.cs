using System.Net.Http;
using EventSauceApi.AcceptanceTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EventSauceApi.AcceptanceTests;

public class ApiApplicationFactory : WebApplicationFactory<Program>
{
    public ApiApplicationFactory()
    {
        DbHelper = new DynamoHelper();
        ApiClient = CreateClient();
    }

    public DynamoHelper DbHelper { get; }
    public HttpClient ApiClient { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DbHelper.Dispose();
            ApiClient?.Dispose();
        }

        base.Dispose(disposing);
    }
}
