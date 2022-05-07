using Microsoft.Extensions.Options;

namespace EventSauceApi;

public record DefaultOptions : IOptions<DefaultOptions>
{
    public string TableName { get; init; } = string.Empty;
    public DefaultOptions Value => this;
}
