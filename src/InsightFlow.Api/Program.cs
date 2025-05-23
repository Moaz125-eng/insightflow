using InsightFlow.Api.Controllers;
using InsightFlow.Core.Configuration;
using InsightFlow.Infrastructure.DependencyInjection;
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

builder.Services.AddSingleton(settings);
builder.Services.AddInsightFlowInfrastructure(settings);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
