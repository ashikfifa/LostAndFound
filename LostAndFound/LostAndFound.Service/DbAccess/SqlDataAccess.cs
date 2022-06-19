using Dapper;
using LostAndFound.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace LostAndFound.Service.DbAccess
{
    public class SqlDataAccess : ISqlDataAccess
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;


        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }


        public async Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U paramiters)
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(_connectionString));

            return await connection.QueryAsync<T>(storedProcedure, paramiters, commandType: CommandType.StoredProcedure);
        }

        public async Task SaveData<T>(string storedProcedure, T paramiters)
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(_connectionString));

            await connection.ExecuteAsync(storedProcedure, paramiters, commandType: CommandType.StoredProcedure);
        }

        public async Task<T> SaveDataAndReturnId<T, U>(string storedProcedure, U paramiters)
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(_connectionString)); 

            return await connection.ExecuteScalarAsync<T>(storedProcedure, paramiters, commandType: CommandType.StoredProcedure);
        }
    }
}
