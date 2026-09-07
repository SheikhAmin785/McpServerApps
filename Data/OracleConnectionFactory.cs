using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace MeghnaMcpServer.Data;


public interface IOracleConnectionFactory
{
    IDbConnection CreateConnection();
}

public class OracleConnectionFactory : IOracleConnectionFactory
{
    private readonly string _connectionString;

    public OracleConnectionFactory(IConfiguration configuration)
    {

        _connectionString = configuration.GetConnectionString("OracleConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:OracleConnection is not configured. Check appsettings.json.");
    }

    public IDbConnection CreateConnection()
    {
        return new OracleConnection(_connectionString);
    }
}
