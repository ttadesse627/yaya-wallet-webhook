using Yaya.Webhook.API;
using Yaya.Webhook.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:8000"); //used static host to ignore randomly generated Ports on startup
builder.Services.AddControllers();

//Dependency Injection
builder.Services.AddTransient<IWebhookProcessor, WebhookProcessor>(); 
builder.Services.AddTransient<IWebhookValidator, WebhookValidator>();
builder.Services.Configure<YayaWebhookSettings>( // Add setting services
    builder.Configuration.GetSection("YayaWallet"));

var app = builder.Build();

app.MapControllers();
app.Run();
