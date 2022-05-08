using System;
using System.Collections.Generic;
using System.Linq;
using EventSauceApi.Data.Entities;
using EventSauceApi.Models;
using EventSauceApi.Models.Response;

namespace EventSauceApi.AcceptanceTests.Helpers;

public class EventBuilder
{
    private readonly string _id;
    private readonly DateTime _createdOn;
    private readonly DateTime _occurredOn;
    private readonly string _detail;
    private readonly RiskFactor _riskFactor;
    private readonly string _locationId;
    private readonly string _area;
    private readonly string _sector;
    private readonly string _domain;
    private readonly List<EventEntity.Person> _people;

    private EventBuilder(RiskFactor riskFactor)
    {
        _id = Guid.NewGuid().ToString();
        _createdOn = DateTime.UtcNow;
        _occurredOn = DateTime.UnixEpoch.AddMinutes(-1);
        _detail = $"Some Cool Detail - Risk Factor: {riskFactor:G}";
        _riskFactor = riskFactor;
        _locationId = Guid.NewGuid().ToString();
        _area = $"Some Cool Area {Guid.NewGuid():N}";
        _sector = $"Some Cool Sector {Guid.NewGuid():N}";
        _domain = $"Some Cool Domain {Guid.NewGuid():N}";
        _people = new List<EventEntity.Person>
        {
            new()
            {
                Name = "Carole Baskin",
                EmailAddress = "carole@baskin.com",
                PersonId = Guid.NewGuid().ToString()
            },
            new()
            {
                Name = "Bob Ross",
                EmailAddress = "bob@ross.com",
                PersonId = Guid.NewGuid().ToString()
            },
            new()
            {
                Name = "David Attenborough",
                EmailAddress = "david@attenborough.com",
                PersonId = Guid.NewGuid().ToString()
            }
        };
    }

    public static EventBuilder Create(RiskFactor riskFactor) => new(riskFactor);

    public (EventEntity entity, EventResponse response) Build() =>
    (
        new EventEntity
        {
            Id = _id,
            CreatedOn = _createdOn,
            OccurredOn = _occurredOn,
            Detail = _detail,
            RiskFactor = _riskFactor,
            LocationId = _locationId,
            Area = _area,
            Sector = _sector,
            Domain = _domain,
            People = _people
        }.WithDefaultKeys(),
        new EventResponse(
            new What(_detail, _riskFactor),
            new When(_occurredOn, _createdOn),
            new Where(_area, _sector, _domain),
            new Who(_people.Select(person => new Person(person.Name, person.EmailAddress))))
    );
}
