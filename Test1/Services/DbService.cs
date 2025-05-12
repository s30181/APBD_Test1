using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public class DbService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<T?> FetchScalar<T>(string query, Dictionary<string, object>? parameters = null) where T : struct
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        await connection.OpenAsync();

        if (parameters != null)
        {
            PutParameters(command, parameters);
        }

        var result = await command.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value)
        {
            return null;
        }

        return (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<T?> FetchRow<T>(
        string query, 
        Func<SqlDataReader, Task<T>> function, 
        Dictionary<string, object>? parameters = null) where T : class
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        await connection.OpenAsync();

        if (parameters != null)
        {
            PutParameters(command, parameters);
        }

        var result = await command.ExecuteReaderAsync();
        if (!result.HasRows)
        {
            return null;
        }

        await result.ReadAsync();

        return await function(result);
    }

    private void PutParameters(SqlCommand command, Dictionary<string, object> parameters)
    {
        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
        }
    }

    public async Task<int> ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand(query, connection);

        if (parameters is not null)
        {
            PutParameters(command, parameters);
        }

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }
    
    public async Task<List<T>> FetchList<T>(string command, Func<SqlDataReader, Task<T>> function, Dictionary<string, object>? parameters = null)
    {
        var result = new List<T>();

        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(command, conn);
        await conn.OpenAsync();

        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(await function(reader));
        }

        return result;
    }
}