using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using VBFB_API.Models;

namespace VBFB_API.Classes
{
    /// <summary>
    /// Runs a parameterized SELECT and returns the rows as dictionaries (column name -> value), for
    /// joined/computed queries that don't map to one entity. Serialises to JSON with the SQL column names.
    /// </summary>
    public static class SqlQuery
    {
        public static List<Dictionary<string, object?>> Select(ApplicationDbContext db, string sql, IEnumerable<SqlParameter>? parameters = null)
        {
            List<Dictionary<string, object?>> rows = new();
            DbConnection conn = db.Database.GetDbConnection();
            bool opened = false;

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
                opened = true;
            }

            try
            {
                using DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                // Parameter objects can only belong to one command, so copy them
                if (parameters != null)
                {
                    foreach (SqlParameter p in parameters) cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value));
                }

                using DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object?> row = new(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        // Duplicate names (select p.*, c.*) keep the first one
                        string name = reader.GetName(i);
                        if (!row.ContainsKey(name)) row[name] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    rows.Add(row);
                }
            }
            finally
            {
                if (opened) conn.Close();
            }

            return rows;
        }

        public static int Count(ApplicationDbContext db, string sql, IEnumerable<SqlParameter>? parameters = null)
        {
            var rows = Select(db, sql, parameters);
            return rows.Count == 0 ? 0 : System.Convert.ToInt32(rows[0].Values.First());
        }
    }
}
