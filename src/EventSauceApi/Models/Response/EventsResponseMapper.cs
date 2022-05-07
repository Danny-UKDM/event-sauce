﻿using EventSauceApi.Data.Entities;

namespace EventSauceApi.Models.Response;

internal static class EventsResponseMapper
{
    public static IEnumerable<EventResponse> Map(IEnumerable<EventEntity> eventEntities)
    {
        var eventEntitiesArray = eventEntities as EventEntity[] ?? eventEntities.ToArray();

        if (!eventEntitiesArray.Any())
            return Enumerable.Empty<EventResponse>();

        var eventsResponse = new List<EventResponse>();

        eventsResponse.AddRange(eventEntitiesArray.Select(entity =>
        {
            var what = new What(entity.Detail, entity.RiskFactor);
            var when = new When(entity.OccurredOn, entity.CreatedOn);
            var where = new Where(entity.Area, entity.Sector, entity.Domain);
            var who = new Who(entity.People.Select(person => new Person(person.Name, person.EmailAddress)));

            return new EventResponse(what, when, where, who);
        }));

        return eventsResponse;
    }
}
