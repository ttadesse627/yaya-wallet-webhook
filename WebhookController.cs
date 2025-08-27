using Microsoft.AspNetCore.Mvc;
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
        string rawRequestBody;
        using (var reader = new StreamReader(Request.Body))
        {
            rawRequestBody = await reader.ReadToEndAsync();
        }

        var signature = Request.Headers.TryGetValue("YAYA-SIGNATURE", out var sig) ? sig.ToString() : null;
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        // immediately return 200 OK
        _ = Task.Run(async () =>
        {
            try
            {
                var validationResult = await _webhookValidator.ValidateWebhookRequest(rawRequestBody, signature, remoteIp);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning($"Webhook validation failed: {validationResult.ErrorMessage}");
                    return;
                }

                await _webhookProcessor.ProcessWebhookAsync(validationResult.Payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
            }
        });

        return Ok();
    }


}