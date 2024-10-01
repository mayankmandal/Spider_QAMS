using Spider_QAMS.DAL;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Models;
using Spider_QAMS.Repositories.Skeleton;
using System.Data.SqlClient;
using System.Data;
using static Spider_QAMS.Utilities.Constants;

namespace Spider_QAMS.Repositories.Domain
{
    public class NavigationRepository : INavigationRepository
    {
        public async Task<object> FetchRecordByTypeAsync(Record record)
        {
            switch (record.RecordType)
            {
                case (int)FetchRecordByIdEnum.GetCurrentUserDetails:
                    return await GetUserRecordAsync(record.RecordId);
                case (int)FetchRecordByIdEnum.GetCurrentUserProfile:
                    return await GetCurrentUserProfileAsync(record.RecordId);
                case (int)FetchRecordByIdEnum.GetCurrentUserPages:
                    return await GetCurrentUserPagesAsync(record.RecordId);
                case (int)FetchRecordByIdEnum.GetCurrentUserCategories:
                    return await GetCurrentUserCategoriesAsync(record.RecordId);
                case (int)FetchRecordByIdEnum.GetSettingsData:
                    return await GetSettingsDataAsync(record.RecordId);
                case (int)FetchRecordByIdEnum.GetProfileData:
                    return await GetProfileDataAsync(record.RecordId);
                case (int)FetchRecordByIdEnum.GetCategoryData:
                    return await GetCategoryDataAsync(record.RecordId);
                case (int)FetchRecordByIdEnum.GetRegionData:
                    return await GetRegionDataAsync(record.RecordId);
                case (int)FetchRecordByIdEnum.GetCityData:
                    return await GetCityDataAsync(record.RecordId);
                case (int)FetchRecordByIdEnum.GetLocationData:
                    return await GetLocationDataAsync(record.RecordId);
                case (int)FetchRecordByIdEnum.GetContactData:
                    return await GetContactDataAsync(record.RecordId);
                default:
                    throw new Exception("Invalid Record Type");
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
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_AddNewUser, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetSettingsData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_UpdateUser, CommandType.StoredProcedure, sqlParameters);
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
        public async Task<bool> CheckUniquenessAsync(string field, string value)
        {
            int isUnique = 0;
            try
            {
                TableNameCheckUniqueness? tableEnum = null;
                if (TableNameClassForUniqueness.User.Contains(field.ToLower()))
                {
                    tableEnum = TableNameCheckUniqueness.User;
                }
                else if (TableNameClassForUniqueness.Profile.Contains(field.ToLower()))
                {
                    tableEnum = TableNameCheckUniqueness.Profile;
                }
                else if (TableNameClassForUniqueness.PageCategory.Contains(field.ToLower()))
                {
                    tableEnum = TableNameCheckUniqueness.PageCategory;
                }
                else if (TableNameClassForUniqueness.Region.Contains(field.ToLower()))
                {
                    tableEnum = TableNameCheckUniqueness.Region;
                }
                else if (TableNameClassForUniqueness.City.Contains(field.ToLower()))
                {
                    tableEnum = TableNameCheckUniqueness.City;
                }
                if (tableEnum == null)
                {
                    throw new ArgumentException($"Field - {field} does not match any known column.");
                }
                // User Profile Creation
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TableId", SqlDbType.Int) { Value = (int)tableEnum },
                    new SqlParameter("@Field", SqlDbType.VarChar, 50) { Value = field },
                    new SqlParameter("@Value", SqlDbType.VarChar, 100) { Value = value }
                };

                // Execute the command
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_CheckUniqueness, CommandType.StoredProcedure, sqlParameters);
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
                throw new Exception($"Error while checking existing {field} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while checking existing {field}.", ex);
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
                    new SqlParameter("@Type", SqlDbType.VarChar, 10){Value = deleteType},
                };

