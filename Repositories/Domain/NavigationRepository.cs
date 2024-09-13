using Spider_QAMS.DAL;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Models;
using Spider_QAMS.Repositories.Skeleton;
using System.Data.SqlClient;
using System.Data;
using Spider_QAMS.Utilities;
using static Spider_QAMS.Utilities.Constants;

namespace Spider_QAMS.Repositories.Domain
{
    public class NavigationRepository : INavigationRepository
    {
        private object GetDbValue<T>(T newValue, T existingValue)
        {
            if (newValue == null || newValue.Equals(DBNull.Value))
            {
                return DBNull.Value;
            }
            // Handle string case
            if (typeof(T) == typeof(string))
            {
                if (string.IsNullOrEmpty(newValue as string) || (newValue as string) == (existingValue as string))
                {
                    return DBNull.Value;
                }
            }
            // Handle other types (int?, bool?, etc.)
            else if (newValue.Equals(existingValue))
            {
                return DBNull.Value;
            }
            return newValue;
        }
        public async Task<string> UpdateUserVerificationAsync(UserVerifyApiVM userVerifyApiVM, int CurrentUserId)
        {
            try
            {
                int NumberOfRowsAffected = 0;

                ProfileUserAPIVM profileUserExisting = new ProfileUserAPIVM();
                string commandText = $"SELECT u.IdNumber, u.FullName, u.Email, u.MobileNo, u.Username, u.Userimgpath, u.IsActive from AspNetUsers u WHERE U.Id = {CurrentUserId}";
                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

                if (dataTable.Rows.Count > 0)
                {
                    DataRow dataRow = dataTable.Rows[0];
                    profileUserExisting = new ProfileUserAPIVM
                    {
                        FullName = dataRow["FullName"] != DBNull.Value ? dataRow["FullName"].ToString() : string.Empty,
                        Designation = dataRow["Designation"] != DBNull.Value ? dataRow["Designation"].ToString() : string.Empty,
                        EmailID = dataRow["EmailID"] != DBNull.Value ? dataRow["EmailID"].ToString() : string.Empty,
                        PhoneNumber = dataRow["PhoneNumber"] != DBNull.Value ? dataRow["PhoneNumber"].ToString() : string.Empty,
                        UserName = dataRow["UserName"] != DBNull.Value ? dataRow["UserName"].ToString() : string.Empty,
                        ProfilePicName = dataRow["ProfilePicName"] != DBNull.Value ? dataRow["ProfilePicName"].ToString() : string.Empty,
                        IsActive = dataRow["IsActive"] != DBNull.Value && Convert.ToBoolean(dataRow["IsActive"]),
                    };
                }

                // User Profile Updation
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@NewDesignation", SqlDbType.VarChar, 200) { Value = GetDbValue(userVerifyApiVM.Designation, profileUserExisting.Designation) },
                    new SqlParameter("@NewFullName", SqlDbType.VarChar, 200) { Value = GetDbValue(userVerifyApiVM.FullName, profileUserExisting.FullName) },
                    new SqlParameter("@NewPhoneNumber", SqlDbType.VarChar, 15) { Value = GetDbValue(userVerifyApiVM.PhoneNumber, profileUserExisting.PhoneNumber) },
                    new SqlParameter("@NewUserName", SqlDbType.VarChar, 100) { Value = GetDbValue(userVerifyApiVM.UserName, profileUserExisting.UserName) },
                    new SqlParameter("@NewProfilePicName", SqlDbType.VarChar, 255) { Value = GetDbValue(userVerifyApiVM.ProfilePicName, profileUserExisting.ProfilePicName) },
                    new SqlParameter("@NewIsActive", SqlDbType.Bit) { Value = 1 }, // Makes User Active with User Verification Setup completion
                    new SqlParameter("@NewUpdateUserId", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@NewCreateUserId", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = (int)UserFlagsProperty.UserVerificationSetupEnabled },
                    new SqlParameter("@InputFlag", SqlDbType.Bit) { Value = true },
                    new SqlParameter("@BaseUserRoleName", SqlDbType.VarChar, 200) { Value = Constants.BaseUserRoleName }
                };

                // Execute the command
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQuery(Constants.SP_UpdateUserVerificationInitialSetup, CommandType.StoredProcedure, sqlParameters);
                if (tables.Count > 0)
                {
                    dataTable = tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        NumberOfRowsAffected = (int)dataRow["RowsAffected"];
                        if (NumberOfRowsAffected < 0)
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                return profileUserExisting.ProfilePicName;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Error while adding User Profile - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding User Profile.", ex);
            }
        }
    }
}
