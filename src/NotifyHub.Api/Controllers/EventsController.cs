using Microsoft.AspNetCore.Mvc;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Models;

namespace NotifyHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(INotificationService notificationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Event @event)
    {
        await notificationService.ProcessEventAsync(@event);
        return Ok();
    }
} 