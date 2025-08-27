namespace Yaya.Webhook.API;

public class YayaWebhookSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string AllowedIPs { get; set; } = string.Empty;
    public int SignatureToleranceSeconds { get; set; }

}