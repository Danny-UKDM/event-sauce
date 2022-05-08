using Xunit;

namespace EventSauceApi.AcceptanceTests;

[CollectionDefinition(nameof(ApiCollectionFixture))]
public class ApiCollectionFixture : ICollectionFixture<ApiApplicationFactory>
{
}
