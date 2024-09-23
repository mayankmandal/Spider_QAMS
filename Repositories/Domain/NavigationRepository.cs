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
        public async Task<List<ProfileUserAPIVM>> GetAllUsersDataAsync()
        {
            try
            {
                string commandText = "select p.ProfileID, p.ProfileName, u.UserId, u.Designation, u.FullName, u.EmailID, u.PhoneNumber, u.Username, u.ProfilePicName, u.IsActive, U.IsADUser from Users u INNER JOIN UserProfile up on up.UserId = u.UserId INNER JOIN Profiles p on p.ProfileID = up.ProfileID";

                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

                List<ProfileUserAPIVM> users = new List<ProfileUserAPIVM>();
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
                        };
                        users.Add(profileUser);
                    }
                }
                return users;
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
        }
        public async Task<List<ProfileSite>> GetAllProfilesAsync()
        {
            try
            {
                string commandText = "SELECT ProfileId, ProfileName, CreateDate, CreateUserId, UpdateDate, UpdateUserId FROM Profiles p";

                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

                List<ProfileSite> profiles = new List<ProfileSite>();
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        ProfileSite profile = new ProfileSite
                        {
                            ProfileId = (int)row["ProfileId"],
                            ProfileName = row["ProfileName"].ToString(),
                            CreateDate = (DateTime)row["CreateDate"],
                            CreateUserId = (int)row["CreateUserId"],
                            UpdateDate = (DateTime)row["UpdateDate"],
                            UpdateUserId = (int)row["UpdateUserId"],
                        };
                        profiles.Add(profile);
                    }
                }
                return profiles;
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
        }
        public async Task<List<PageSite>> GetAllPagesAsync()
        {
            try
            {
                string commandText = "SELECT PageId, PageUrl, PageDesc, PageSeq, PageCat, PageImgUrl, PageName FROM tblPage";

                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

                List<PageSite> pages = new List<PageSite>();
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
                return pages;
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
        }
        public async Task<List<PageCategory>> GetAllCategoriesAsync()
        {
            try
            {
                string commandText = "SELECT PageCatId,CategoryName, CreateDate, CreateUserId, UpdateDate, UpdateUserId FROM tblPageCatagory";

                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

                List<PageCategory> pageCategories = new List<PageCategory>();
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        PageCategory pageCategory = new PageCategory
                        {
                            PageCatId = (int)row["PageCatId"],
                            CategoryName = row["CategoryName"].ToString(),
                            PageId = 0,
                            CreateDate = (DateTime)row["CreateDate"],
                            CreateUserId = (int)row["CreateUserId"],
                            UpdateDate = (DateTime)row["UpdateDate"],
                            UpdateUserId = (int)row["UpdateUserId"],
                        };
                        pageCategories.Add(pageCategory);
                    }
                }
                return pageCategories;
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
        }
        public async Task<ProfileUserAPIVM> GetUserRecordAsync(string newUserId)
        {
            try
            {
                ProfileUserAPIVM userSettings = new ProfileUserAPIVM();
                string commandText = $"SELECT U.UserId, U.UserName, u.ProfilePicName, U.FullName, u.EmailID, U.Designation, U.PhoneNumber, U.ProfileId, p.ProfileName, u.IsActive, u.IsADUser from Users u LEFT JOIN UserProfile up on u.UserId = up.UserID LEFT JOIN Profiles p on up.ProfileID = u.ProfileId where U.UserId = {newUserId}";

                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

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
                        ProfileSiteData = ProfileData,
                        Password = string.Empty
                    };
                }
                return userSettings;
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
            try
            {
                int NumberOfRowsAffected = -1;

                ProfileUser profileUserExisting = null;
                string commandText = $"SELECT u.Designation, u.FullName, u.EmailID, u.PhoneNumber, u.UserName, u.ProfilePicName, u.IsActive, u.IsADUser, u.ProfileId, u.PasswordSalt, u.PasswordHash from Users u WHERE U.UserId = {userProfileData.UserId}";
                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

                if (dataTable.Rows.Count > 0)
                {
                    DataRow dataRow = dataTable.Rows[0];
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
                    };
                }

                // User Profile Updation
                SqlParameter[] sqlParameters = new SqlParameter[]
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
        public async Task<bool> CheckUniquenessAsync(string field, string value)
        {
            try
            {
                int isUnique = 0;
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

                }
                return isUnique > 0;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error while checking existing {field} - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while checking existing {field}.", ex);
            }
        }
        public async Task<ProfileUserAPIVM> GetCurrentUserDetailsAsync(int CurrentUserId)
        {
            try
            {
                string commandText = $"select u.*, u.ProfileId,tp.ProfileName from Users u LEFT JOIN UserProfile tup on tup.UserId = u.UserId LEFT JOIN Profiles tp on tp.ProfileID = tup.ProfileID WHERE U.UserId = {CurrentUserId}";

                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

                ProfileUserAPIVM profileUser = new ProfileUserAPIVM();
                if (dataTable.Rows.Count > 0)
                {
                    DataRow dataRow = dataTable.Rows[0];
                    profileUser = new ProfileUserAPIVM
                    {
                        UserId = dataRow["UserId"] != DBNull.Value ? Convert.ToInt32(dataRow["UserId"]) : 0,
                        Designation = dataRow["Designation"] != DBNull.Value ? dataRow["Designation"].ToString() : string.Empty,
                        FullName = dataRow["FullName"] != DBNull.Value ? dataRow["FullName"].ToString() : string.Empty,
                        EmailID = dataRow["EmailID"] != DBNull.Value ? dataRow["EmailID"].ToString() : string.Empty,
                        PhoneNumber = dataRow["PhoneNumber"] != DBNull.Value ? dataRow["PhoneNumber"].ToString() : string.Empty,
                        ProfileSiteData = new ProfileSite
                        {
                            ProfileId = dataRow["ProfileId"] != DBNull.Value ? Convert.ToInt32(dataRow["ProfileId"]) : 0,
                            ProfileName = dataRow["ProfileName"] != DBNull.Value ? dataRow["ProfileName"].ToString() : string.Empty
                        },
                        UserName = dataRow["UserName"] != DBNull.Value ? dataRow["UserName"].ToString() : string.Empty,
                        ProfilePicName = dataRow["ProfilePicName"] != DBNull.Value ? dataRow["ProfilePicName"].ToString() : string.Empty,
                        IsActive = dataRow["IsActive"] != DBNull.Value ? Convert.ToBoolean(dataRow["IsActive"]) : false,
                        IsADUser = dataRow["IsADUser"] != DBNull.Value ? Convert.ToBoolean(dataRow["IsADUser"]) : false,
                        CreateUserId = dataRow["CreateUserId"] != DBNull.Value ? Convert.ToInt32(dataRow["CreateUserId"]) : 0,
                        UpdateUserId = dataRow["UpdateUserId"] != DBNull.Value ? Convert.ToInt32(dataRow["UpdateUserId"]) : 0
                    };
                }
                return profileUser;
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL exceptions
                throw new Exception("Error executing SQL command.", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                throw new Exception("Error in Getting All Users Data.", ex);
            }
        }
        public async Task<ProfileSite> GetCurrentUserProfileAsync(int CurrentUserId)
        {
            try
            {
                string commandText = $"SELECT tp.ProfileID, tp.ProfileName, tp.CreateDate, tp.UpdateDate, tp.CreateUserId, tp.UpdateUserId FROM Profiles tp INNER JOIN UserProfile tbup on tp.ProfileID = tbup.ProfileID WHERE tbup.UserId = {CurrentUserId}";

                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

                ProfileSite profileSite = new ProfileSite();
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        profileSite = new ProfileSite
                        {
                            ProfileId = (int)row["ProfileId"],
                            ProfileName = row["ProfileName"].ToString(),
                            CreateDate = (DateTime)row["CreateDate"],
                            CreateUserId = (int)row["CreateUserId"],
                            UpdateDate = (DateTime)row["UpdateDate"],
                            UpdateUserId = (int)row["UpdateUserId"],
                        };
                    }
                }
                return profileSite;
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
        }
        public async Task<List<PageSiteVM>> GetCurrentUserPagesAsync(int CurrentUserId)
        {
            try
            {
                string commandText = $"SELECT * FROM vwUserPageAccess where UserId = {CurrentUserId}";

                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

                List<PageSiteVM> pages = new List<PageSiteVM>();
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
                return pages;
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
        }
        public async Task<List<CategoriesSetDTO>> GetCurrentUserCategoriesAsync(int CurrentUserId)
        {
            try
            {
                string commandText = $"SELECT * FROM vwUserPagesData where UserId = {CurrentUserId}";

                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

                List<CategoriesSetDTO> categoriesSet = new List<CategoriesSetDTO>();
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
                return categoriesSet;
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
        public async Task<ProfileUserAPIVM> GetSettingsDataAsync(int CurrentUserId)
        {
            try
            {
                ProfileUserAPIVM userSettings = new ProfileUserAPIVM();
                string commandText = $"SELECT UserId, u.UserName, u.ProfilePicName, u.FullName, u.EmailID, u.Designation, u.PhoneNumber from Users u where u.UserId = {CurrentUserId}";

                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);

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
                return userSettings;
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
        }
        public async Task<string> UpdateSettingsDataAsync(ProfileUser userSettings, int CurrentUserId)
        {
            try
            {
                int NumberOfRowsAffected = -1;
                ProfileUser profileUserExisting = new ProfileUser();
                string commandText = $"SELECT u.Username, u.FullName, u.EmailID, u.ProfilePicName, u.PasswordHash, u.PasswordSalt, u.UpdateUserId from Users u WHERE U.UserId = {userSettings.UserId}";
                DataTable dataTable = SqlDBHelper.ExecuteSelectCommand(commandText, CommandType.Text);
                if (dataTable.Rows.Count > 0)
                {
                    DataRow dataRow = dataTable.Rows[0];
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

                // User Profile Updation
                SqlParameter[] sqlParameters = new SqlParameter[]
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
                throw new Exception("Error while updating Settings - SQL Exception.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating Settings.", ex);
            }
        }
    }
}
