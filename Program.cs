using MeghnaMcpServer.Data;
using MeghnaMcpServer.MCP;
using MeghnaMcpServer.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();
builder.Logging.AddConsole();


builder.Services.AddSingleton<IOracleConnectionFactory, OracleConnectionFactory>();
builder.Services.AddScoped<IQuotationService, QuotationService>();


builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<QuotationTools>();

var app = builder.Build();


app.MapGet("/", () => Results.Ok(new
{
    application = "Meghna MCP Server",
    version = "1.0",
    framework = ".NET 10",
    authentication = false,
    status = "Running"
}));


app.MapMcp("/mcp");

app.Run();
