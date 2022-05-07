using System.Net;
using EventSauceApi.Data;
using EventSauceApi.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace EventSauceApi.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventGetter _eventGetter;

    public EventsController(IEventGetter eventGetter) => _eventGetter = eventGetter;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var (querySuccess, results) = await _eventGetter.GetAllAsync();

        if (!querySuccess)
            return StatusCode((int)HttpStatusCode.InternalServerError);

        if (!results.Any())
            return NotFound();

        var response = EventsResponseMapper.Map(results);

        return Ok(response);
    }
}
