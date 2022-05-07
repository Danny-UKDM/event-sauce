using Amazon.DynamoDBv2.DataModel;
using EventSauceApi.Models;

namespace EventSauceApi.Data.Entities;

[DynamoDBTable("event-sauce-events")]
public record EventEntity : DynamoEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime OccurredOn { get; set; }
    public string Detail { get; set; } = string.Empty;
    public RiskFactor RiskFactor { get; set; }
    public Guid LocationId { get; set; }
    public string Area { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public IEnumerable<Person> People { get; set; } = Enumerable.Empty<Person>();

    public record Person
    {
        public Guid PersonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
    }
}
