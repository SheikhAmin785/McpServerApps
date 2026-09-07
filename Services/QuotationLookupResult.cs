using System.Text.Json;

namespace MeghnaMcpServer.Services;

public enum QuotationLookupStatus
{
    Found,
    NotFound,
    EmptyJson,
    InvalidJson
}

public readonly struct QuotationLookupResult
{
    public required QuotationLookupStatus Status { get; init; }
    public JsonElement? Data { get; init; }
}
