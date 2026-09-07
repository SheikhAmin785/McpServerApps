using System.ComponentModel;
using System.Text.Json;
using MeghnaMcpServer.Services;
using ModelContextProtocol.Server;

namespace MeghnaMcpServer.MCP;

[McpServerToolType]
public class QuotationTools
{
    private readonly IQuotationService _quotationService;
    private readonly ILogger<QuotationTools> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public QuotationTools(IQuotationService quotationService, ILogger<QuotationTools> logger)
    {
        _quotationService = quotationService;
        _logger = logger;
    }

    [McpServerTool(Name = "get_quotation")]
    [Description(
        "Retrieves an insurance quotation from the database using its quotation number. " +
        "Use this tool whenever the user asks for details, premium, sum assured, plan, or supplementary " +
        "benefits of a specific quotation, given a quotation number such as ADC260824000043.")]
    public async Task<string> GetQuotationAsync(
        [Description("The unique quotation number to look up, e.g. ADC260824000043.")]
        string quotationNumber,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(quotationNumber))
        {
            return Fail("Quotation number is required.");
        }

        var trimmedNumber = quotationNumber.Trim();

        try
        {
            var result = await _quotationService.GetQuotationDataAsync(trimmedNumber, cancellationToken);

            return result.Status switch
            {
                QuotationLookupStatus.NotFound or QuotationLookupStatus.EmptyJson =>
                    Fail($"Quotation '{trimmedNumber}' was not found."),

                QuotationLookupStatus.InvalidJson =>
                    Fail("Quotation data could not be read for this quotation number. Please contact support."),

                QuotationLookupStatus.Found =>
                    JsonSerializer.Serialize(new
                    {
                        success = true,
                        data = result.Data!.Value
                    }, JsonOptions),

                _ => Fail("Unexpected error retrieving quotation.")
            };
        }
        catch (Exception ex)
        {
           
            _logger.LogError(ex, "Error retrieving quotation {QuotationNumber}", trimmedNumber);
            return Fail("An internal error occurred while retrieving the quotation. Please try again later.");
        }
    }

    private static string Fail(string message) =>
        JsonSerializer.Serialize(new { success = false, message }, JsonOptions);
}
