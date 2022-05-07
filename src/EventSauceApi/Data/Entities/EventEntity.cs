using Amazon.DynamoDBv2.DataModel;
using EventSauceApi.Models;

namespace EventSauceApi.Data.Entities;

[DynamoDBTable("event-sauce-events")]
public record EventEntity : DynamoEntity
{
    public string EventType { get; set; } = "EVENT";
    public string Id { get; set; } = Guid.Empty.ToString();
    public DateTime CreatedOn { get; set; }
    public DateTime OccurredOn { get; set; }
    public string Detail { get; set; } = string.Empty;
    public RiskFactor RiskFactor { get; set; }
    public string LocationId { get; set; } = Guid.Empty.ToString();
    public string Area { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public IEnumerable<Person> People { get; set; } = Enumerable.Empty<Person>();

    public record Person
    {
        public string PersonId { get; set; } = Guid.Empty.ToString();
        public string Name { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
    }
}
