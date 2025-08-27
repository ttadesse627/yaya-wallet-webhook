using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Yaya.Webhook.API.Models;

namespace Yaya.Webhook.API;

[ApiController]
[Route("api/webhook")]
public class WebhookController: ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly YayaWebhookSettings _settings;
    private readonly IWebhookProcessor _webhookProcessor;

    public WebhookController(ILogger<WebhookController> logger, IOptions<YayaWebhookSettings> settings, IWebhookProcessor webhookProcessor)
    {
        _logger = logger;
        _settings = settings.Value;
        _webhookProcessor = webhookProcessor;
    }

    [HttpPost("transaction")]
    public async Task<IActionResult> ReceiveNotification()
    {
        // Return 200 status code quickly to ignore delayed response before processing the logic.
        HttpContext.Response.StatusCode = StatusCodes.Status200OK;
        await HttpContext.Response.Body.FlushAsync();

        try
        {
            string rawRequestBody;
            using (var reader = new StreamReader(Request.Body))
            {
                rawRequestBody = await reader.ReadToEndAsync();
            }

            // Validate the webhook request
            var validationResult = await ValidateWebhookRequest(rawRequestBody);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning($"Webhook validation failed: {validationResult.ErrorMessage}");
                return BadRequest(validationResult.ErrorMessage);
            }

            _ = Task.Run(() => _webhookProcessor.ProcessWebhookAsync(validationResult.Payload));
            return Ok("Webhook received successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task<WebhookValidationResult> ValidateWebhookRequest(string rawRequestBody)
    {
        await Task.CompletedTask;
        var result = new WebhookValidationResult();

        // 1. Check IP address against whitelist
        if (!IsRequestFromTrustedIp(Request))
        {
            result.ErrorMessage = "Untrusted IP address";
            return result;
        }

        // 2. Verify the presence of the YAYA-SIGNATURE header
        if (!Request.Headers.TryGetValue("YAYA-SIGNATURE", out var headerSignature))
        {
            result.ErrorMessage = "Missing the signature";
            return result;
        }

        // 3. Get the timestamp sent from the server;
        using var doc = JsonDocument.Parse(rawRequestBody);
        var timestamp = doc.RootElement.GetProperty("timestamp").GetInt64();

        // 4. Check timestamp tolerance to prevent replay attacks
        if (!IsTimestampWithinTolerance(timestamp))
        {
            result.ErrorMessage = "Timestamp outside tolerance window";
            return result;
        }
        return result;
    }

    private bool IsRequestFromTrustedIp(HttpRequest request)
    {
        // Compare the server's IP (stored in the setting) with the request IP to ensure the request is from Yaya Wallet
        var clientIp = request.HttpContext.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(clientIp))
        {
            return _settings.AllowedIPs.Contains(clientIp);
        }

        return false;
    }
    private bool IsTimestampWithinTolerance(long timestamp)
    {
        var receivedTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
        var currentTime = DateTime.UtcNow;
        var difference = currentTime - receivedTime;
        return difference.TotalSeconds <= _settings.SignatureToleranceSeconds;
    }
}