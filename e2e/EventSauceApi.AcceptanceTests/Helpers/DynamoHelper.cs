using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using EventSauceApi.Data.Entities;

namespace EventSauceApi.AcceptanceTests.Helpers;

public class DynamoHelper : IDisposable
{
    private readonly DynamoDBContext _dbContext;
    private readonly DynamoDBOperationConfig _config;
    private const string MainTableName = "event-sauce-events";

    public DynamoHelper()
    {
        var amazonDynamoDbConfig = new AmazonDynamoDBConfig
        {
            ServiceURL = "http://127.0.0.1:4566/",
            AuthenticationRegion = "eu-west-1"
        };

        var dbClient = new AmazonDynamoDBClient(new BasicAWSCredentials("localstack", "localstack"), amazonDynamoDbConfig);

        _dbContext = new DynamoDBContext(dbClient);
        _config = new DynamoDBOperationConfig { OverrideTableName = MainTableName };
    }

    public Task PutEntitiesAsync<T>(params T[] entities) where T : DynamoEntity =>
        Task.WhenAll(entities.Select(entity => _dbContext.SaveAsync(entity, _config)).ToArray());

    public Task DeleteEntitiesAsync<T>(params T[] entities) where T : DynamoEntity =>
        Task.WhenAll(entities.Select(entity => _dbContext.DeleteAsync(entity, _config)).ToArray());

    public void Dispose() => _dbContext.Dispose();
}
