using Newtonsoft.Json;

namespace Yaya.Webhook.API.Models;
public record WebhookPayload
{
    // Used JsonProperty to populate json properties to Model class fields
    [JsonProperty("id")]
    public Guid Id { get; set; }
    
    [JsonProperty("amount")]
    public decimal Amount { get; set; }
    
    [JsonProperty("currency")]
    public required string Currency { get; set; }
    
    [JsonProperty("created_at_time")]
    public long CreatedAtTime { get; set; }
    
    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }
    
    [JsonProperty("cause")]
    public string? Cause { get; set; }
    
    [JsonProperty("full_name")]
    public required string FullName { get; set; }
    
    [JsonProperty("account_name")]
    public required string AccountName { get; set; }

    [JsonProperty("invoice_url")]
    public string InvoiceUrl { get; set; } = string.Empty;
}