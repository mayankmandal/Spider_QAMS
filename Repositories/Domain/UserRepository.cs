using Spider_QAMS.DAL;
using Spider_QAMS.Models;
using Spider_QAMS.Repositories.Skeleton;
using System.Data.SqlClient;
using System.Data;
using Spider_QAMS.Utilities;
using static Spider_QAMS.Utilities.Constants;

namespace Spider_QAMS.Repositories.Domain
{
    public class UserRepository : IUserRepository
    {
        private SqlTransaction _transaction;
        public void SetTransaction(SqlTransaction transaction)
        {
            _transaction = transaction;
        }
        public async Task<ApplicationUser> GetUserByEmailAsyncRepo(string email)
        {
            ApplicationUser ActualUser = new ApplicationUser();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = UserDataRetrievalCriteria.GetUserByEmail },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = DBNull.Value },
                    new SqlParameter("@InputString", SqlDbType.NVarChar, 200) { Value = email ?? (object)DBNull.Value },
                    new SqlParameter("@InputFlag", SqlDbType.Bit) { Value = DBNull.Value },
                    new SqlParameter("@NewUpdateUserId", SqlDbType.Int) { Value = 0 }, // You can modify this based on your logic
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetUserData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        // Map the DataTable to the ApplicationUser object
                        List<ApplicationUser> users = DataTableHelper.MapDataTableToList<ApplicationUser>(dataTable);
                        ActualUser = users.FirstOrDefault();
                        if (ActualUser != null && ActualUser.UserId <= 0)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Getting User by Email.", ex);
            }
            return ActualUser;
        }
        public async Task<ApplicationUser> GetUserByIdAsyncRepo(int userId)
        {
            ApplicationUser ActualUser = new ApplicationUser();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = UserDataRetrievalCriteria.GetUserById },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value =  userId },
                    new SqlParameter("@InputString", SqlDbType.NVarChar, 200) { Value = (object)DBNull.Value },
                    new SqlParameter("@InputFlag", SqlDbType.Bit) { Value = DBNull.Value },
                    new SqlParameter("@NewUpdateUserId", SqlDbType.Int) { Value = 0 }, // You can modify this based on your logic
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetUserData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        List<ApplicationUser> users = DataTableHelper.MapDataTableToList<ApplicationUser>(dataTable);
                        ActualUser = users.FirstOrDefault();
                        if (ActualUser != null && ActualUser.UserId <= 0)
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Getting User by ID.", ex);
            }
            return ActualUser;
        }
        public async Task<IList<string>> GetUserRolesAsyncRepo(int userId)
        {
            List<string> roles = new List<string>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = UserDataRetrievalCriteria.GetUserRoles },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value =  userId },
                    new SqlParameter("@InputString", SqlDbType.NVarChar, 200) { Value = (object)DBNull.Value },
                    new SqlParameter("@InputFlag", SqlDbType.Bit) { Value = DBNull.Value },
                    new SqlParameter("@NewUpdateUserId", SqlDbType.Int) { Value = 0 }, // You can modify this based on your logic
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetUserData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            roles.Add(dataRow["ProfileName"].ToString());
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Getting User Roles.", ex);
            }
            return roles;
        }
    }
}
