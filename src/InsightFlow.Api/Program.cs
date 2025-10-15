using System.Text;
using InsightFlow.Core.Configuration;
using InsightFlow.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

var settings = new AppSettings
{
    StorageRoot = builder.Configuration["STORAGE_ROOT"] ?? "./data/uploads",
    DatabasePath = builder.Configuration["DATABASE_PATH"] ?? "./data/insightflow.db",
    OnnxModelPath = builder.Configuration["ONNX_MODEL_PATH"] ?? "./models/embedding.onnx",
    OcrTessdataPath = builder.Configuration["OCR_TESSDATA_PATH"] ?? "./models/tessdata"
};

var jwtSettings = new JwtSettings
{
    Secret = builder.Configuration["JWT_SECRET"] ?? "replace-with-32-char-minimum-secret-key",
    Issuer = builder.Configuration["JWT_ISSUER"] ?? "InsightFlow",
    Audience = builder.Configuration["JWT_AUDIENCE"] ?? "InsightFlowClients"
};

builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddInsightFlowInfrastructure(settings);
builder.Services.AddInsightFlowAuth(jwtSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AnalystOrAdmin", policy => policy.RequireRole("Admin", "Analyst"));
});

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52_428_800;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<InsightFlow.Infrastructure.Persistence.DatabaseInitializer>();
    initializer.Initialize();
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
