using EventSauceApi.Data.Entities;

namespace EventSauceApi.Data;

public interface IEventGetter
{
    Task<(bool querySuccess , List<EventEntity> results)> GetAllAsync();
}
