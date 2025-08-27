namespace Yaya.Webhook.API.Models;

public class WebhookValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public WebhookPayload Payload { get; set; } = null!;
}