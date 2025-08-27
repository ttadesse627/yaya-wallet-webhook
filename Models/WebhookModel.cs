namespace Yaya.Webhook.API.Models;

public class WebhookModel
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ReceivedAt { get; set; }

    public string? Cause { get; set; }
    
    public required string CustomerName { get; set; }
    
    public required string AccountName { get; set; }

    public string InvoiceUrl { get; set; } = string.Empty;
}