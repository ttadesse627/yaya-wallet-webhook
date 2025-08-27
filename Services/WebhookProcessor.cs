using Yaya.Webhook.API.Models;

namespace Yaya.Webhook.API;
public interface IWebhookProcessor
{
    Task ProcessWebhookAsync(WebhookPayload payload);
}

public class WebhookProcessor(ILogger<WebhookProcessor> logger) : IWebhookProcessor
{
    private readonly ILogger<WebhookProcessor> _logger = logger;

    public async Task ProcessWebhookAsync(WebhookPayload payload)
    {
        await Task.CompletedTask;
        try
        {
            // Map the request to the model if needed for storing in database
            var transaction = MapToModelEntity(payload);

            // Log the information for now, but we could implement the notification through email or something like that.
            _logger.LogInformation($"Processed webhook for transaction {payload.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing webhook for transaction {payload.Id}");
        }
    }
    
    private static WebhookModel MapToModelEntity(WebhookPayload payload)
    {
        return new WebhookModel
        {
            Id = payload.Id,
            Amount = payload.Amount,
            Currency = payload.Currency,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(payload.CreatedAtTime).DateTime,
            ReceivedAt = DateTime.UtcNow,
            Cause = payload.Cause,
            CustomerName = payload.FullName,
            AccountName = payload.AccountName,
            InvoiceUrl = payload.InvoiceUrl
        };
    }
}