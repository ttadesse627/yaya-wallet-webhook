using System.Text.Json;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Yaya.Webhook.API.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;

namespace Yaya.Webhook.API.Services;
public interface IWebhookValidator
{
    Task<WebhookValidationResult> ValidateWebhookRequest(string request, HttpRequest httpRequest);
}
public class WebhookValidator: IWebhookValidator
{
    private readonly YayaWebhookSettings _settings;
    public WebhookValidator(IOptions<YayaWebhookSettings> settings)
    {
        _settings = settings.Value;
    }
    public async Task<WebhookValidationResult> ValidateWebhookRequest(string request, HttpRequest httpRequest)
    {
        await Task.CompletedTask;
        var result = new WebhookValidationResult();

        // 1. Check IP address against whitelist
        if (!IsRequestFromTrustedIp(httpRequest))
        {
            result.ErrorMessage = "Untrusted IP address";
            return result;
        }

        // 2. Verify the presence of the YAYA-SIGNATURE header
        if (!httpRequest.Headers.TryGetValue("YAYA-SIGNATURE", out var headerSignature))
        {
            result.ErrorMessage = "Missing the signature";
            return result;
        }

        // 3. Get the timestamp sent from the server;
        using var doc = JsonDocument.Parse(request);
        var timestamp = doc.RootElement.GetProperty("timestamp").GetInt64();

        // 4. Check timestamp tolerance to prevent replay attacks
        if (!IsTimestampWithinTolerance(timestamp))
        {
            result.ErrorMessage = "Timestamp outside tolerance window";
            return result;
        }

        // 5. Compute expected signature
        var expectedSignature = ComputeSignature(request);

        // 6. Compare signatures
        if (!SecureEquals(expectedSignature, headerSignature!))
        {
            result.ErrorMessage = "Signature verification failed";
            return result;
        }

        // 7. Deserialize payload if validation passed
        try
        {
            result.Payload = JsonConvert.DeserializeObject<WebhookPayload>(request)!;
            result.IsValid = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Invalid payload format: {ex.Message}";
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
    private string ComputeSignature(string payload)
    {

        var payloadValues = ExtractValuesFromJson(payload);
        var signedPayload = string.Join("", payloadValues);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        return Convert.ToBase64String(hash);
    }
    private static List<string> ExtractValuesFromJson(string json)
    {
        var values = new List<string>();
        var jObject = JsonConvert.DeserializeObject<JObject>(json);

        if (jObject != null)
        {
            foreach (var property in jObject.Properties())
            {
                values.Add(property.Value.ToString());
            }
        }

        return values;
    }

    private static bool SecureEquals(string a, string b)
    {
        // Used constant-time comparison to prevent timing attacks
        if (a.Length != b.Length) return false;

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;

    }
}
