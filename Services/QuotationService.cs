using System.Text.Json;
using Dapper;
using MeghnaMcpServer.Data;

namespace MeghnaMcpServer.Services;

public interface IQuotationService
{
    Task<QuotationLookupResult> GetQuotationDataAsync(string quotationNumber, CancellationToken cancellationToken = default);
}

public class QuotationService : IQuotationService
{
    private readonly IOracleConnectionFactory _connectionFactory;
    private readonly ILogger<QuotationService> _logger;

  
    private const string Sql = @"
        SELECT QUOTATION_JSON
        FROM LIFECORE.CUSTOMER_ACCOUNTS
        WHERE QUOTATION_NUMBER = :QuotationNumber";

    public QuotationService(IOracleConnectionFactory connectionFactory, ILogger<QuotationService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<QuotationLookupResult> GetQuotationDataAsync(string quotationNumber, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: Sql,
            parameters: new { QuotationNumber = quotationNumber },
            cancellationToken: cancellationToken);

        
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(command);

        if (row is null)
        {
            return new QuotationLookupResult { Status = QuotationLookupStatus.NotFound };
        }

        string? quotationJson = row.QUOTATION_JSON;

        if (string.IsNullOrWhiteSpace(quotationJson))
        {
            _logger.LogWarning("QUOTATION_JSON empty for {QuotationNumber}", quotationNumber);
            return new QuotationLookupResult { Status = QuotationLookupStatus.EmptyJson };
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(quotationJson);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid QUOTATION_JSON for {QuotationNumber}", quotationNumber);
            return new QuotationLookupResult { Status = QuotationLookupStatus.InvalidJson };
        }

        using (doc)
        {
            if (!doc.RootElement.TryGetProperty("data", out var dataElement))
            {
                _logger.LogError("'data' node missing in QUOTATION_JSON for {QuotationNumber}", quotationNumber);
                return new QuotationLookupResult { Status = QuotationLookupStatus.InvalidJson };
            }


            var dataClone = dataElement.Clone();
            return new QuotationLookupResult { Status = QuotationLookupStatus.Found, Data = dataClone };
        }
    }
}
