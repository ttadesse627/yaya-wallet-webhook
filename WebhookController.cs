using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Yaya.Webhook.API.Services;


namespace Yaya.Webhook.API;

[ApiController]
[Route("api/webhook")]
public class WebhookController: ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly IWebhookProcessor _webhookProcessor;
    private readonly IWebhookValidator _webhookValidator;

    public WebhookController(ILogger<WebhookController> logger, IWebhookProcessor webhookProcessor, IWebhookValidator webhookValidator)
    {
        _logger = logger;
        _webhookProcessor = webhookProcessor;
        _webhookValidator = webhookValidator;
    }

    [HttpPost("transaction")]
    public async Task<IActionResult> ReceiveNotification()
    {
        // Return 200 status code quickly to ignore delayed response before processing the logic.
        // HttpContext.Response.StatusCode = StatusCodes.Status200OK;
        // await HttpContext.Response.Body.FlushAsync();

        try
        {
            string rawRequestBody;
            using (var reader = new StreamReader(Request.Body))
            {
                rawRequestBody = await reader.ReadToEndAsync();
            }

            // Validate the webhook request
            var validationResult = await _webhookValidator.ValidateWebhookRequest(rawRequestBody, Request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning($"Webhook validation failed: {validationResult.ErrorMessage}");
                return BadRequest(validationResult.ErrorMessage);
            }

            _ = Task.Run(() => _webhookProcessor.ProcessWebhookAsync(validationResult.Payload));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500, "Internal server error");
        }

        return Ok();
    }

}