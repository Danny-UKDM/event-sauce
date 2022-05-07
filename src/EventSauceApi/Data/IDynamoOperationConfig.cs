using Amazon.DynamoDBv2.DataModel;

namespace EventSauceApi.Data;

public interface IDynamoOperationConfig
{
    DynamoDBOperationConfig Main();
}
