using Amazon.DynamoDBv2.DataModel;

namespace EventSauceApi.Data.Entities;

public abstract record DynamoEntity
{
    [DynamoDBHashKey]
    public string PK { get; set; }

    [DynamoDBRangeKey]
    public string SK { get; set; }

    [DynamoDBGlobalSecondaryIndexHashKey("GSI1")]
    public string GSI1PK { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("GSI1")]
    public string GSI1SK { get; set; }

    [DynamoDBGlobalSecondaryIndexHashKey("GSI2")]
    public string GSI2PK { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("GSI2")]
    public string GSI2SK { get; set; }

    [DynamoDBGlobalSecondaryIndexHashKey("GSI3")]
    public string GSI3PK { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("GSI3")]
    public string GSI3SK { get; set; }
}
