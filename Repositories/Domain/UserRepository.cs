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
        public async Task<ApplicationUser> GetUserByEmailAsyncRepo(string email)
        {
            ApplicationUser ActualUser = new ApplicationUser();
            try
            {
                string commandText = "SELECT * FROM Users u WHERE u.EmailID = @Email";
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                new SqlParameter("@Email", SqlDbType.VarChar, 100) { Value = email }
                };

                DataTable dataTable = SqlDBHelper.ExecuteParameterizedSelectCommand(commandText, CommandType.Text, sqlParameters);
                if (dataTable.Rows.Count > 0)
                {
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
            try
            {
                string commandText = "SELECT * FROM AspNetUsers WHERE UserId = @UserId";
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
                };

                DataTable dataTable = SqlDBHelper.ExecuteParameterizedSelectCommand(commandText, CommandType.Text, sqlParameters);
                if (dataTable.Rows.Count > 0)
                {
                    DataRow dataRow = dataTable.Rows[0];
                    return new ApplicationUser
                    {
                        UserId = Convert.ToInt32(dataRow["UserId"]),
                        FullName = dataRow["FullName"].ToString(),
                        EmailID = dataRow["Email"].ToString(),
                        UserName = dataRow["Username"].ToString()
                        // Populate other fields as necessary
                    };
                }
                return null;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Getting User by ID.", ex);
            }
        }

        public async Task<IList<string>> GetUserRolesAsyncRepo(int userId)
        {
            try
            {
                string commandText = "SELECT DISTINCT p.ProfileName FROM Profiles p INNER JOIN UserProfile up ON p.ProfileID = up.ProfileID INNER JOIN Users u ON up.UserID = @UserId";
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
                };

                DataTable dataTable = SqlDBHelper.ExecuteParameterizedSelectCommand(commandText, CommandType.Text, sqlParameters);
                List<string> roles = new List<string>();

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    roles.Add(dataRow["ProfileName"].ToString());
                }
                return roles;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Getting User Roles.", ex);
            }
        }

        public async Task<ApplicationUser> RegisterUserAsyncRepo(ApplicationUser user)
        {
            ApplicationUser ActualUser = new ApplicationUser();
            try
            {
                // User Profile Creation
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@NewFullName", SqlDbType.VarChar, 200) { Value = user.FullName },
                    new SqlParameter("@NewEmail", SqlDbType.VarChar, 100) { Value = user.EmailID },
                    new SqlParameter("@NewPasswordSalt", SqlDbType.VarChar, 255) { Value = user.PasswordSalt },
                    new SqlParameter("@NewPasswordHash", SqlDbType.VarChar, 255) { Value = user.PasswordHash },
                };

                // Execute the command
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQuery(Constants.SP_RegisterNewUser, CommandType.StoredProcedure, sqlParameters);
                if (tables.Count > 0)
                {
                    DataTable dataTable = tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        // Map the DataTable to the ApplicationUser object
                        List<ApplicationUser> users = DataTableHelper.MapDataTableToList<ApplicationUser>(dataTable);
                        ActualUser = users.FirstOrDefault();
                    }
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
            catch (SqlException sqlEx)
            {
                throw new Exception("Error executing SQL command - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in User Registration.", ex);
            }
            return ActualUser;
        }
        public async Task<bool> ConfirmEmailAsyncRepo(int userId)
        {
            try
            {
                int isUnique = 0;

                // User Profile Creation
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = (int)UserFlagsProperty.EmailConfirmed },
                    new SqlParameter("@InputFlag", SqlDbType.Bit) { Value = true },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@NewUpdateUserId", SqlDbType.Int) { Value = userId },
                };

                // Execute the command
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_UpdateUserFlags, CommandType.StoredProcedure, sqlParameters);
                if (tables.Count > 0)
                {
                    DataTable dataTable = tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        isUnique = (int)dataRow["RowsAffected"];
                    }
                }
                return isUnique > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in confirming email.", ex);
            }
        }
    }
}