                bool isFailure = SqlDBHelper.ExecuteNonQuery(SP_DeleteEntityRecord, CommandType.StoredProcedure, sqlParameters);
                if (isFailure)
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
        public async Task<string> UpdateSettingsDataAsync(ProfileUser userSettings, int CurrentUserId)
        {
            ProfileUser profileUserExisting = new ProfileUser();
            try
            {
                int NumberOfRowsAffected = -1;
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetSettingsData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> tables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_UpdateUserSettings, CommandType.StoredProcedure, sqlParameters);
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

                bool isFailure = SqlDBHelper.ExecuteNonQuery(SP_UpdateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                int RowsAffected = (sqlParameters[3].Value != DBNull.Value) ? (int)sqlParameters[3].Value : -1;

                if (RowsAffected <= 0 && isFailure)
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

                bool isFailure = SqlDBHelper.ExecuteNonQuery(SP_CreateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                newId = (sqlParameters[2].Value != DBNull.Value) ? (int)sqlParameters[2].Value : -1;
                // Validate if the newId is greater than 0
                if (newId <= 0 && isFailure)
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

                bool isFailure = SqlDBHelper.ExecuteNonQuery(SP_UpdateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                int RowsAffected = (sqlParameters[4].Value != DBNull.Value) ? (int)sqlParameters[4].Value : -1;

                if (RowsAffected <= 0 && isFailure)
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

                bool isFailure = SqlDBHelper.ExecuteNonQuery(SP_CreateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                newId = (sqlParameters[3].Value != DBNull.Value) ? (int)sqlParameters[3].Value : -1;
                // Validate if the newId is greater than 0
                if (newId <= 0 && isFailure)
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

                bool isFailure = SqlDBHelper.ExecuteNonQuery(SP_UpdateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                int RowsAffected = (sqlParameters[10].Value != DBNull.Value) ? (int)sqlParameters[10].Value : -1;

                if (RowsAffected <= 0 && isFailure)
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

                bool isFailure = SqlDBHelper.ExecuteNonQuery(SP_CreateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                newId = (sqlParameters[9].Value != DBNull.Value) ? (int)sqlParameters[9].Value : -1;
                // Validate if the newId is greater than 0
                if (newId <= 0 && isFailure)
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

                bool isFailure = SqlDBHelper.ExecuteNonQuery(SP_UpdateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                int RowsAffected = (sqlParameters[9].Value != DBNull.Value) ? (int)sqlParameters[9].Value : -1;

                if (RowsAffected <= 0 && isFailure)
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

                bool isFailure = SqlDBHelper.ExecuteNonQuery(SP_CreateEntityRecord, CommandType.StoredProcedure, sqlParameters);

                newId = (sqlParameters[3].Value != DBNull.Value) ? (int)sqlParameters[3].Value : -1;
                // Validate if the newId is greater than 0
                if (newId <= 0 && isFailure)
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

        public async Task<ProfileUserAPIVM> GetUserRecordAsync(int newUserId)
        {
            ProfileUserAPIVM userSettings = new ProfileUserAPIVM();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetCurrentUserDetails },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetCurrentUserProfile },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetCurrentUserPages },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetCurrentUserCategories },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetSettingsData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = CurrentUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetProfileData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetCategoryData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetRegionData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetCityData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetLocationData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                    new SqlParameter("@TextCriteria", SqlDbType.Int) { Value = FetchRecordByIdEnum.GetContactData },
                    new SqlParameter("@InputInt", SqlDbType.Int) { Value = newUserId },
                    new SqlParameter("@RowsAffected", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_uspFetchRecordById, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                                Description = dataRow["Description"].ToString(),
                                siteType = new SiteType
                                {
                                    SiteTypeID = Convert.ToInt32(dataRow["SiteTypeID"]),
                                    Description = dataRow["Description"].ToString(),
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
                List<DataTable> dataTables = SqlDBHelper.ExecuteParameterizedNonQuery(SP_GetTableAllData, CommandType.StoredProcedure, sqlParameters);
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
    }
}
