using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Yaya.Webhook.API;

[ApiController]
[Route("api/webhook")]
public class WebhookController: ControllerBase
{

    [HttpPost("transaction")]
    public async Task<IActionResult> ReceiveNotification()
    {
        // Return 200 status code quickly to ignore delayed response before processing the logic.
        HttpContext.Response.StatusCode = StatusCodes.Status200OK;
        await HttpContext.Response.Body.FlushAsync();
        return Ok();
    }
}