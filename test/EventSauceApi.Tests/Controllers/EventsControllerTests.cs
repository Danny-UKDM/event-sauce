using System.Collections.Generic;
using System.Threading.Tasks;
using EventSauceApi.Controllers;
using EventSauceApi.Data;
using EventSauceApi.Data.Entities;
using EventSauceApi.Models;
using EventSauceApi.Models.Response;
using EventSauceApi.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EventSauceApi.Tests.Controllers;

public class EventsControllerTests
{
    private readonly Mock<IEventGetter> _eventGetter;
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        _eventGetter = new Mock<IEventGetter>();
        _controller = new EventsController(_eventGetter.Object);
    }

    [Fact]
    public async Task GetAllShouldReturnInternalServerErrorIfQueryWasUnsuccessful()
    {
        _eventGetter
            .Setup(getter => getter.GetAllAsync())
            .ReturnsAsync((false, new List<EventEntity>(0)));

        var result = await _controller.GetAll();

        result
            .Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetAllShouldReturnNotFoundIfNoEntitiesWereFound()
    {
        _eventGetter
            .Setup(getter => getter.GetAllAsync())
            .ReturnsAsync((true, new List<EventEntity>(0)));

        var result = await _controller.GetAll();

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAllShouldReturnOkWithResultsWhenEntitiesFound()
    {
        var (entity1, response1) = EventBuilder.Create(RiskFactor.High).Build();
        var (entity2, response2) = EventBuilder.Create(RiskFactor.Medium).Build();
        var (entity3, response3) = EventBuilder.Create(RiskFactor.Low).Build();

        _eventGetter
            .Setup(getter => getter.GetAllAsync())
            .ReturnsAsync((true, new List<EventEntity>{entity1, entity2, entity3}));

        var result = await _controller.GetAll();

        result
            .Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new List<EventResponse>{response1, response2, response3});
    }
}
