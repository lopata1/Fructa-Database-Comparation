using System.Data.SqlClient;
using System.Net.Sockets;
using System.Data;

namespace Fructa_Database_Comparation
{
    internal class Database : ICloneable
    {
        public string address;
        public string userId;
        public string password;
        public string databaseName;
        public string code;
        private string _connectionString;

        public Database(string address, string database, string userId, string password, string code)
        {
            this.address = address;
            this.userId = userId;
            this.databaseName = database;
            this.password = password;

            _connectionString = $"Server={address};Database={databaseName};User Id={userId};Password={password};TrustServerCertificate=true;\r\n";
            this.code = code;
        }

        public List<Object> executeOneAttributeReadQuery(string query, int timeout = 30)
        {
            List<Object> rows = new List<Object>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                query = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED " + query;
                SqlCommand command = new SqlCommand(query, connection);
                command.Connection.Open();
                command.CommandTimeout = timeout;
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (Properties.Settings.Default.cancelScan) return rows;
                    rows.Add(reader[0]);
                }
            }
            return rows;
        }

        public DataTable executeReadQuery(string query, int timeout = 30)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                query = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED " + query;
                SqlCommand command = new SqlCommand(query, connection);
                command.Connection.Open();
                command.CommandTimeout = timeout;
                SqlDataReader reader = command.ExecuteReader();

                for (int i = 0; i < reader.FieldCount; ++i)
                {
                    dataTable.Columns.Add(i.ToString());
                }

                while (reader.Read())
                {
                    List<Object> row = new List<Object>(reader.FieldCount);
                    for (int i = 0; i < reader.FieldCount; ++i)
                    {
                        row.Add(reader[i]);
                    }
                    dataTable.Rows.Add(row.ToArray());
                }
            }
            return dataTable;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
