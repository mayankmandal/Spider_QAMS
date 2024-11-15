using System.Data;
using System.Data.SqlClient;

namespace Spider_QAMS.DAL
{
    public class SqlDBHelper
    {
        public static string CONNECTION_STRING = "";
        const Int32 CONNECTION_TIMEOUT = 3000000;

        // This function will be used to execute R(CRUD) operation of parameterless commands
        internal static DataTable ExecuteSelectCommand(string CommandName, CommandType cmdType)
        {
            DataTable table;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    cmd.CommandTimeout = CONNECTION_TIMEOUT;

                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            table = new DataTable();
                            da.Fill(table);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return table;
        }

        // This function will be used to execute R(CRUD) operation of parameterized commands
        internal static DataTable ExecuteParameterizedSelectCommand(string CommandName, CommandType cmdType, SqlParameter[] param)
        {
            DataTable table = new DataTable();

            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    cmd.Parameters.AddRange(param);
                    cmd.CommandTimeout = CONNECTION_TIMEOUT;

                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(table);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return table;
        }

        // This function will be used to execute CUD(CRUD) operation of parameterized commands
        internal static bool ExecuteNonQuery(string CommandName, CommandType cmdType, SqlParameter[] param)
        {
            int result = 0;

            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    cmd.Parameters.AddRange(param);
                    cmd.CommandTimeout = CONNECTION_TIMEOUT;

                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }

                        result = cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return (result > 0);
        }

        internal static DataSet ExecuteParameterizedSelectCommandDs(string CommandName, CommandType cmdType, SqlParameter[] param)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    cmd.Parameters.AddRange(param);
                    cmd.CommandTimeout = CONNECTION_TIMEOUT;

                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return ds;
        }

        internal static DataSet ExecuteSelectCommandDs(string CommandName, CommandType cmdType)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    cmd.CommandTimeout = CONNECTION_TIMEOUT;

                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return ds;
        }

        internal static List<DataTable> ExecuteParameterizedNonQuery(string CommandName, CommandType cmdType, SqlParameter[] param)
        {
            List<DataTable> tables = new List<DataTable>();

            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    cmd.Parameters.AddRange(param);
                    cmd.CommandTimeout = CONNECTION_TIMEOUT;

                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);

                            foreach (DataTable dt in ds.Tables)
                            {
                                tables.Add(dt);
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return tables;
        }

        public static bool ExecuteNonQueryWithTransaction(SqlTransaction transaction, string CommandName, CommandType cmdType, SqlParameter[] param)
        {
            int result = 0;

            using (SqlCommand cmd = transaction.Connection.CreateCommand())
            {
                cmd.Transaction = transaction;  // Assign the transaction
                cmd.CommandType = cmdType;
                cmd.CommandText = CommandName;
                cmd.Parameters.AddRange(param);

                try
                {
                    result = cmd.ExecuteNonQuery();
                }
                catch
                {
                    throw;
                }
            }

            return (result > 0);
        }

        internal static List<DataTable> ExecuteParameterizedNonQueryWithTransaction(SqlTransaction transaction,string CommandName,CommandType cmdType,SqlParameter[] param)
        {
            List<DataTable> tables = new List<DataTable>();
            using (SqlCommand cmd = transaction.Connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandType = cmdType;
                cmd.CommandText = CommandName;
                cmd.Parameters.AddRange(param);
                cmd.CommandTimeout = CONNECTION_TIMEOUT;
                try
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        foreach (DataTable dt in ds.Tables)
                        {
                            tables.Add(dt);
                        }
                    }
                }
                catch
                {
                    throw;
                }
                
            }
            return tables;
        }
    }
}
