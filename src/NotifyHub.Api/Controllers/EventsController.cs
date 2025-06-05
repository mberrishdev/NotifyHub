using Microsoft.AspNetCore.Mvc;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Models;

namespace NotifyHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public EventsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Event @event)
    {
        await _notificationService.ProcessEventAsync(@event);
        return Ok();
    }
} 