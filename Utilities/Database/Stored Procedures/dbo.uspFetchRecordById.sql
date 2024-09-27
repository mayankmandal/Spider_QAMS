USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspFetchRecordById]    Script Date: 12-09-2024 15:31:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: For Fetching Data by using Integer Type Id
-- =============================================
ALTER PROCEDURE [dbo].[uspFetchRecordById]
    -- Add the parameters for the stored procedure here
	@TextCriteria INT = 0,
    @InputInt INT = NULL,
	@RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

		-- Validate the value of @TextCriteria currently 1 to 10 only
        IF @TextCriteria NOT BETWEEN 1 AND 11
		BEGIN
				
			SELECT -1 AS RowsAffected;
			RETURN;

		END
		-- GetCurrentUserDetails
		IF @TextCriteria = 1
		BEGIN
			select u.*, u.ProfileId,tp.ProfileName from Users u WITH (NOLOCK) 
			LEFT JOIN UserProfile tup WITH (NOLOCK) on tup.UserId = u.UserId 
			LEFT JOIN Profiles tp WITH (NOLOCK) on tp.ProfileID = tup.ProfileID WHERE U.UserId = @InputInt
		END

		-- GetCurrentUserProfile
		ELSE IF @TextCriteria = 2
		BEGIN
			SELECT tp.ProfileID, tp.ProfileName, tp.CreateDate, tp.UpdateDate, tp.CreateUserId, tp.UpdateUserId FROM Profiles tp WITH (NOLOCK)
			INNER JOIN UserProfile tbup WITH (NOLOCK) on tp.ProfileID = tbup.ProfileID WHERE tbup.UserId = @InputInt;
		END

		-- GetCurrentUserPages
		ELSE IF @TextCriteria = 3
		BEGIN
			SELECT ProfileId, PageId, PageUrl, PageDesc, UserId FROM vwUserPageAccess WITH (NOLOCK) where UserId = @InputInt;
		END

		-- GetCurrentUserCategories
		ELSE IF @TextCriteria = 4
		BEGIN
			SELECT ProfileId, PageId, PageUrl, PageDesc, PageCatId, CategoryName, UserId FROM vwUserPagesData WITH (NOLOCK) where UserId = @InputInt;
		END

		 -- GetSettingsData
		ELSE IF @TextCriteria = 5
		BEGIN
			SELECT u.UserId, u.FullName, u.UserName, u.PasswordHash, u.PasswordSalt, u.EmailID, u.PhoneNumber, u.ProfilePicName, u.ProfileId, u.Designation, u.Location, u.IsADUser, u.IsActive, u.CreateDate, u.CreateUserId, u.UpdateDate, u.UpdateUserId, u.IsDeleted from Users u WITH (NOLOCK) where u.UserId = @InputInt;
		END

		-- GetProfileData
		ELSE IF @TextCriteria = 6
		BEGIN
			SELECT p.ProfileID, p.ProfileName from Profiles p WITH (NOLOCK) where P.ProfileID = @InputInt;
		END

		-- GetCategoryData
		ELSE IF @TextCriteria = 7
		BEGIN
			SELECT pc.PageCatId, pc.CategoryName from tblPageCategory pc WITH (NOLOCK) where pc.PageCatId = @InputInt;
		END

		-- GetRegionData
		ELSE IF @TextCriteria = 8
		BEGIN
			SELECT r.RegionID, r.RegionName from Region r WITH (NOLOCK) where r.RegionID = @InputInt;
		END

		-- GetCityData
		ELSE IF @TextCriteria = 9
		BEGIN
			SELECT c.CityID, c.CityName, c.RegionID, r.RegionName from City c WITH (NOLOCK)
			INNER JOIN Region r WITH (NOLOCK) ON c.RegionID = r.RegionID 
			where c.CityID = @InputInt;
		END
		
		-- GetLocationData
		ELSE IF @TextCriteria = 10
		BEGIN
			SELECT l.LocationID, l.Location, l.StreetName, l.DistrictName, l.BranchName, l.CityID, c.CityName, c.RegionID, r.RegionName, l.SponsorId, s.SponsorName from [LOCATION] l WITH (NOLOCK)
			INNER JOIN City c WITH (NOLOCK) ON l.CityID = c.CityID
			INNER JOIN Region r WITH (NOLOCK) ON c.RegionID = r.RegionID
			INNER JOIN Sponsor s WITH (NOLOCK) ON l.SponsorId = s.SponsorId
			where L.LocationID = @InputInt;
		END

		-- GetLocationData
		ELSE IF @TextCriteria = 11
		BEGIN
			SELECT c.ContactID, c.[Name], c.Designation, c.OfficePhone, c.Mobile, c.EmailID, c.Fax, c.BranchName, c.SponsorId, s.SponsorName FROM Contact c WITH (NOLOCK)
			INNER JOIN Sponsor s WITH (NOLOCK) ON c.SponsorId = s.SponsorId
			where c.ContactID = @InputInt;
		END

		-- Capture number of rows affected
        SET @RowsAffected = @@ROWCOUNT; -- Set output parameter

        -- Return the number of rows affected
        SELECT @RowsAffected AS RowsAffected;

    END TRY
    BEGIN CATCH

        -- Handle exceptions
        SELECT
            ERROR_NUMBER() AS ErrorNumber,
            ERROR_MESSAGE() AS ErrorMessage,
            ERROR_SEVERITY() AS ErrorSeverity,
            ERROR_STATE() AS ErrorState,
            ERROR_LINE() AS ErrorLine,
            ERROR_PROCEDURE() AS ErrorProcedure;

        -- Return an error code
        SELECT -1 AS RowsAffected;
    END CATCH;
END
