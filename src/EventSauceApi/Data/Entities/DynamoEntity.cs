using Amazon.DynamoDBv2.DataModel;
// ReSharper disable InconsistentNaming

namespace EventSauceApi.Data.Entities;

public abstract record DynamoEntity
{
    [DynamoDBHashKey]
    public string PK { get; set; } = string.Empty;

    [DynamoDBRangeKey]
    public string SK { get; set; } = string.Empty;
}
