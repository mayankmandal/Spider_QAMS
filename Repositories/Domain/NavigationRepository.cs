using Spider_QAMS.DAL;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Models;
using Spider_QAMS.Repositories.Skeleton;
using System.Data.SqlClient;
using System.Data;
using static Spider_QAMS.Utilities.Constants;
using Spider_QAMS.Utilities;

namespace Spider_QAMS.Repositories.Domain
{
    public class NavigationRepository : INavigationRepository
    {
        private SqlTransaction _transaction;
        public void SetTransaction(SqlTransaction transaction)
        {
            _transaction = transaction;
        }
        private static string GetString(object value) => value == DBNull.Value ? string.Empty : value.ToString();
        private static int? GetNullableInt(object value) => value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        private static long? GetNullableLong(object value) => value == DBNull.Value ? (long?)null : Convert.ToInt64(value);
        private static bool GetBoolean(object value) => value != DBNull.Value && Convert.ToBoolean(value);
        private static DateTime? GetNullableDateTime(object value) => value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value);

        public async Task<object> FetchRecordByTypeAsync(Record record)
        {
            switch (record.RecordType)
            {
                case (int)FetchRecordByIdOrTextEnum.GetCurrentUserDetails:
                    return await GetUserRecordAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetCurrentUserProfile:
                    return await GetCurrentUserProfileAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetCurrentUserPages:
                    return await GetCurrentUserPagesAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetCurrentUserCategories:
                    return await GetCurrentUserCategoriesAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetSettingsData:
                    return await GetSettingsDataAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetProfileData:
                    return await GetProfileDataAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetCategoryData:
                    return await GetCategoryDataAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetRegionData:
                    return await GetRegionDataAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetCityData:
                    return await GetCityDataAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetLocationData:
                    return await GetLocationDataAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetContactData:
                    return await GetContactDataAsync(record.RecordId);
                case (int)FetchRecordByIdOrTextEnum.GetSiteDetail:
                    return await GetSiteDetailsDataAsync(record);
                case (int)FetchRecordByIdOrTextEnum.GetSitePictureBySiteId:
                    return await GetSitePicturesDataAsync(record);
                case (int)FetchRecordByIdOrTextEnum.GetSitePictureBySitePicID:
                    return await GetSitePictureDataAsync(record);
                default:
                    throw new Exception("Invalid Record Type");
            }
        }
        public async Task<bool> CheckUniquenessAsync(UniquenessCheckRequest uniquenessCheckRequest)
        {
            int isUnique = 0;
            try
            {
                TableNameCheckUniqueness? tableEnum = null;
                if (TableNameClassForUniqueness.User.Contains(uniquenessCheckRequest.Field1))
                {
                    tableEnum = TableNameCheckUniqueness.User;
                }
                else if (TableNameClassForUniqueness.Profile.Contains(uniquenessCheckRequest.Field1))
                {
                    tableEnum = TableNameCheckUniqueness.Profile;
                }
                else if (TableNameClassForUniqueness.PageCategory.Contains(uniquenessCheckRequest.Field1))
                {
                    tableEnum = TableNameCheckUniqueness.PageCategory;
                }
                else if (TableNameClassForUniqueness.Region.Contains(uniquenessCheckRequest.Field1))
                {
                    tableEnum = TableNameCheckUniqueness.Region;
                }
                else if (TableNameClassForUniqueness.City.Contains(uniquenessCheckRequest.Field1))
                {
                    tableEnum = TableNameCheckUniqueness.City;
                }
                else if (string.IsNullOrEmpty(uniquenessCheckRequest.Field2) && !string.IsNullOrEmpty(uniquenessCheckRequest.Field1) && TableNameClassForUniqueness.SiteDetail.Contains(uniquenessCheckRequest.Field1))
                {
                    tableEnum = TableNameCheckUniqueness.SiteDetailWith1Field;
                }
                else if (!string.IsNullOrEmpty(uniquenessCheckRequest.Field2) && !string.IsNullOrEmpty(uniquenessCheckRequest.Field1) && TableNameClassForUniqueness.SiteDetail.Contains(uniquenessCheckRequest.Field1) && TableNameClassForUniqueness.SiteDetail.Contains(uniquenessCheckRequest.Field2))
                {
                    tableEnum = TableNameCheckUniqueness.SiteDetailWith2Field;
                }
                if (tableEnum == null)
                {
                    throw new ArgumentException($"Field - {uniquenessCheckRequest.Field1} does not match any known column.");
                }
                // User Profile Creation
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TableId", SqlDbType.Int) { Value = (int)tableEnum },
                    new SqlParameter("@Field1", SqlDbType.VarChar, 50) { Value = uniquenessCheckRequest.Field1 },
                    new SqlParameter("@Field2", SqlDbType.VarChar, 50) { Value = uniquenessCheckRequest.Field2 },
                    new SqlParameter("@Value1", SqlDbType.VarChar, 100) { Value = uniquenessCheckRequest.Value1 },
                    new SqlParameter("@Value2", SqlDbType.VarChar, 100) { Value = uniquenessCheckRequest.Value2 }
                };

                // Execute the command
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_CheckUniqueness, CommandType.StoredProcedure, sqlParameters);
                if (tables.Count > 0)
                {
                    DataTable dataTable = tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        isUnique = (int)dataRow["IsUnique"];
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error while checking existing {uniquenessCheckRequest.Field1} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while checking existing {uniquenessCheckRequest.Field1}.", ex);
            }
            return isUnique > 0;
        }
        public async Task<bool> DeleteEntityAsync(int deleteId, string deleteType)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@Id", SqlDbType.Int){Value = deleteId},
                    new SqlParameter("@Type", SqlDbType.VarChar, 50){Value = deleteType},
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                bool isSuccess = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_DeleteEntityRecord, CommandType.StoredProcedure, sqlParameters);

                int RowsAffected = (sqlParameters[2].Value != DBNull.Value) ? (int)sqlParameters[2].Value : -1;

                if (RowsAffected < 0)
                {
                    return false;
                }
                return true;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error deleting record for {deleteType} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting record for {deleteType}.", ex);
            }
        }

        public async Task<bool> CreateUserProfileAsync(ProfileUser userProfileData, int CurrentUserId)
        {
            try
            {
                int UserId = 0;
                // User Profile Creation
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@NewDesignation", SqlDbType.VarChar, 100) { Value = userProfileData.Designation },
                    new SqlParameter("@NewFullName", SqlDbType.VarChar, 200) { Value = userProfileData.FullName },
                    new SqlParameter("@NewEmailAddress", SqlDbType.VarChar, 100) { Value = userProfileData.EmailID },
                    new SqlParameter("@NewPhoneNumber", SqlDbType.VarChar, 15) { Value = userProfileData.PhoneNumber },
                    new SqlParameter("@NewProfileId", SqlDbType.Int) { Value = userProfileData.ProfileSiteData.ProfileId },
                    new SqlParameter("@NewUserName", SqlDbType.VarChar, 100) { Value = userProfileData.UserName },
                    new SqlParameter("@NewProfilePicName", SqlDbType.VarChar, 255) { Value = userProfileData.ProfilePicName },
                    new SqlParameter("@NewPasswordHash", SqlDbType.VarChar, 255) { Value = userProfileData.PasswordHash },
                    new SqlParameter("@NewPasswordSalt", SqlDbType.VarChar, 255) { Value = userProfileData.PasswordSalt },
                    new SqlParameter("@NewIsActive", SqlDbType.Bit) { Value = userProfileData.IsActive ? 1 : 0 },
                    new SqlParameter("@NewIsADUser", SqlDbType.Bit) { Value = userProfileData.IsADUser ? 1 : 0  },
                    new SqlParameter("@NewIsDeleted", SqlDbType.Bit) { Value = userProfileData.IsDeleted ? 1 : 0  },
                    new SqlParameter("@NewCreateUserId", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@NewUpdateUserId", SqlDbType.Int) { Value = CurrentUserId }
                };

                // Execute the command
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_AddNewUser, CommandType.StoredProcedure, sqlParameters);
                if (tables.Count > 0)
                {
                    DataTable dataTable = tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        UserId = (int)dataRow["UserId"];
                    }
                    if (UserId <= 0)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Error while adding User Profile - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding User Profile.", ex);
            }
            return true;
        }
        public async Task<string> UpdateUserProfileAsync(ProfileUser userProfileData, int CurrentUserId)
        {
            ProfileUser profileUserExisting = null;
            try
            {
                int NumberOfRowsAffected = -1;

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetSettingsData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTab = dataTables[0];
                    if (dataTab.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTab.Rows[0];
                        profileUserExisting = new ProfileUser
                        {
                            UserId = userProfileData.UserId,
                            Designation = dataRow["Designation"] != DBNull.Value ? dataRow["Designation"].ToString() : string.Empty,
                            FullName = dataRow["FullName"] != DBNull.Value ? dataRow["FullName"].ToString() : string.Empty,
                            EmailID = dataRow["EmailID"] != DBNull.Value ? dataRow["EmailID"].ToString() : string.Empty,
                            PhoneNumber = dataRow["PhoneNumber"] != DBNull.Value ? dataRow["PhoneNumber"].ToString() : string.Empty,
                            PasswordHash = dataRow["PasswordHash"] != DBNull.Value ? dataRow["PasswordHash"].ToString() : string.Empty,
                            PasswordSalt = dataRow["PasswordSalt"] != DBNull.Value ? dataRow["PasswordSalt"].ToString() : string.Empty,
                            ProfileSiteData = new ProfileSite
                            {
                                ProfileId = dataRow["ProfileId"] != DBNull.Value ? Convert.ToInt32(dataRow["ProfileId"]) : 0,
                                ProfileName = string.Empty
                            },
                            UserName = dataRow["UserName"] != DBNull.Value ? dataRow["UserName"].ToString() : string.Empty,
                            ProfilePicName = dataRow["ProfilePicName"] != DBNull.Value ? dataRow["ProfilePicName"].ToString() : string.Empty,
                            IsActive = dataRow["IsActive"] != DBNull.Value ? Convert.ToBoolean(dataRow["IsActive"]) : false,
                            IsADUser = dataRow["IsADUser"] != DBNull.Value ? Convert.ToBoolean(dataRow["IsADUser"]) : false,
                            IsDeleted = dataRow["IsDeleted"] != DBNull.Value ? Convert.ToBoolean(dataRow["IsDeleted"]) : false,
                        };
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }

                // User Profile Updation
                sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userProfileData.UserId },
                    new SqlParameter("@NewDesignation", SqlDbType.VarChar, 10) { Value = userProfileData.Designation == profileUserExisting.Designation ? DBNull.Value : userProfileData.Designation },
                    new SqlParameter("@NewFullName", SqlDbType.VarChar, 200) { Value = userProfileData.FullName == profileUserExisting.FullName ? DBNull.Value : userProfileData.FullName },
                    new SqlParameter("@NewEmailID", SqlDbType.VarChar, 100) { Value = userProfileData.EmailID == profileUserExisting.EmailID ? DBNull.Value : userProfileData.EmailID },
                    new SqlParameter("@NewPhoneNumber", SqlDbType.VarChar, 15) { Value = userProfileData.PhoneNumber == profileUserExisting.PhoneNumber ? DBNull.Value : userProfileData.PhoneNumber },
                    new SqlParameter("@NewProfileId", SqlDbType.Int) { Value = userProfileData.ProfileSiteData.ProfileId == profileUserExisting.ProfileSiteData.ProfileId ? 0 : userProfileData.ProfileSiteData.ProfileId },
                    new SqlParameter("@NewUserName", SqlDbType.VarChar, 100) { Value = userProfileData.UserName == profileUserExisting.UserName ? DBNull.Value : userProfileData.UserName },
                    new SqlParameter("@NewProfilePicName", SqlDbType.VarChar, 255) { Value = userProfileData.ProfilePicName == profileUserExisting.ProfilePicName ? DBNull.Value : userProfileData.ProfilePicName },
                    new SqlParameter("@NewPasswordSalt", SqlDbType.VarChar, 255) { Value = userProfileData.PasswordSalt == profileUserExisting.PasswordSalt ? DBNull.Value : userProfileData.PasswordSalt },
                    new SqlParameter("@NewPasswordHash", SqlDbType.VarChar, 255) { Value = userProfileData.PasswordHash == profileUserExisting.PasswordHash ? DBNull.Value : userProfileData.PasswordHash },
                    new SqlParameter("@NewIsActive", SqlDbType.Bit) { Value = userProfileData.IsActive == profileUserExisting.IsActive ? profileUserExisting.IsActive : userProfileData.IsActive },
                    new SqlParameter("@NewIsADUser", SqlDbType.Bit) { Value = userProfileData.IsADUser == profileUserExisting.IsADUser ? profileUserExisting.IsADUser : userProfileData.IsADUser },
                    new SqlParameter("@NewUpdateUserId", SqlDbType.Int) { Value = CurrentUserId }
                };

                // Execute the command
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_UpdateUser, CommandType.StoredProcedure, sqlParameters);
                if (tables.Count > 0)
                {
                    DataTable dataTable = tables[0];
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
                else
                {
                    return string.Empty;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Error while adding User Profile - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding User Profile.", ex);
            }
            return profileUserExisting.ProfilePicName;
        }

        public async Task<string> UpdateSettingsDataAsync(ProfileUser userSettings, int CurrentUserId)
        {
            ProfileUser profileUserExisting = new ProfileUser();
            try
            {
                int NumberOfRowsAffected = -1;
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetSettingsData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTab = dataTables[0];
                    if (dataTab.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTab.Rows[0];
                        profileUserExisting = new ProfileUser
                        {
                            UserId = userSettings.UserId,
                            FullName = dataRow["FullName"] != DBNull.Value ? dataRow["FullName"].ToString() : string.Empty,
                            EmailID = dataRow["EmailID"] != DBNull.Value ? dataRow["EmailID"].ToString() : string.Empty,
                            UserName = dataRow["UserName"] != DBNull.Value ? dataRow["UserName"].ToString() : string.Empty,
                            ProfilePicName = dataRow["ProfilePicName"] != DBNull.Value ? dataRow["ProfilePicName"].ToString() : string.Empty,
                            PasswordHash = dataRow["PasswordHash"] != DBNull.Value ? dataRow["PasswordHash"].ToString() : string.Empty,
                            PasswordSalt = dataRow["PasswordSalt"] != DBNull.Value ? dataRow["PasswordSalt"].ToString() : string.Empty,
                            UpdateUserId = Convert.ToInt32(dataRow["UpdateUserId"]),
                        };
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }

                // User Profile Updation
                sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userSettings.UserId },
                    new SqlParameter("@NewUserName", SqlDbType.VarChar, 100) { Value = userSettings.UserName == profileUserExisting.UserName? DBNull.Value : userSettings.UserName },
                    new SqlParameter("@NewFullName", SqlDbType.VarChar, 200) { Value = userSettings.FullName == profileUserExisting.FullName ? DBNull.Value : userSettings.FullName },
                    new SqlParameter("@NewEmailID", SqlDbType.VarChar, 100) { Value = userSettings.EmailID == profileUserExisting.EmailID ? DBNull.Value : userSettings.EmailID },
                    new SqlParameter("@NewProfilePicName", SqlDbType.VarChar, 255) { Value = userSettings.ProfilePicName == profileUserExisting.ProfilePicName ? DBNull.Value : userSettings.ProfilePicName },
                    new SqlParameter("@NewPasswordSalt", SqlDbType.VarChar, 255) { Value = userSettings.PasswordSalt == profileUserExisting.PasswordSalt ? DBNull.Value : userSettings.PasswordSalt },
                    new SqlParameter("@NewPasswordHash", SqlDbType.VarChar, 255) { Value = userSettings.PasswordHash == profileUserExisting.PasswordHash ? DBNull.Value : userSettings.PasswordHash },
                    new SqlParameter("@NewUpdateUserId", SqlDbType.Int) { Value = CurrentUserId }
                };

                // Execute the command
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_UpdateUserSettings, CommandType.StoredProcedure, sqlParameters);
                if (tables.Count > 0)
                {
                    DataTable dataTable = tables[0];
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
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Error while updating Settings - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating Settings.", ex);
            }
            return profileUserExisting.ProfilePicName;
        }

        public async Task<bool> UpdateRegionAsync(Region region)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@NewIntInput1", SqlDbType.Int){Value = region.RegionId},
                    new SqlParameter("@NewInput1", SqlDbType.VarChar, 200){Value = region.RegionName},
                    new SqlParameter("@Type", SqlDbType.VarChar, 10){Value = DeleteEntityType.Region},
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_UpdateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                int RowsAffected = (sqlParameters[3].Value != DBNull.Value) ? (int)sqlParameters[3].Value : -1;

                if (RowsAffected <= 0)
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error updating record for {region.RegionName} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating record for {region.RegionName}.", ex);
            }
            return true;
        }
        public async Task<bool> CreateRegionAsync(Region region)
        {
            int newId = -1;
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@NewInput1", SqlDbType.VarChar, 200){Value = region.RegionName},
                    new SqlParameter("@Type", SqlDbType.VarChar, 10){Value = DeleteEntityType.Region},
                    new SqlParameter("@NewID", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_CreateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                newId = (sqlParameters[2].Value != DBNull.Value) ? (int)sqlParameters[2].Value : -1;
                // Validate if the newId is greater than 0
                if (newId <= 0)
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error creating record for {region.RegionName} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating record for {region.RegionName}.", ex);
            }
            return true;
        }

        public async Task<bool> UpdateCityAsync(City city)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@NewIntInput1", SqlDbType.Int){Value = city.CityId},
                    new SqlParameter("@NewInput1", SqlDbType.VarChar, 200){Value = city.CityName},
                    new SqlParameter("@NewIntInput2", SqlDbType.Int){Value = city.RegionData.RegionId},
                    new SqlParameter("@Type", SqlDbType.VarChar, 10){Value = DeleteEntityType.City},
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_UpdateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                int RowsAffected = (sqlParameters[4].Value != DBNull.Value) ? (int)sqlParameters[4].Value : -1;

                if (RowsAffected <= 0)
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error updating record for {city.CityName} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating record for {city.CityName}.", ex);
            }
            return true;
        }
        public async Task<bool> CreateCityAsync(City city)
        {
            int newId = -1;
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@NewInput1", SqlDbType.VarChar, 200){Value = city.CityName},
                    new SqlParameter("@NewIntInput1", SqlDbType.Int){Value = city.RegionData.RegionId},
                    new SqlParameter("@Type", SqlDbType.VarChar, 10){Value = DeleteEntityType.City},
                    new SqlParameter("@NewID", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_CreateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                newId = (sqlParameters[3].Value != DBNull.Value) ? (int)sqlParameters[3].Value : -1;
                // Validate if the newId is greater than 0
                if (newId <= 0)
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error creating record for {city.CityName} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating record for {city.CityName}.", ex);
            }
            return true;
        }

        public async Task<bool> UpdateContactAsync(Contact contact)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@NewIntInput1", SqlDbType.Int){Value = contact.ContactId},
                    new SqlParameter("@NewInput1", SqlDbType.VarChar, 200){Value = contact.Name},
                    new SqlParameter("@NewInput2", SqlDbType.VarChar, 200){Value = contact.Designation},
                    new SqlParameter("@NewInput3", SqlDbType.VarChar, 200){Value = contact.OfficePhone},
                    new SqlParameter("@NewInput4", SqlDbType.VarChar, 200){Value = contact.Mobile},
                    new SqlParameter("@NewInput5", SqlDbType.VarChar, 200){Value = contact.EmailID},
                    new SqlParameter("@NewInput6", SqlDbType.VarChar, 200){Value = contact.Fax},
                    new SqlParameter("@NewInput7", SqlDbType.VarChar, 200){Value = contact.BranchName},
                    new SqlParameter("@NewIntInput2", SqlDbType.Int){Value = contact.sponsor.SponsorId},
                    new SqlParameter("@Type", SqlDbType.VarChar, 10){Value = DeleteEntityType.Contact},
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_UpdateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                int RowsAffected = (sqlParameters[10].Value != DBNull.Value) ? (int)sqlParameters[10].Value : -1;

                if (RowsAffected <= 0)
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error updating record for {contact.Name} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating record for {contact.Name}.", ex);
            }
            return true;
        }
        public async Task<bool> CreateContactAsync(Contact contact)
        {
            int newId = -1;
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@NewInput1", SqlDbType.VarChar, 200){Value = contact.Name},
                    new SqlParameter("@NewInput2", SqlDbType.VarChar, 200){Value = contact.Designation},
                    new SqlParameter("@NewInput3", SqlDbType.VarChar, 200){Value = contact.OfficePhone},
                    new SqlParameter("@NewInput4", SqlDbType.VarChar, 200){Value = contact.Mobile},
                    new SqlParameter("@NewInput5", SqlDbType.VarChar, 200){Value = contact.EmailID},
                    new SqlParameter("@NewInput6", SqlDbType.VarChar, 200){Value = contact.Fax},
                    new SqlParameter("@NewInput7", SqlDbType.VarChar, 200){Value = contact.BranchName},
                    new SqlParameter("@NewIntInput1", SqlDbType.Int){Value = contact.sponsor.SponsorId},
                    new SqlParameter("@Type", SqlDbType.VarChar, 10){Value = DeleteEntityType.Contact},
                    new SqlParameter("@NewID", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_CreateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                newId = (sqlParameters[9].Value != DBNull.Value) ? (int)sqlParameters[9].Value : -1;
                // Validate if the newId is greater than 0
                if (newId <= 0)
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error creating record for {contact.Name} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating record for {contact.Name}.", ex);
            }
            return true;
        }

        public async Task<bool> UpdateLocationAsync(SiteLocation siteLocation)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@NewIntInput1", SqlDbType.Int){Value = siteLocation.LocationId},
                    new SqlParameter("@NewInput1", SqlDbType.VarChar, 200){Value = siteLocation.Location},
                    new SqlParameter("@NewInput2", SqlDbType.VarChar, 200){Value = siteLocation.StreetName},
                    new SqlParameter("@NewIntInput2", SqlDbType.Int){Value = siteLocation.City.CityId},
                    new SqlParameter("@NewIntInput3", SqlDbType.Int){Value = siteLocation.City.RegionData.RegionId},
                    new SqlParameter("@NewInput3", SqlDbType.VarChar, 200){Value = siteLocation.DistrictName},
                    new SqlParameter("@NewInput4", SqlDbType.VarChar, 200){Value = siteLocation.BranchName},
                    new SqlParameter("@NewIntInput4", SqlDbType.Int){Value = siteLocation.Sponsor.SponsorId},
                    new SqlParameter("@Type", SqlDbType.VarChar, 10){Value = DeleteEntityType.Location},
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_UpdateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                int RowsAffected = (sqlParameters[9].Value != DBNull.Value) ? (int)sqlParameters[9].Value : -1;

                if (RowsAffected <= 0)
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error updating record for {siteLocation.Location} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating record for {siteLocation.Location}.", ex);
            }
            return true;
        }
        public async Task<bool> CreateLocationAsync(SiteLocation siteLocation)
        {
            int newId = -1;
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@NewInput1", SqlDbType.VarChar, 200){Value = siteLocation.Location},
                    new SqlParameter("@NewInput2", SqlDbType.VarChar, 200){Value = siteLocation.StreetName},
                    new SqlParameter("@NewIntInput1", SqlDbType.Int){Value = siteLocation.City.CityId},
                    new SqlParameter("@NewIntInput2", SqlDbType.Int){Value = siteLocation.City.RegionData.RegionId},
                    new SqlParameter("@NewInput3", SqlDbType.VarChar, 200){Value = siteLocation.DistrictName},
                    new SqlParameter("@NewInput4", SqlDbType.VarChar, 200){Value = siteLocation.BranchName},
                    new SqlParameter("@NewIntInput3", SqlDbType.Int){Value = siteLocation.Sponsor.SponsorId},
                    new SqlParameter("@Type", SqlDbType.VarChar, 10){Value = DeleteEntityType.Location},
                    new SqlParameter("@NewID", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_CreateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                newId = (sqlParameters[3].Value != DBNull.Value) ? (int)sqlParameters[3].Value : -1;
                // Validate if the newId is greater than 0
                if (newId <= 0)
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error creating record for {siteLocation.Location} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating record for {siteLocation.Location}.", ex);
            }
            return true;
        }

        public async Task<SiteDetail> CreateSiteDetailsAsync(SiteDetail siteDetail)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    // SiteDetails table parameters
                    new SqlParameter("@SiteCode", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.SiteCode) ? (object)DBNull.Value : siteDetail.SiteCode },
                    new SqlParameter("@SiteName", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.SiteName) ? (object)DBNull.Value : siteDetail.SiteName },
                    new SqlParameter("@SiteCategory", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.SiteCategory) ? (object)DBNull.Value : siteDetail.SiteCategory },
                    new SqlParameter("@SponsorID", SqlDbType.Int) { Value = siteDetail.SponsorID == 0 ? (object)DBNull.Value : siteDetail.SponsorID },
                    new SqlParameter("@RegionID", SqlDbType.Int) { Value = siteDetail.RegionID == 0 ? (object)DBNull.Value : siteDetail.RegionID },
                    new SqlParameter("@CityID", SqlDbType.Int) { Value = siteDetail.CityID == 0 ? (object)DBNull.Value : siteDetail.CityID },
                    new SqlParameter("@LocationID", SqlDbType.Int) { Value = siteDetail.LocationID == 0 ? (object)DBNull.Value : siteDetail.LocationID },
                    new SqlParameter("@ContactID", SqlDbType.Int) { Value = siteDetail.ContactID == 0 ? (object)DBNull.Value : siteDetail.ContactID },
                    new SqlParameter("@SiteTypeID", SqlDbType.Int) { Value = siteDetail.SiteTypeID == 0 ? (object)DBNull.Value : siteDetail.SiteTypeID },
                    new SqlParameter("@GPSLong", SqlDbType.VarChar, 30) { Value = string.IsNullOrEmpty(siteDetail.GPSLong) ? (object)DBNull.Value : siteDetail.GPSLong },
                    new SqlParameter("@GPSLatt", SqlDbType.VarChar, 30) { Value = string.IsNullOrEmpty(siteDetail.GPSLatt) ? (object)DBNull.Value : siteDetail.GPSLatt },
                    new SqlParameter("@VisitUserID", SqlDbType.Int) { Value = siteDetail.VisitUserID == 0 ? (object)DBNull.Value : siteDetail.VisitUserID },
                    new SqlParameter("@VisitedDate", SqlDbType.DateTime) { Value = siteDetail.VisitedDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.VisitedDate },
                    new SqlParameter("@ApprovedUserID", SqlDbType.Int) { Value = siteDetail.ApprovedUserID == 0 ? (object)DBNull.Value : siteDetail.ApprovedUserID },
                    new SqlParameter("@ApprovalDate", SqlDbType.DateTime) { Value = siteDetail.ApprovalDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.ApprovalDate },
                    new SqlParameter("@VisitStatusID", SqlDbType.Int) { Value = siteDetail.VisitStatusID == 0 ? (object)DBNull.Value : siteDetail.VisitStatusID },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = siteDetail.IsActive? 1 : 0 },
                    new SqlParameter("@BranchNo", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.BranchNo) ? (object)DBNull.Value : siteDetail.BranchNo },
                    new SqlParameter("@BranchTypeId", SqlDbType.Int) { Value = siteDetail.BranchTypeId == 0 ? (object)DBNull.Value : siteDetail.BranchTypeId },
                    new SqlParameter("@AtmClass", SqlDbType.Char, 1) { Value = string.IsNullOrEmpty(siteDetail.AtmClass) ? (object)DBNull.Value : siteDetail.AtmClass },

                    // GeographicalDetails table parameters
                    new SqlParameter("@NearestLandmark", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.NearestLandmark) ? (object)DBNull.Value : siteDetail.GeographicalDetails.NearestLandmark },
                    new SqlParameter("@NumberofKmNearestCity", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.NumberOfKmNearestCity) ? (object)DBNull.Value : siteDetail.GeographicalDetails.NumberOfKmNearestCity },
                    new SqlParameter("@BranchConstructionType", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.BranchConstructionType) ? (object)DBNull.Value : siteDetail.GeographicalDetails.BranchConstructionType },
                    new SqlParameter("@BranchIsLocatedAt", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.BranchIsLocatedAt) ? (object)DBNull.Value : siteDetail.GeographicalDetails.BranchIsLocatedAt },
                    new SqlParameter("@HowToReachThere", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.HowToReachThere) ? (object)DBNull.Value : siteDetail.GeographicalDetails.HowToReachThere },
                    new SqlParameter("@SiteisonServiceRoad", SqlDbType.Bit) { Value = siteDetail.GeographicalDetails.SiteIsOnServiceRoad? 1 : 0 },
                    new SqlParameter("@Howtogetthere", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.HowToGetThere) ? (object)DBNull.Value : siteDetail.GeographicalDetails.HowToGetThere },

                    // SiteBranchFacilities table parameters
                    new SqlParameter("@Parking", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.Parking? 1 : 0 },
                    new SqlParameter("@Landscape", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.Landscape? 1 : 0 },
                    new SqlParameter("@Elevator", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.Elevator? 1 : 0 },
                    new SqlParameter("@VIPSection", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.VIPSection? 1 : 0 },
                    new SqlParameter("@SafeBox", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.SafeBox? 1 : 0 },
                    new SqlParameter("@ICAP", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.ICAP? 1 : 0 },

                    // SiteContactInformation table parameters
                    new SqlParameter("@BranchTelephoneNumber", SqlDbType.VarChar, 20) { Value = string.IsNullOrEmpty(siteDetail.ContactInformation.BranchTelephoneNumber) ? (object)DBNull.Value : siteDetail.ContactInformation.BranchTelephoneNumber },
                    new SqlParameter("@BranchFaxNumber", SqlDbType.VarChar, 20) { Value = string.IsNullOrEmpty(siteDetail.ContactInformation.BranchFaxNumber) ? (object)DBNull.Value : siteDetail.ContactInformation.BranchFaxNumber },
                    new SqlParameter("@EmailAddress", SqlDbType.Text) { Value = string.IsNullOrEmpty(siteDetail.ContactInformation.EmailAddress) ? (object)DBNull.Value : siteDetail.ContactInformation.EmailAddress },

                    // SiteDataCenter table parameters
                    new SqlParameter("@UPSBrand", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.UPSBrand) ? (object)DBNull.Value : siteDetail.DataCenter.UPSBrand },
                    new SqlParameter("@UPSCapacity", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.UPSCapacity) ? (object)DBNull.Value : siteDetail.DataCenter.UPSCapacity },
                    new SqlParameter("@PABXBrand", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.PABXBrand) ? (object)DBNull.Value : siteDetail.DataCenter.PABXBrand },
                    new SqlParameter("@StabilizerBrand", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.StabilizerBrand) ? (object)DBNull.Value : siteDetail.DataCenter.StabilizerBrand },
                    new SqlParameter("@StabilizerCapacity", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.StabilizerCapacity) ? (object)DBNull.Value : siteDetail.DataCenter.StabilizerCapacity },
                    new SqlParameter("@SecurityAccessSystemBrand", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.SecurityAccessSystemBrand) ? (object)DBNull.Value : siteDetail.DataCenter.SecurityAccessSystemBrand },

                    // SignBoardType table parameters
                    new SqlParameter("@Cylinder", SqlDbType.Bit) { Value = siteDetail.SignBoard.Cylinder? 1 : 0 },
                    new SqlParameter("@StraightOrTotem", SqlDbType.Bit) { Value = siteDetail.SignBoard.StraightOrTotem? 1 : 0 },

                    // SiteMiscInformation table parameters
                    new SqlParameter("@TypeofATMLocation", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.MiscSiteInfo.TypeOfATMLocation) ? (object)DBNull.Value : siteDetail.MiscSiteInfo.TypeOfATMLocation },
                    new SqlParameter("@NoofExternalCameras", SqlDbType.Int) { Value = siteDetail.MiscSiteInfo.NoOfExternalCameras == 0 ? (object)DBNull.Value : siteDetail.MiscSiteInfo.NoOfExternalCameras },
                    new SqlParameter("@NoofInternalCameras", SqlDbType.Int) { Value = siteDetail.MiscSiteInfo.NoOfInternalCameras == 0 ? (object)DBNull.Value : siteDetail.MiscSiteInfo.NoOfInternalCameras },
                    new SqlParameter("@TrackingSystem", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.MiscSiteInfo.TrackingSystem) ? (object)DBNull.Value : siteDetail.MiscSiteInfo.TrackingSystem },

                    // BranchMiscInformation table parameters
                    new SqlParameter("@Noofcleaners", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfCleaners == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfCleaners },
                    new SqlParameter("@Frequencyofdailymailingservice", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.FrequencyOfDailyMailingService == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.FrequencyOfDailyMailingService },
                    new SqlParameter("@ElectricSupply", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.MiscBranchInfo.ElectricSupply) ? (object)DBNull.Value : siteDetail.MiscBranchInfo.ElectricSupply },
                    new SqlParameter("@WaterSupply", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.MiscBranchInfo.WaterSupply) ? (object)DBNull.Value : siteDetail.MiscBranchInfo.WaterSupply },
                    new SqlParameter("@BranchOpenDate", SqlDbType.Date) { Value = siteDetail.MiscBranchInfo.BranchOpenDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.MiscBranchInfo.BranchOpenDate },
                    new SqlParameter("@TellersCounter", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.TellersCounter == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.TellersCounter },
                    new SqlParameter("@NoofSalesmanageroffices", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfSalesManagerOffices == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfSalesManagerOffices },
                    new SqlParameter("@ExistVIPsection", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.ExistVIPSection? 1 : 0 },
                    new SqlParameter("@ContractStartDate", SqlDbType.Date) { Value = siteDetail.MiscBranchInfo.ContractStartDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.MiscBranchInfo.ContractStartDate },
                    new SqlParameter("@NoofRenovationRetouchtime", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfRenovationRetouchTime == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfRenovationRetouchTime },
                    new SqlParameter("@LeasedOwbuilding", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.LeasedOwBuilding? 1 : 0 },
                    new SqlParameter("@Noofteaboys", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfTeaBoys == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfTeaBoys },
                    new SqlParameter("@Frequencyofmonthlycleaningservice", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.FrequencyOfMonthlyCleaningService == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.FrequencyOfMonthlyCleaningService },
                    new SqlParameter("@DrainSewerage", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.MiscBranchInfo.DrainSewerage) ? (object)DBNull.Value : siteDetail.MiscBranchInfo.DrainSewerage },
                    new SqlParameter("@CentralAC", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.CentralAC? 1 : 0 },
                    new SqlParameter("@SplitAC", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.SplitAC? 1 : 0 },
                    new SqlParameter("@WindowAC", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.WindowAC? 1 : 0 },
                    new SqlParameter("@Cashcountertype", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.CashCounterType == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.CashCounterType },
                    new SqlParameter("@NoofTellerCounters", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfTellerCounters == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfTellerCounters },
                    new SqlParameter("@Noofaffluentrelationshipmanageroffices", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfAffluentRelationshipManagerOffices == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfAffluentRelationshipManagerOffices },
                    new SqlParameter("@SeperateVIPsection", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.SeperateVIPSection? 1 : 0 },
                    new SqlParameter("@ContractEndDate", SqlDbType.Date) { Value = siteDetail.MiscBranchInfo.ContractEndDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.MiscBranchInfo.ContractEndDate },
                    new SqlParameter("@RenovationRetouchDate", SqlDbType.Date) { Value = siteDetail.MiscBranchInfo.RenovationRetouchDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.MiscBranchInfo.RenovationRetouchDate },
                    new SqlParameter("@NoofTCRmachines", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfTCRMachines == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfTCRMachines },
                    new SqlParameter("@NoOfTotem", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfTotem == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfTotem },
                    new SqlParameter("@NewSiteID", SqlDbType.BigInt) { Direction = ParameterDirection.Output }
                };

                bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_CreateSiteDetails, CommandType.StoredProcedure, sqlParameters);

                siteDetail.SiteID = (sqlParameters[73].Value != DBNull.Value) ? (long)sqlParameters[73].Value : -1;

                // Validate if the SiteID is greater than 0
                if (siteDetail.SiteID <= 0)
                {
                    return null;
                }
                siteDetail.SitePicturesLst = new List<SitePictures>(); ;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error creating record for {siteDetail.SiteName} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating record for {siteDetail.SiteName}.", ex);
            }
            return siteDetail;
        }
        public async Task<bool> UpdateSiteDetailsAsync(SiteDetail siteDetail)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    // SiteDetails table parameters
                    new SqlParameter("@SiteID",SqlDbType.BigInt){Value = siteDetail.SiteID == 0? (object)DBNull.Value: siteDetail.SiteID },
                    new SqlParameter("@SiteCode", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.SiteCode) ? (object)DBNull.Value : siteDetail.SiteCode },
                    new SqlParameter("@SiteName", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.SiteName) ? (object)DBNull.Value : siteDetail.SiteName },
                    new SqlParameter("@SiteCategory", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.SiteCategory) ? (object)DBNull.Value : siteDetail.SiteCategory },
                    new SqlParameter("@SponsorID", SqlDbType.Int) { Value = siteDetail.SponsorID == 0 ? (object)DBNull.Value : siteDetail.SponsorID },
                    new SqlParameter("@RegionID", SqlDbType.Int) { Value = siteDetail.RegionID == 0 ? (object)DBNull.Value : siteDetail.RegionID },
                    new SqlParameter("@CityID", SqlDbType.Int) { Value = siteDetail.CityID == 0 ? (object)DBNull.Value : siteDetail.CityID },
                    new SqlParameter("@LocationID", SqlDbType.Int) { Value = siteDetail.LocationID == 0 ? (object)DBNull.Value : siteDetail.LocationID },
                    new SqlParameter("@ContactID", SqlDbType.Int) { Value = siteDetail.ContactID == 0 ? (object)DBNull.Value : siteDetail.ContactID },
                    new SqlParameter("@SiteTypeID", SqlDbType.Int) { Value = siteDetail.SiteTypeID == 0 ? (object)DBNull.Value : siteDetail.SiteTypeID },
                    new SqlParameter("@GPSLong", SqlDbType.VarChar, 30) { Value = string.IsNullOrEmpty(siteDetail.GPSLong) ? (object)DBNull.Value : siteDetail.GPSLong },
                    new SqlParameter("@GPSLatt", SqlDbType.VarChar, 30) { Value = string.IsNullOrEmpty(siteDetail.GPSLatt) ? (object)DBNull.Value : siteDetail.GPSLatt },
                    new SqlParameter("@VisitUserID", SqlDbType.Int) { Value = siteDetail.VisitUserID == 0 ? (object)DBNull.Value : siteDetail.VisitUserID },
                    new SqlParameter("@VisitedDate", SqlDbType.DateTime) { Value = siteDetail.VisitedDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.VisitedDate },
                    new SqlParameter("@ApprovedUserID", SqlDbType.Int) { Value = siteDetail.ApprovedUserID == 0 ? (object)DBNull.Value : siteDetail.ApprovedUserID },
                    new SqlParameter("@ApprovalDate", SqlDbType.DateTime) { Value = siteDetail.ApprovalDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.ApprovalDate },
                    new SqlParameter("@VisitStatusID", SqlDbType.Int) { Value = siteDetail.VisitStatusID == 0 ? (object)DBNull.Value : siteDetail.VisitStatusID },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = siteDetail.IsActive? 1 : 0 },
                    new SqlParameter("@BranchNo", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.BranchNo) ? (object)DBNull.Value : siteDetail.BranchNo },
                    new SqlParameter("@BranchTypeId", SqlDbType.Int) { Value = siteDetail.BranchTypeId == 0 ? (object)DBNull.Value : siteDetail.BranchTypeId },
                    new SqlParameter("@AtmClass", SqlDbType.Char, 1) { Value = string.IsNullOrEmpty(siteDetail.AtmClass) ? (object)DBNull.Value : siteDetail.AtmClass },

                    // GeographicalDetails table parameters
                    new SqlParameter("@NearestLandmark", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.NearestLandmark) ? (object)DBNull.Value : siteDetail.GeographicalDetails.NearestLandmark },
                    new SqlParameter("@NumberofKmNearestCity", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.NumberOfKmNearestCity) ? (object)DBNull.Value : siteDetail.GeographicalDetails.NumberOfKmNearestCity },
                    new SqlParameter("@BranchConstructionType", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.BranchConstructionType) ? (object)DBNull.Value : siteDetail.GeographicalDetails.BranchConstructionType },
                    new SqlParameter("@BranchIsLocatedAt", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.BranchIsLocatedAt) ? (object)DBNull.Value : siteDetail.GeographicalDetails.BranchIsLocatedAt },
                    new SqlParameter("@HowToReachThere", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.HowToReachThere) ? (object)DBNull.Value : siteDetail.GeographicalDetails.HowToReachThere },
                    new SqlParameter("@SiteisonServiceRoad", SqlDbType.Bit) { Value = siteDetail.GeographicalDetails.SiteIsOnServiceRoad? 1 : 0 },
                    new SqlParameter("@Howtogetthere", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.GeographicalDetails.HowToGetThere) ? (object)DBNull.Value : siteDetail.GeographicalDetails.HowToGetThere },

                    // SiteBranchFacilities table parameters
                    new SqlParameter("@Parking", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.Parking? 1 : 0 },
                    new SqlParameter("@Landscape", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.Landscape? 1 : 0 },
                    new SqlParameter("@Elevator", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.Elevator? 1 : 0 },
                    new SqlParameter("@VIPSection", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.VIPSection? 1 : 0 },
                    new SqlParameter("@SafeBox", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.SafeBox? 1 : 0 },
                    new SqlParameter("@ICAP", SqlDbType.Bit) { Value = siteDetail.BranchFacilities.ICAP? 1 : 0 },

                    // SiteContactInformation table parameters
                    new SqlParameter("@BranchTelephoneNumber", SqlDbType.VarChar, 20) { Value = string.IsNullOrEmpty(siteDetail.ContactInformation.BranchTelephoneNumber) ? (object)DBNull.Value : siteDetail.ContactInformation.BranchTelephoneNumber },
                    new SqlParameter("@BranchFaxNumber", SqlDbType.VarChar, 20) { Value = string.IsNullOrEmpty(siteDetail.ContactInformation.BranchFaxNumber) ? (object)DBNull.Value : siteDetail.ContactInformation.BranchFaxNumber },
                    new SqlParameter("@EmailAddress", SqlDbType.Text) { Value = string.IsNullOrEmpty(siteDetail.ContactInformation.EmailAddress) ? (object)DBNull.Value : siteDetail.ContactInformation.EmailAddress },

                    // SiteDataCenter table parameters
                    new SqlParameter("@UPSBrand", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.UPSBrand) ? (object)DBNull.Value : siteDetail.DataCenter.UPSBrand },
                    new SqlParameter("@UPSCapacity", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.UPSCapacity) ? (object)DBNull.Value : siteDetail.DataCenter.UPSCapacity },
                    new SqlParameter("@PABXBrand", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.PABXBrand) ? (object)DBNull.Value : siteDetail.DataCenter.PABXBrand },
                    new SqlParameter("@StabilizerBrand", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.StabilizerBrand) ? (object)DBNull.Value : siteDetail.DataCenter.StabilizerBrand },
                    new SqlParameter("@StabilizerCapacity", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.StabilizerCapacity) ? (object)DBNull.Value : siteDetail.DataCenter.StabilizerCapacity },
                    new SqlParameter("@SecurityAccessSystemBrand", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.DataCenter.SecurityAccessSystemBrand) ? (object)DBNull.Value : siteDetail.DataCenter.SecurityAccessSystemBrand },

                    // SignBoardType table parameters
                    new SqlParameter("@Cylinder", SqlDbType.Bit) { Value = siteDetail.SignBoard.Cylinder? 1 : 0 },
                    new SqlParameter("@StraightOrTotem", SqlDbType.Bit) { Value = siteDetail.SignBoard.StraightOrTotem? 1 : 0 },

                    // SiteMiscInformation table parameters
                    new SqlParameter("@TypeofATMLocation", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.MiscSiteInfo.TypeOfATMLocation) ? (object)DBNull.Value : siteDetail.MiscSiteInfo.TypeOfATMLocation },
                    new SqlParameter("@NoofExternalCameras", SqlDbType.Int) { Value = siteDetail.MiscSiteInfo.NoOfExternalCameras == 0 ? (object)DBNull.Value : siteDetail.MiscSiteInfo.NoOfExternalCameras },
                    new SqlParameter("@NoofInternalCameras", SqlDbType.Int) { Value = siteDetail.MiscSiteInfo.NoOfInternalCameras == 0 ? (object)DBNull.Value : siteDetail.MiscSiteInfo.NoOfInternalCameras },
                    new SqlParameter("@TrackingSystem", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.MiscSiteInfo.TrackingSystem) ? (object)DBNull.Value : siteDetail.MiscSiteInfo.TrackingSystem },

                    // BranchMiscInformation table parameters
                    new SqlParameter("@Noofcleaners", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfCleaners == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfCleaners },
                    new SqlParameter("@Frequencyofdailymailingservice", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.FrequencyOfDailyMailingService == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.FrequencyOfDailyMailingService },
                    new SqlParameter("@ElectricSupply", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.MiscBranchInfo.ElectricSupply) ? (object)DBNull.Value : siteDetail.MiscBranchInfo.ElectricSupply },
                    new SqlParameter("@WaterSupply", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.MiscBranchInfo.WaterSupply) ? (object)DBNull.Value : siteDetail.MiscBranchInfo.WaterSupply },
                    new SqlParameter("@BranchOpenDate", SqlDbType.Date) { Value = siteDetail.MiscBranchInfo.BranchOpenDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.MiscBranchInfo.BranchOpenDate },
                    new SqlParameter("@TellersCounter", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.TellersCounter == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.TellersCounter },
                    new SqlParameter("@NoofSalesmanageroffices", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfSalesManagerOffices == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfSalesManagerOffices },
                    new SqlParameter("@ExistVIPsection", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.ExistVIPSection? 1 : 0 },
                    new SqlParameter("@ContractStartDate", SqlDbType.Date) { Value = siteDetail.MiscBranchInfo.ContractStartDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.MiscBranchInfo.ContractStartDate },
                    new SqlParameter("@NoofRenovationRetouchtime", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfRenovationRetouchTime == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfRenovationRetouchTime },
                    new SqlParameter("@LeasedOwbuilding", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.LeasedOwBuilding? 1 : 0 },
                    new SqlParameter("@Noofteaboys", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfTeaBoys == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfTeaBoys },
                    new SqlParameter("@Frequencyofmonthlycleaningservice", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.FrequencyOfMonthlyCleaningService == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.FrequencyOfMonthlyCleaningService },
                    new SqlParameter("@DrainSewerage", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(siteDetail.MiscBranchInfo.DrainSewerage) ? (object)DBNull.Value : siteDetail.MiscBranchInfo.DrainSewerage },
                    new SqlParameter("@CentralAC", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.CentralAC? 1 : 0 },
                    new SqlParameter("@SplitAC", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.SplitAC? 1 : 0 },
                    new SqlParameter("@WindowAC", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.WindowAC? 1 : 0 },
                    new SqlParameter("@Cashcountertype", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.CashCounterType == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.CashCounterType },
                    new SqlParameter("@NoofTellerCounters", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfTellerCounters == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfTellerCounters },
                    new SqlParameter("@Noofaffluentrelationshipmanageroffices", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfAffluentRelationshipManagerOffices == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfAffluentRelationshipManagerOffices },
                    new SqlParameter("@SeperateVIPsection", SqlDbType.Bit) { Value = siteDetail.MiscBranchInfo.SeperateVIPSection? 1 : 0 },
                    new SqlParameter("@ContractEndDate", SqlDbType.Date) { Value = siteDetail.MiscBranchInfo.ContractEndDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.MiscBranchInfo.ContractEndDate },
                    new SqlParameter("@RenovationRetouchDate", SqlDbType.Date) { Value = siteDetail.MiscBranchInfo.RenovationRetouchDate == DateTime.MinValue ? (object)DBNull.Value : siteDetail.MiscBranchInfo.RenovationRetouchDate },
                    new SqlParameter("@NoofTCRmachines", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfTCRMachines == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfTCRMachines },
                    new SqlParameter("@NoOfTotem", SqlDbType.Int) { Value = siteDetail.MiscBranchInfo.NoOfTotem == 0 ? (object)DBNull.Value : siteDetail.MiscBranchInfo.NoOfTotem },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_UpdateSiteDetails, CommandType.StoredProcedure, sqlParameters);

                int RowsAffected = (sqlParameters[74].Value != DBNull.Value) ? (int)sqlParameters[74].Value : -1;

                if (RowsAffected <= 0)
                {
                    return false;
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error updating record for {siteDetail.SiteName} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating record for {siteDetail.SiteName}.", ex);
            }
            return true;
        }

        public async Task <bool> UpdateSiteImagesAsync(List<SitePictures> sitePictures)
        {
            try
            {
                SqlParameter[] sqlParameters;

                foreach(var image in sitePictures)
                {
                    sqlParameters = new SqlParameter[] {
                        new SqlParameter("@SiteID", SqlDbType.BigInt) { Value = image.SiteID == 0 ? (object)DBNull.Value : image.SiteID },
                        new SqlParameter("@PicCatID", SqlDbType.Int) { Value = image.SitePicCategoryData.PicCatID == 0 ? (object)DBNull.Value : image.SitePicCategoryData.PicCatID },
                        new SqlParameter("@SitePicID", SqlDbType.Int) { Value = image.SitePicID < 0 ? (object)DBNull.Value : image.SitePicID },
                        new SqlParameter("@Description", SqlDbType.VarChar, -1) { Value = string.IsNullOrEmpty(image.Description) ? (object)DBNull.Value : image.Description },
                        new SqlParameter("@PicPath", SqlDbType.VarChar, -1) { Value = string.IsNullOrEmpty(image.PicPath) ? (object)DBNull.Value : image.PicPath },
                        new SqlParameter("@IsDeleted", SqlDbType.Bit) { Value = !image.IsDeleted.GetValueOrDefault() ? (object)DBNull.Value : image.IsDeleted },
                        new SqlParameter("@NewSitePicID", SqlDbType.Int) { Direction = ParameterDirection.Output },
                        new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                    };

                    bool isFailure = SqlDBHelper.ExecuteNonQueryWithTransaction(_transaction,SP_UpsertDeleteSitePictures, CommandType.StoredProcedure, sqlParameters);

                    // Get the output values from the stored procedure
                    image.SitePicID = sqlParameters[6].Value != DBNull.Value ? Convert.ToInt32(sqlParameters[6].Value) : -1;
                    int rowsAffected = sqlParameters[7].Value != DBNull.Value ? Convert.ToInt32(sqlParameters[7].Value) : 0;

                    // Verify the output from stored procedure to confirm upsert/delete
                    if (image.SitePicID <= 0 || rowsAffected <= 0)
                    {
                        return false;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error updating record - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating record", ex);
            }
            return true;
        }

        public async Task<ProfileUserAPIVM> GetUserRecordAsync(int newUserId)
        {
            ProfileUserAPIVM userSettings = new ProfileUserAPIVM();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetCurrentUserDetails },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        ProfileSite ProfileData = new ProfileSite
                        {
                            ProfileId = dataRow["ProfileId"] != DBNull.Value ? (int)dataRow["ProfileId"] : 0,
                            ProfileName = dataRow["ProfileName"] != DBNull.Value ? dataRow["ProfileName"].ToString() : string.Empty
                        };
                        userSettings = new ProfileUserAPIVM
                        {
                            UserId = dataRow["UserId"] != DBNull.Value ? (int)dataRow["UserId"] : 0,
                            FullName = dataRow["FullName"] != DBNull.Value ? dataRow["FullName"].ToString() : string.Empty,
                            EmailID = dataRow["EmailID"] != DBNull.Value ? dataRow["EmailID"].ToString() : string.Empty,
                            UserName = dataRow["UserName"] != DBNull.Value ? dataRow["UserName"].ToString() : string.Empty,
                            ProfilePicName = dataRow["ProfilePicName"] != DBNull.Value ? dataRow["ProfilePicName"].ToString() : string.Empty,
                            Designation = dataRow["Designation"] != DBNull.Value ? dataRow["Designation"].ToString() : string.Empty,
                            PhoneNumber = dataRow["PhoneNumber"] != DBNull.Value ? dataRow["PhoneNumber"].ToString() : string.Empty,
                            IsActive = dataRow["IsActive"] != DBNull.Value ? Convert.ToBoolean(dataRow["IsActive"]) : false,
                            IsADUser = dataRow["IsADUser"] != DBNull.Value ? Convert.ToBoolean(dataRow["IsADUser"]) : false,
                            IsDeleted = dataRow["IsDeleted"] != DBNull.Value ? Convert.ToBoolean(dataRow["IsDeleted"]) : false,
                            ProfileSiteData = ProfileData,
                            Password = string.Empty
                        };
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return userSettings;
        }
        public async Task<ProfileSite> GetCurrentUserProfileAsync(int CurrentUserId)
        {
            ProfileSite profileSite = new ProfileSite();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetCurrentUserProfile },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            profileSite = new ProfileSite
                            {
                                ProfileId = (int)row["ProfileId"],
                                ProfileName = row["ProfileName"].ToString()
                            };
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Current User Profile.", ex);
            }
            return profileSite;
        }
        public async Task<List<PageSiteVM>> GetCurrentUserPagesAsync(int CurrentUserId)
        {
            List<PageSiteVM> pages = new List<PageSiteVM>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetCurrentUserPages },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            PageSiteVM page = new PageSiteVM
                            {
                                PageId = (int)row["PageId"],
                                PageDesc = row["PageDesc"].ToString(),
                                PageUrl = row["PageUrl"].ToString(),
                                isSelected = true
                            };
                            pages.Add(page);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Current User Pages.", ex);
            }
            return pages;
        }
        public async Task<List<CategoriesSetDTO>> GetCurrentUserCategoriesAsync(int CurrentUserId)
        {
            List<CategoriesSetDTO> categoriesSet = new List<CategoriesSetDTO>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetCurrentUserCategories },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            CategoriesSetDTO category = new CategoriesSetDTO
                            {
                                PageCatId = row["PageCatId"] != DBNull.Value ? (int)row["PageCatId"] : 0,
                                CategoryName = row["CategoryName"] != DBNull.Value ? row["CategoryName"].ToString() : string.Empty,
                                PageId = row["PageId"] != DBNull.Value ? (int)row["PageId"] : 0,
                                PageDesc = row["PageDesc"] != DBNull.Value ? row["PageDesc"].ToString() : string.Empty,
                                PageUrl = row["PageUrl"] != DBNull.Value ? row["PageUrl"].ToString() : string.Empty,
                            };
                            categoriesSet.Add(category);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Current User Categories.", ex);
            }
            return categoriesSet;
        }
        public async Task<ProfileUserAPIVM> GetSettingsDataAsync(int CurrentUserId)
        {
            ProfileUserAPIVM userSettings = new ProfileUserAPIVM();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetSettingsData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        userSettings = new ProfileUserAPIVM
                        {
                            UserId = dataRow["UserId"] != DBNull.Value ? (int)dataRow["UserId"] : 0,
                            FullName = dataRow["FullName"] != DBNull.Value ? dataRow["FullName"].ToString() : string.Empty,
                            EmailID = dataRow["EmailID"] != DBNull.Value ? dataRow["EmailID"].ToString() : string.Empty,
                            UserName = dataRow["UserName"] != DBNull.Value ? dataRow["UserName"].ToString() : string.Empty,
                            ProfilePicName = dataRow["ProfilePicName"] != DBNull.Value ? dataRow["ProfilePicName"].ToString() : string.Empty,
                            Designation = dataRow["Designation"] != DBNull.Value ? dataRow["Designation"].ToString() : string.Empty,
                            PhoneNumber = dataRow["PhoneNumber"] != DBNull.Value ? dataRow["PhoneNumber"].ToString() : string.Empty,
                        };
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return userSettings;
        }
        public async Task<ProfileSite> GetProfileDataAsync(int newUserId)
        {
            ProfileSite profileSite = new ProfileSite();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetProfileData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        profileSite = new ProfileSite
                        {
                            ProfileId = dataRow["ProfileId"] != DBNull.Value ? (int)dataRow["ProfileId"] : 0,
                            ProfileName = dataRow["ProfileName"] != DBNull.Value ? dataRow["ProfileName"].ToString() : string.Empty
                        };
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return profileSite;
        }
        public async Task<PageCategory> GetCategoryDataAsync(int newUserId)
        {
            PageCategory pageCategory = new PageCategory();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetCategoryData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        pageCategory = new PageCategory
                        {
                            PageCatId = dataRow["PageCatId"] != DBNull.Value ? (int)dataRow["PageCatId"] : 0,
                            CategoryName = dataRow["CategoryName"] != DBNull.Value ? dataRow["CategoryName"].ToString() : string.Empty,
                            PageId = 0
                        };
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return pageCategory;
        }
        public async Task<Region> GetRegionDataAsync(int newUserId)
        {
            Region region = new Region();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetRegionData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        region = new Region
                        {
                            RegionId = dataRow["RegionId"] != DBNull.Value ? (int)dataRow["RegionId"] : 0,
                            RegionName = dataRow["RegionName"] != DBNull.Value ? dataRow["RegionName"].ToString() : string.Empty
                        };
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return region;
        }
        public async Task<City> GetCityDataAsync(int newUserId)
        {
            City citee = new City();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetCityData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        citee = new City
                        {
                            CityId = dataRow["CityId"] != DBNull.Value ? (int)dataRow["CityId"] : 0,
                            CityName = dataRow["CityName"] != DBNull.Value ? dataRow["CityName"].ToString() : string.Empty,
                            RegionData = new Region
                            {
                                RegionId = dataRow["RegionId"] != DBNull.Value ? (int)dataRow["RegionId"] : 0,
                                RegionName = dataRow["RegionName"] != DBNull.Value ? dataRow["RegionName"].ToString() : string.Empty
                            }
                        };
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return citee;
        }
        public async Task<SiteLocation> GetLocationDataAsync(int newUserId)
        {
            SiteLocation site = new SiteLocation();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetLocationData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        site = new SiteLocation
                        {
                            LocationId = dataRow["LocationId"] != DBNull.Value ? (int)dataRow["LocationId"] : 0,
                            Sponsor = new Sponsor
                            {
                                SponsorId = dataRow["SponsorId"] != DBNull.Value ? (int)dataRow["SponsorId"] : 0,
                                SponsorName = dataRow["SponsorName"] != DBNull.Value ? dataRow["SponsorName"].ToString() : string.Empty
                            },
                            Location = dataRow["Location"] != DBNull.Value ? dataRow["Location"].ToString() : string.Empty,
                            StreetName = dataRow["StreetName"] != DBNull.Value ? dataRow["StreetName"].ToString() : string.Empty,
                            DistrictName = dataRow["DistrictName"] != DBNull.Value ? dataRow["DistrictName"].ToString() : string.Empty,
                            BranchName = dataRow["BranchName"] != DBNull.Value ? dataRow["BranchName"].ToString() : string.Empty,
                            City = new City
                            {
                                CityId = dataRow["CityId"] != DBNull.Value ? (int)dataRow["CityId"] : 0,
                                CityName = dataRow["CityName"] != DBNull.Value ? dataRow["CityName"].ToString() : string.Empty,
                                RegionData = new Region
                                {
                                    RegionId = dataRow["RegionId"] != DBNull.Value ? (int)dataRow["RegionId"] : 0,
                                    RegionName = dataRow["RegionName"] != DBNull.Value ? dataRow["RegionName"].ToString() : string.Empty,
                                }
                            }
                        };
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return site;
        }
        public async Task<Contact> GetContactDataAsync(int newUserId)
        {
            Contact contact = new Contact();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdOrTextEnum.GetContactData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        contact = new Contact
                        {
                            ContactId = dataRow["ContactId"] != DBNull.Value ? (int)dataRow["ContactId"] : 0,
                            sponsor = new Sponsor
                            {
                                SponsorId = dataRow["SponsorId"] != DBNull.Value ? (int)dataRow["SponsorId"] : 0,
                                SponsorName = dataRow["SponsorName"] != DBNull.Value ? dataRow["SponsorName"].ToString() : string.Empty
                            },
                            Designation = dataRow["Designation"] != DBNull.Value ? dataRow["Designation"].ToString() : string.Empty,
                            OfficePhone = dataRow["OfficePhone"] != DBNull.Value ? dataRow["OfficePhone"].ToString() : string.Empty,
                            Mobile = dataRow["Mobile"] != DBNull.Value ? dataRow["Mobile"].ToString() : string.Empty,
                            EmailID = dataRow["EmailID"] != DBNull.Value ? dataRow["EmailID"].ToString() : string.Empty,
                            Fax = dataRow["Fax"] != DBNull.Value ? dataRow["Fax"].ToString() : string.Empty,
                            Name = dataRow["Name"] != DBNull.Value ? dataRow["Name"].ToString() : string.Empty,
                            BranchName = dataRow["BranchName"] != DBNull.Value ? dataRow["BranchName"].ToString() : string.Empty,
                        };
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return contact;
        }
        public async Task<SiteDetail> GetSiteDetailsDataAsync(Record record)
        {
            SiteDetail site = new SiteDetail();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = record.RecordType != 0 ? record.RecordType : DBNull.Value  },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = record.RecordId != 0 ? record.RecordId : DBNull.Value },
                    new SqlParameter("@InputText", SqlDbType.VarChar, 100) { Value = record.RecordText != string.Empty ? record.RecordText : Constants.SitePicCategory_SiteProfilePicture },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable1 = dataTables[0];
                    if (dataTable1.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable1.Rows)
                        {
                            site = new SiteDetail
                            {
                                SiteID = Convert.ToInt64(dataRow["SiteID"]),
                                SiteCode = GetString(dataRow["SiteCode"]),
                                SiteName = GetString(dataRow["SiteName"]),
                                SiteCategory = GetString(dataRow["SiteCategory"]),
                                SponsorID = GetNullableInt(dataRow["SponsorID"]) ?? 0,
                                SponsorName = GetString(dataRow["SponsorName"]),
                                RegionID = GetNullableInt(dataRow["RegionID"]) ?? 0,
                                RegionName = GetString(dataRow["RegionName"]),
                                CityID = GetNullableInt(dataRow["CityID"]) ?? 0,
                                CityName = GetString(dataRow["CityName"]),
                                LocationID = GetNullableInt(dataRow["LocationID"]) ?? 0,
                                LocationName = GetString(dataRow["LocationName"]),
                                ContactID = GetNullableInt(dataRow["ContactID"]) ?? 0,
                                ContactName = GetString(dataRow["ContactName"]),
                                SiteTypeID = GetNullableInt(dataRow["SiteTypeID"]) ?? 0,
                                SiteTypeDescription = GetString(dataRow["SiteTypeDescription"]),
                                GPSLong = GetString(dataRow["GPSLong"]),
                                GPSLatt = GetString(dataRow["GPSLatt"]),
                                VisitUserID = GetNullableInt(dataRow["VisitUserID"]),
                                VisitedDate = GetNullableDateTime(dataRow["VisitedDate"]),
                                ApprovedUserID = GetNullableInt(dataRow["ApprovedUserID"]),
                                ApprovalDate = GetNullableDateTime(dataRow["ApprovalDate"]),
                                VisitStatusID = GetNullableInt(dataRow["VisitStatusID"]),
                                IsActive = GetBoolean(dataRow["IsActive"]),
                                BranchNo = GetString(dataRow["BranchNo"]),
                                BranchTypeId = GetNullableInt(dataRow["BranchTypeId"]),
                                BranchTypeDescription = GetString(dataRow["BranchTypeDescription"]),
                                AtmClass = GetString(dataRow["AtmClass"]),

                                // Contact Information
                                ContactInformation = new SiteContactInformation
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    BranchTelephoneNumber = GetString(dataRow["BranchTelephoneNumber"]),
                                    BranchFaxNumber = GetString(dataRow["BranchFaxNumber"]),
                                    EmailAddress = GetString(dataRow["EmailAddress"])
                                },

                                // Geographical Details
                                GeographicalDetails = new GeographicalDetails
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    NearestLandmark = GetString(dataRow["NearestLandmark"]),
                                    NumberOfKmNearestCity = GetString(dataRow["NumberOfKmNearestCity"]),
                                    BranchConstructionType = GetString(dataRow["BranchConstructionType"]),
                                    BranchIsLocatedAt = GetString(dataRow["BranchIsLocatedAt"]),
                                    HowToReachThere = GetString(dataRow["HowToReachThere"]),
                                    SiteIsOnServiceRoad = GetBoolean(dataRow["SiteIsOnServiceRoad"]),
                                    HowToGetThere = GetString(dataRow["HowToGetThere"])
                                },

                                // Branch Facilities
                                BranchFacilities = new SiteBranchFacilities
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    Parking = GetBoolean(dataRow["Parking"]),
                                    Landscape = GetBoolean(dataRow["Landscape"]),
                                    Elevator = GetBoolean(dataRow["Elevator"]),
                                    VIPSection = GetBoolean(dataRow["VIPSection"]),
                                    SafeBox = GetBoolean(dataRow["SafeBox"]),
                                    ICAP = GetBoolean(dataRow["ICAP"])
                                },

                                // Data Center Information
                                DataCenter = new SiteDataCenter
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    UPSBrand = GetString(dataRow["UPSBrand"]),
                                    UPSCapacity = GetString(dataRow["UPSCapacity"]),
                                    PABXBrand = GetString(dataRow["PABXBrand"]),
                                    StabilizerBrand = GetString(dataRow["StabilizerBrand"]),
                                    StabilizerCapacity = GetString(dataRow["StabilizerCapacity"]),
                                    SecurityAccessSystemBrand = GetString(dataRow["SecurityAccessSystemBrand"])
                                },

                                // SignBoard Type
                                SignBoard = new SignBoardType
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    Cylinder = GetBoolean(dataRow["Cylinder"]),
                                    StraightOrTotem = GetBoolean(dataRow["StraightOrTotem"])
                                },

                                // Miscellaneous Site Information
                                MiscSiteInfo = new SiteMiscInformation
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    TypeOfATMLocation = GetString(dataRow["TypeOfATMLocation"]),
                                    NoOfExternalCameras = GetNullableInt(dataRow["NoOfExternalCameras"]),
                                    NoOfInternalCameras = GetNullableInt(dataRow["NoOfInternalCameras"]),
                                    TrackingSystem = GetString(dataRow["TrackingSystem"])
                                },

                                // Miscellaneous Branch Information
                                MiscBranchInfo = new BranchMiscInformation
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    NoOfCleaners = GetNullableInt(dataRow["Noofcleaners"]),
                                    FrequencyOfDailyMailingService = GetNullableInt(dataRow["Frequencyofdailymailingservice"]),
                                    ElectricSupply = GetString(dataRow["ElectricSupply"]),
                                    WaterSupply = GetString(dataRow["WaterSupply"]),
                                    BranchOpenDate = GetNullableDateTime(dataRow["BranchOpenDate"]),
                                    TellersCounter = GetNullableInt(dataRow["TellersCounter"]),
                                    NoOfSalesManagerOffices = GetNullableInt(dataRow["NoofSalesmanageroffices"]),
                                    ExistVIPSection = GetBoolean(dataRow["ExistVIPsection"]),
                                    ContractStartDate = GetNullableDateTime(dataRow["ContractStartDate"]),
                                    NoOfRenovationRetouchTime = GetNullableInt(dataRow["NoofRenovationRetouchtime"]),
                                    LeasedOwBuilding = GetBoolean(dataRow["LeasedOwbuilding"]),
                                    NoOfTeaBoys = GetNullableInt(dataRow["Noofteaboys"]),
                                    FrequencyOfMonthlyCleaningService = GetNullableInt(dataRow["Frequencyofmonthlycleaningservice"]),
                                    DrainSewerage = GetString(dataRow["DrainSewerage"]),
                                    CentralAC = GetBoolean(dataRow["CentralAC"]),
                                    SplitAC = GetBoolean(dataRow["SplitAC"]),
                                    WindowAC = GetBoolean(dataRow["WindowAC"]),
                                    CashCounterType = GetNullableInt(dataRow["Cashcountertype"]),
                                    NoOfTellerCounters = GetNullableInt(dataRow["NoofTellerCounters"]),
                                    NoOfAffluentRelationshipManagerOffices = GetNullableInt(dataRow["Noofaffluentrelationshipmanageroffices"]),
                                    SeperateVIPSection = GetBoolean(dataRow["SeperateVIPsection"]),
                                    ContractEndDate = GetNullableDateTime(dataRow["ContractEndDate"]),
                                    RenovationRetouchDate = GetNullableDateTime(dataRow["RenovationRetouchDate"]),
                                    NoOfTCRMachines = GetNullableInt(dataRow["NoofTCRmachines"]),
                                    NoOfTotem = GetNullableInt(dataRow["NoOfTotem"])
                                },
                                SitePicturesLst = new List<SitePictures>()
                            };
                        }
                    }
                    DataTable dataTable2 = dataTables[1];
                    if (dataTable2.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable2.Rows)
                        {
                            // Create a new SitePicCategory object
                            var sitePicCategory = new SitePicCategory
                            {
                                PicCatID = GetNullableInt(dataRow["PicCatID"]) ?? 0, // Default to 0 if null
                                Description = GetString(dataRow["SitePicCategoryDescription"])
                            };

                            // Create a new SitePictures object and map properties
                            var sitePicture = new SitePictures
                            {
                                SitePicID = GetNullableInt(dataRow["SitePicID"]) ?? 0, // Default to 0 if null
                                SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0, // Default to 0 if null
                                Description = GetString(dataRow["SitePicturesDescription"]),
                                PicPath = GetString(dataRow["PicPath"]),
                                SitePicCategoryData = sitePicCategory // Set the category data
                            };

                            // Add the site picture to the list
                            site.SitePicturesLst.Add(sitePicture);
                        }
                    }
                    else
                    {
                        //
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return site;
        }
        public async Task<List<SitePictures>> GetSitePicturesDataAsync(Record record)
        {
            List<SitePictures> sitePictures = new List<SitePictures>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = record.RecordType != 0 ? record.RecordType : DBNull.Value  },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = record.RecordId != 0 ? record.RecordId : DBNull.Value },
                    new SqlParameter("@InputText", SqlDbType.VarChar, 100) { Value = record.RecordText != string.Empty ? record.RecordText : DBNull.Value },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            // Create a new SitePicCategory object
                            var sitePicCategory = new SitePicCategory
                            {
                                PicCatID = GetNullableInt(dataRow["PicCatID"]) ?? 0, // Default to 0 if null
                                Description = GetString(dataRow["SitePicCategoryDescription"])
                            };

                            // Create a new SitePictures object and map properties
                            var sitePicture = new SitePictures
                            {
                                SitePicID = GetNullableInt(dataRow["SitePicID"]) ?? 0, // Default to 0 if null
                                SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0, // Default to 0 if null
                                Description = GetString(dataRow["SitePicturesDescription"]),
                                PicPath = GetString(dataRow["PicPath"]),
                                SitePicCategoryData = sitePicCategory // Set the category data
                            };

                            // Add the site picture to the list
                            sitePictures.Add(sitePicture);
                        }
                    }
                    else
                    {
                        //
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return sitePictures;
        }
        public async Task<SitePictures> GetSitePictureDataAsync(Record record)
        {
            SitePictures sitePicture = new SitePictures();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = record.RecordType != 0 ? record.RecordType : DBNull.Value  },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = record.RecordId != 0 ? record.RecordId : DBNull.Value },
                    new SqlParameter("@InputText", SqlDbType.VarChar, 100) { Value = record.RecordText != string.Empty ? record.RecordText : DBNull.Value },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_FetchRecordByIdOrText, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];

                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            // Create a new SitePicCategory object
                            var sitePicCategory = new SitePicCategory
                            {
                                PicCatID = GetNullableInt(dataRow["PicCatID"]) ?? 0, // Default to 0 if null
                                Description = string.Empty
                            };

                            // Create a new SitePictures object and map properties
                            sitePicture = new SitePictures
                            {
                                SitePicID = GetNullableInt(dataRow["SitePicID"]) ?? 0, // Default to 0 if null
                                SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0, // Default to 0 if null
                                Description = GetString(dataRow["SitePicturesDescription"]),
                                PicPath = GetString(dataRow["PicPath"]),
                                SitePicCategoryData = sitePicCategory // Set the category data
                            };
                        }
                    }
                    else
                    {
                        //
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Settings.", ex);
            }
            return sitePicture;
        }

        public async Task<List<ProfileUserAPIVM>> GetAllUsersDataAsync()
        {
            List<ProfileUserAPIVM> users = new List<ProfileUserAPIVM>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllUsersData },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ProfileUserAPIVM profileUser = new ProfileUserAPIVM
                            {
                                UserId = Convert.ToInt32(dataRow["UserId"]),
                                Designation = dataRow["Designation"].ToString(),
                                FullName = dataRow["FullName"].ToString(),
                                EmailID = dataRow["EmailID"].ToString(),
                                PhoneNumber = dataRow["PhoneNumber"].ToString(),
                                ProfileSiteData = new ProfileSite
                                {
                                    ProfileId = Convert.ToInt32(dataRow["ProfileId"]),
                                    ProfileName = dataRow["ProfileName"].ToString()
                                },
                                UserName = dataRow["UserName"].ToString(),
                                ProfilePicName = dataRow["ProfilePicName"].ToString(),
                                IsActive = Convert.ToBoolean(dataRow["IsActive"]),
                                IsADUser = Convert.ToBoolean(dataRow["IsADUser"]),
                                IsDeleted = Convert.ToBoolean(dataRow["IsDeleted"]),
                            };
                            users.Add(profileUser);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Users Details.", ex);
            }
            return users;
        }
        public async Task<List<ProfileSite>> GetAllProfilesAsync()
        {
            List<ProfileSite> profiles = new List<ProfileSite>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllProfiles },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ProfileSite profile = new ProfileSite
                            {
                                ProfileId = (int)dataRow["ProfileId"],
                                ProfileName = dataRow["ProfileName"].ToString()
                            };
                            profiles.Add(profile);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Profiles.", ex);
            }
            return profiles;
        }
        public async Task<List<PageSite>> GetAllPagesAsync()
        {
            List<PageSite> pages = new List<PageSite>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllProfiles },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            PageSite page = new PageSite
                            {
                                PageId = (int)row["PageId"],
                                PageUrl = row["PageUrl"].ToString(),
                                PageDesc = row["PageDesc"].ToString(),
                                PageSeq = (int)row["PageSeq"],
                                PageCat = (int)row["PageCat"],
                                PageImgUrl = row["PageImgUrl"].ToString(),
                                PageName = row["PageName"].ToString(),
                            };
                            pages.Add(page);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All the Pages.", ex);
            }
            return pages;
        }
        public async Task<List<PageCategory>> GetAllCategoriesAsync()
        {
            List<PageCategory> pageCategories = new List<PageCategory>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllProfiles },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            PageCategory pageCategory = new PageCategory
                            {
                                PageCatId = (int)row["PageCatId"],
                                CategoryName = row["CategoryName"].ToString(),
                                PageId = 0,
                            };
                            pageCategories.Add(pageCategory);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Get All the Page Categories.", ex);
            }
            return pageCategories;
        }
        public async Task<List<Region>> GetAllRegionsAsync()
        {
            List<Region> regions = new List<Region>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllRegions },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            Region region = new Region
                            {
                                RegionId = Convert.ToInt32(dataRow["RegionId"]),
                                RegionName = dataRow["RegionName"].ToString(),
                            };
                            regions.Add(region);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Regions List.", ex);
            }
            return regions;
        }
        public async Task<List<City>> GetAllCitiesAsync()
        {
            List<City> cities = new List<City>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllCities },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            City city = new City
                            {
                                CityId = Convert.ToInt32(dataRow["CityId"]),
                                CityName = dataRow["CityName"].ToString(),
                                RegionData = new Region
                                {
                                    RegionId = Convert.ToInt32(dataRow["RegionId"]),
                                    RegionName = dataRow["RegionName"].ToString()
                                }
                            };
                            cities.Add(city);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Cities List.", ex);
            }
            return cities;
        }
        public async Task<List<SiteLocation>> GetAllLocationsAsync()
        {
            List<SiteLocation> sites = new List<SiteLocation>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllLocations },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            SiteLocation site = new SiteLocation
                            {
                                LocationId = Convert.ToInt32(dataRow["LocationId"]),
                                Location = dataRow["Location"].ToString(),
                                StreetName = dataRow["StreetName"].ToString(),
                                BranchName = dataRow["BranchName"].ToString(),
                                DistrictName = dataRow["DistrictName"].ToString(),
                                Sponsor = new Sponsor
                                {
                                    SponsorId = Convert.ToInt32(dataRow["SponsorId"]),
                                    SponsorName = dataRow["SponsorName"].ToString()
                                },
                                City = new City
                                {
                                    CityId = Convert.ToInt32(dataRow["CityId"]),
                                    CityName = dataRow["CityName"].ToString(),
                                    RegionData = new Region
                                    {
                                        RegionId = Convert.ToInt32(dataRow["RegionId"]),
                                        RegionName = dataRow["RegionName"].ToString()
                                    }
                                }
                            };
                            sites.Add(site);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Locations List.", ex);
            }
            return sites;
        }
        public async Task<List<CityRegionViewModel>> GetRegionListOfCitiesAsync()
        {
            List<CityRegionViewModel> cityRegionsList = new List<CityRegionViewModel>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetRegionListOfCities },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            CityRegionViewModel cityRegion = new CityRegionViewModel
                            {
                                CityId = Convert.ToInt32(dataRow["CityId"]),
                                CityName = dataRow["CityName"].ToString(),
                                RegionId = Convert.ToInt32(dataRow["RegionId"]),
                                RegionName = dataRow["RegionName"].ToString(),
                            };
                            cityRegionsList.Add(cityRegion);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting Region's Associated List Of Cities.", ex);
            }
            return cityRegionsList;
        }
        public async Task<List<Sponsor>> GetAllSponsorsAsync()
        {
            List<Sponsor> sponsors = new List<Sponsor>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllSponsors },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            Sponsor sponsor = new Sponsor
                            {
                                SponsorId = Convert.ToInt32(dataRow["SponsorId"]),
                                SponsorName = dataRow["SponsorName"].ToString(),
                            };
                            sponsors.Add(sponsor);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Sponsors List.", ex);
            }
            return sponsors;
        }
        public async Task<List<Contact>> GetAllContactsAsync()
        {
            List<Contact> contacts = new List<Contact>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllContacts },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            Contact contact = new Contact
                            {
                                ContactId = Convert.ToInt32(dataRow["ContactId"]),
                                Name = dataRow["Name"].ToString(),
                                Designation = dataRow["Designation"].ToString(),
                                OfficePhone = dataRow["OfficePhone"].ToString(),
                                Mobile = dataRow["Mobile"].ToString(),
                                EmailID = dataRow["EmailID"].ToString(),
                                Fax = dataRow["Fax"].ToString(),
                                BranchName = dataRow["BranchName"].ToString(),
                                sponsor = new Sponsor
                                {
                                    SponsorId = Convert.ToInt32(dataRow["SponsorId"]),
                                    SponsorName = dataRow["SponsorName"].ToString(),
                                }
                            };
                            contacts.Add(contact);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Sponsors List.", ex);
            }
            return contacts;
        }
        public async Task<List<SiteType>> GetAllSiteTypesAsync()
        {
            List<SiteType> siteTypes = new List<SiteType>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllSiteTypes },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            SiteType siteType = new SiteType
                            {
                                SiteTypeID = Convert.ToInt32(dataRow["SiteTypeID"]),
                                Description = dataRow["Description"].ToString(),
                                sponsor = new Sponsor
                                {
                                    SponsorId = Convert.ToInt32(dataRow["SponsorId"]),
                                    SponsorName = dataRow["SponsorName"].ToString()
                                }
                            };
                            siteTypes.Add(siteType);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All SiteType List.", ex);
            }
            return siteTypes;
        }
        public async Task<List<BranchType>> GetAllBranchTypesAsync()
        {
            List<BranchType> branchTypes = new List<BranchType>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllBranchTypes },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            BranchType branchType = new BranchType
                            {
                                BranchTypeId = Convert.ToInt32(dataRow["BranchTypeId"]),
                                Description = dataRow["BranchDescription"].ToString(),
                                siteType = new SiteType
                                {
                                    SiteTypeID = Convert.ToInt32(dataRow["SiteTypeID"]),
                                    Description = dataRow["SiteDescription"].ToString(),
                                    sponsor = new Sponsor
                                    {
                                        SponsorId = Convert.ToInt32(dataRow["SponsorId"]),
                                        SponsorName = dataRow["SponsorName"].ToString()
                                    }
                                }

                            };
                            branchTypes.Add(branchType);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Branch Type List.", ex);
            }
            return branchTypes;
        }
        public async Task<List<VisitStatusModel>> GetAllVisitStatusesAsync()
        {
            List<VisitStatusModel> visitStatuses = new List<VisitStatusModel>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllVisitStatuses },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            VisitStatusModel visitStatus = new VisitStatusModel
                            {
                                VisitStatusID = Convert.ToInt32(dataRow["VisitStatusID"]),
                                VisitStatus = dataRow["VisitStatus"].ToString(),
                            };
                            visitStatuses.Add(visitStatus);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Visit Status List.", ex);
            }
            return visitStatuses;
        }
        public async Task<List<string>> GetAllATMClassesAsync()
        {
            List<string> atmClasses = new List<string>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllATMClasses },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            string classToken = dataRow["Class"].ToString();
                            atmClasses.Add(classToken);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Visit Status List.", ex);
            }
            return atmClasses;
        }
        public async Task<List<SitePicCategory>> GetAllPicCategoriesAsync()
        {
            List<SitePicCategory> sitePicCategories = new List<SitePicCategory>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllPicCategories },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            SitePicCategory sitePicCategory = new SitePicCategory
                            {
                                PicCatID = Convert.ToInt32(dataRow["PicCatID"]),
                                Description = dataRow["Description"].ToString(),
                            };
                            sitePicCategories.Add(sitePicCategory);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Visit Status List.", ex);
            }
            return sitePicCategories;
        }
        public async Task<List<SiteDetail>> GetAllSiteDetailsAsync()
        {
            List<SiteDetail> sites = new List<SiteDetail>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = GetTableData.GetAllSiteDetails },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQueryWithTransaction(_transaction,SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
                if (dataTables.Count > 0)
                {
                    DataTable dataTable = dataTables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            SiteDetail site = new SiteDetail
                            {
                                SiteID = Convert.ToInt64(dataRow["SiteID"]),
                                SiteCode = GetString(dataRow["SiteCode"]),
                                SiteName = GetString(dataRow["SiteName"]),
                                SiteCategory = GetString(dataRow["SiteCategory"]),
                                SponsorID = GetNullableInt(dataRow["SponsorID"]) ?? 0,
                                RegionID = GetNullableInt(dataRow["RegionID"]) ?? 0,
                                CityID = GetNullableInt(dataRow["CityID"]) ?? 0,
                                LocationID = GetNullableInt(dataRow["LocationID"]) ?? 0,
                                ContactID = GetNullableInt(dataRow["ContactID"]) ?? 0,
                                SiteTypeID = GetNullableInt(dataRow["SiteTypeID"]) ?? 0,
                                GPSLong = GetString(dataRow["GPSLong"]),
                                GPSLatt = GetString(dataRow["GPSLatt"]),
                                VisitUserID = GetNullableInt(dataRow["VisitUserID"]),
                                VisitedDate = GetNullableDateTime(dataRow["VisitedDate"]),
                                ApprovedUserID = GetNullableInt(dataRow["ApprovedUserID"]),
                                ApprovalDate = GetNullableDateTime(dataRow["ApprovalDate"]),
                                VisitStatusID = GetNullableInt(dataRow["VisitStatusID"]),
                                IsActive = GetBoolean(dataRow["IsActive"]),
                                BranchNo = GetString(dataRow["BranchNo"]),
                                BranchTypeId = GetNullableInt(dataRow["BranchTypeId"]),
                                AtmClass = GetString(dataRow["AtmClass"]),

                                // Contact Information
                                ContactInformation = new SiteContactInformation
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    BranchTelephoneNumber = GetString(dataRow["BranchTelephoneNumber"]),
                                    BranchFaxNumber = GetString(dataRow["BranchFaxNumber"]),
                                    EmailAddress = GetString(dataRow["EmailAddress"])
                                },

                                // Geographical Details
                                GeographicalDetails = new GeographicalDetails
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    NearestLandmark = GetString(dataRow["NearestLandmark"]),
                                    NumberOfKmNearestCity = GetString(dataRow["NumberOfKmNearestCity"]),
                                    BranchConstructionType = GetString(dataRow["BranchConstructionType"]),
                                    BranchIsLocatedAt = GetString(dataRow["BranchIsLocatedAt"]),
                                    HowToReachThere = GetString(dataRow["HowToReachThere"]),
                                    SiteIsOnServiceRoad = GetBoolean(dataRow["SiteIsOnServiceRoad"]),
                                    HowToGetThere = GetString(dataRow["HowToGetThere"])
                                },

                                // Branch Facilities
                                BranchFacilities = new SiteBranchFacilities
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    Parking = GetBoolean(dataRow["Parking"]),
                                    Landscape = GetBoolean(dataRow["Landscape"]),
                                    Elevator = GetBoolean(dataRow["Elevator"]),
                                    VIPSection = GetBoolean(dataRow["VIPSection"]),
                                    SafeBox = GetBoolean(dataRow["SafeBox"]),
                                    ICAP = GetBoolean(dataRow["ICAP"])
                                },

                                // Data Center Information
                                DataCenter = new SiteDataCenter
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    UPSBrand = GetString(dataRow["UPSBrand"]),
                                    UPSCapacity = GetString(dataRow["UPSCapacity"]),
                                    PABXBrand = GetString(dataRow["PABXBrand"]),
                                    StabilizerBrand = GetString(dataRow["StabilizerBrand"]),
                                    StabilizerCapacity = GetString(dataRow["StabilizerCapacity"]),
                                    SecurityAccessSystemBrand = GetString(dataRow["SecurityAccessSystemBrand"])
                                },

                                // SignBoard Type
                                SignBoard = new SignBoardType
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    Cylinder = GetBoolean(dataRow["Cylinder"]),
                                    StraightOrTotem = GetBoolean(dataRow["StraightOrTotem"])
                                },

                                // Miscellaneous Site Information
                                MiscSiteInfo = new SiteMiscInformation
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    TypeOfATMLocation = GetString(dataRow["TypeOfATMLocation"]),
                                    NoOfExternalCameras = GetNullableInt(dataRow["NoOfExternalCameras"]),
                                    NoOfInternalCameras = GetNullableInt(dataRow["NoOfInternalCameras"]),
                                    TrackingSystem = GetString(dataRow["TrackingSystem"])
                                },

                                // Miscellaneous Branch Information
                                MiscBranchInfo = new BranchMiscInformation
                                {
                                    SiteID = GetNullableLong(dataRow["SiteID"]) ?? 0,
                                    NoOfCleaners = GetNullableInt(dataRow["Noofcleaners"]),
                                    FrequencyOfDailyMailingService = GetNullableInt(dataRow["Frequencyofdailymailingservice"]),
                                    ElectricSupply = GetString(dataRow["ElectricSupply"]),
                                    WaterSupply = GetString(dataRow["WaterSupply"]),
                                    BranchOpenDate = GetNullableDateTime(dataRow["BranchOpenDate"]),
                                    TellersCounter = GetNullableInt(dataRow["TellersCounter"]),
                                    NoOfSalesManagerOffices = GetNullableInt(dataRow["NoofSalesmanageroffices"]),
                                    ExistVIPSection = GetBoolean(dataRow["ExistVIPsection"]),
                                    ContractStartDate = GetNullableDateTime(dataRow["ContractStartDate"]),
                                    NoOfRenovationRetouchTime = GetNullableInt(dataRow["NoofRenovationRetouchtime"]),
                                    LeasedOwBuilding = GetBoolean(dataRow["LeasedOwbuilding"]),
                                    NoOfTeaBoys = GetNullableInt(dataRow["Noofteaboys"]),
                                    FrequencyOfMonthlyCleaningService = GetNullableInt(dataRow["Frequencyofmonthlycleaningservice"]),
                                    DrainSewerage = GetString(dataRow["DrainSewerage"]),
                                    CentralAC = GetBoolean(dataRow["CentralAC"]),
                                    SplitAC = GetBoolean(dataRow["SplitAC"]),
                                    WindowAC = GetBoolean(dataRow["WindowAC"]),
                                    CashCounterType = GetNullableInt(dataRow["Cashcountertype"]),
                                    NoOfTellerCounters = GetNullableInt(dataRow["NoofTellerCounters"]),
                                    NoOfAffluentRelationshipManagerOffices = GetNullableInt(dataRow["Noofaffluentrelationshipmanageroffices"]),
                                    SeperateVIPSection = GetBoolean(dataRow["SeperateVIPsection"]),
                                    ContractEndDate = GetNullableDateTime(dataRow["ContractEndDate"]),
                                    RenovationRetouchDate = GetNullableDateTime(dataRow["RenovationRetouchDate"]),
                                    NoOfTCRMachines = GetNullableInt(dataRow["NoofTCRmachines"]),
                                    NoOfTotem = GetNullableInt(dataRow["NoOfTotem"])
                                }
                            };
                            sites.Add(site);
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
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Visit Status List.", ex);
            }
            return sites;
        }
    }
}
