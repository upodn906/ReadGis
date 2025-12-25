using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Reader.Console
{
    public class SyncService
    {
        private readonly SyncServiceConfig _config;
        private readonly ILogger<SyncService> _logger;

        public SyncService(SyncServiceConfig config , ILogger<SyncService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task ExecuteSyncSpAsync()
        {
            await using var connection = new SqlConnection(_config.ConnectionString);
            await using var command = new SqlCommand(_config.SpName, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 10000000;
            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if(connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

        public class SyncServiceConfig
        {
            public required string ConnectionString { get; init; }
            public required string SpName { get; init; }
        }
    }
}
