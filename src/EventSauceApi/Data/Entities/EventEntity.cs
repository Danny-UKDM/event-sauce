using EventSauceApi.Models;

namespace EventSauceApi.Data.Entities;

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
    public Person[] People { get; set; } = Array.Empty<Person>();
}

public record Person
{
    public string PersonId { get; set; } = Guid.Empty.ToString();
    public string Name { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
}

public static class EventEntityExtensions
{
    public static EventEntity WithDefaultKeys(this EventEntity entity)
    {
        entity.PK = entity.EventType;
        entity.SK = entity.Id;

        return entity;
    }
}
