using InsightFlow.Core.Configuration;
using Microsoft.Data.Sqlite;

namespace InsightFlow.Infrastructure.Persistence;

public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(settings.DatabasePath));
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = settings.DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        };

        _connectionString = builder.ConnectionString;
    }

    public SqliteConnection CreateConnection() => new(_connectionString);
}
