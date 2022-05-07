using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Options;

namespace EventSauceApi.Data;

internal class DynamoOperationConfig : IDynamoOperationConfig
{
    private readonly string _tableName;
    private static DynamoDBOperationConfig? _main;

    public DynamoOperationConfig(IOptions<DefaultOptions> options) => _tableName = options.Value.TableName;

    public DynamoDBOperationConfig Main()
    {
        if (_main != null) return _main;

        _main = new DynamoDBOperationConfig
        {
            OverrideTableName = _tableName
        };

        return _main;
    }
}
