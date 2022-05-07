using Amazon.DynamoDBv2.DataModel;
using EventSauceApi.Data.Entities;

namespace EventSauceApi.Data;

internal class EventGetter : IEventGetter
{
    private readonly IDynamoDBContext _dbContext;
    private readonly IDynamoOperationConfig _operationConfig;
    private readonly ILogger<EventGetter> _logger;

    public EventGetter(IDynamoDBContext dbContext, IDynamoOperationConfig operationConfig, ILogger<EventGetter> logger)
    {
        _dbContext = dbContext;
        _operationConfig = operationConfig;
        _logger = logger;
    }

    public async Task<(bool querySuccess, IReadOnlyList<EventEntity> results)> GetAllAsync()
    {
        try
        {
            var results = new List<EventEntity>();

            var asyncSearch = _dbContext.QueryAsync<EventEntity>(Constants.EventDynamoKey, _operationConfig.Main());

            do
            {
                results.AddRange(await asyncSearch.GetNextSetAsync());
            } while (!asyncSearch.IsDone);

            return (true, results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExceptionType} thrown while attempting to fetch all events: {Message}", ex.GetType().Name, ex.Message);
            return (false, new List<EventEntity>(0));
        }
    }
}
