using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventSauceApi.Data.Entities;
using EventSauceApi.Models;
using EventSauceApi.Models.Response;
using FluentAssertions;
using Xunit;
using EventBuilder = EventSauceApi.AcceptanceTests.Helpers.EventBuilder;

namespace EventSauceApi.AcceptanceTests.Controllers.Events.GetAll;

[Collection(nameof(ApiCollectionFixture))]
public class GivenEventsExist : IClassFixture<GivenEventsExist.Invocation>
{
    public sealed class Invocation : IAsyncLifetime
    {
        public (EventEntity entity, EventResponse response) Event1;
        public (EventEntity entity, EventResponse response) Event2;
        public (EventEntity entity, EventResponse response) Event3;
        public HttpResponseMessage Response = new();

        private readonly ApiApplicationFactory _fixture;

        public Invocation(ApiApplicationFactory fixture) => _fixture = fixture;

        public async Task InitializeAsync()
        {
            Event1 = EventBuilder.Create(RiskFactor.High).Build();
            Event2 = EventBuilder.Create(RiskFactor.Medium).Build();
            Event3 = EventBuilder.Create(RiskFactor.Low).Build();

            await _fixture.DbHelper.PutEntitiesAsync(Event1.entity, Event2.entity, Event3.entity);

            Response = await _fixture.ApiClient.GetAsync("events");
        }

        public async Task DisposeAsync() =>
            await _fixture.DbHelper.DeleteEntitiesAsync(Event1.entity, Event2.entity, Event3.entity);
    }

    private readonly Invocation _invocation;

    public GivenEventsExist(Invocation invocation) => _invocation = invocation;

    [Fact]
    public void ThenTheGetAllEndpointReturns200Ok() =>
        _invocation.Response.StatusCode.Should().Be(HttpStatusCode.OK);
}
