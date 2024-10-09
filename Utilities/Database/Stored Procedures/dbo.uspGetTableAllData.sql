USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspGetTableAllData]    Script Date: 12-09-2024 15:31:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: For Fetching All Data for a Table
-- =============================================
ALTER PROCEDURE [dbo].[uspGetTableAllData]
    -- Add the parameters for the stored procedure here
	@TextCriteria INT = 0,
	@RowsAffected INT OUTPUT 
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

		-- Validate the value of @TextCriteria currently 1 to 15 only
        IF @TextCriteria NOT BETWEEN 1 AND 15
		BEGIN
				
			SELECT -1 AS RowsAffected;
			RETURN;

		END
		-- GetAllUsersData
		IF @TextCriteria = 1
		BEGIN
			SELECT p.ProfileID, p.ProfileName, u.UserId, u.Designation, u.FullName, u.EmailID, u.PhoneNumber, u.Username, u.ProfilePicName, u.IsActive, U.IsADUser, U.IsDeleted FROM Users u WITH (NOLOCK) 
			INNER JOIN UserProfile up WITH (NOLOCK) on up.UserId = u.UserId 
			INNER JOIN Profiles p WITH (NOLOCK) on p.ProfileID = up.ProfileID AND u.IsDeleted != 1
		END

		-- GetAllProfiles
		ELSE IF @TextCriteria = 2
		BEGIN
			SELECT ProfileId, ProfileName FROM Profiles p WITH (NOLOCK)
		END

		-- GetAllPages
		ELSE IF @TextCriteria = 3
		BEGIN
			SELECT PageId, PageUrl, PageDesc, PageSeq, PageCatId, PageImgUrl, PageName FROM tblPage WITH (NOLOCK)
		END

		-- GetAllCategories
		ELSE IF @TextCriteria = 4
		BEGIN
			SELECT PageCatId,CategoryName FROM tblPageCategory WITH (NOLOCK)
		END

		-- GetAllRegions
		ELSE IF @TextCriteria = 5
		BEGIN
			SELECT RegionID, RegionName FROM Region WITH (NOLOCK)
		END

		-- GetAllCities
		ELSE IF @TextCriteria = 6
		BEGIN
			SELECT c.CityID, c.CityName, r.RegionID, r.RegionName FROM City c WITH (NOLOCK)
			INNER JOIN Region r WITH (NOLOCK) ON c.RegionID = r.RegionID
		END

		-- GetAllLocations
		ELSE IF @TextCriteria = 7
		BEGIN
			SELECT L.*, c.CityName, r.RegionName, s.SponsorId, s.SponsorName FROM [Location] l  WITH (NOLOCK)
			INNER JOIN City c WITH (NOLOCK) ON c.CityID = l.CityID
			INNER JOIN Region r WITH (NOLOCK) ON c.RegionID = r.RegionID
			INNER JOIN Sponsor s WITH (NOLOCK) ON l.SponsorId = s.SponsorId
		END

		-- GetRegionListOfCities
		ELSE IF @TextCriteria = 8
		BEGIN
			SELECT c.CityID, c.CityName, r.RegionID, r.RegionName FROM City c WITH (NOLOCK) 
			INNER JOIN Region r WITH (NOLOCK) ON c.RegionID = r.RegionID
		END

		-- GetAllSponsors
		ELSE IF @TextCriteria = 9
		BEGIN
			SELECT s.SponsorId, s.SponsorName FROM Sponsor s WITH (NOLOCK)
		END

		-- GetAllContacts
		ELSE IF @TextCriteria = 10
		BEGIN
			SELECT c.ContactID, c.[Name], c.Designation, c.OfficePhone, c.Mobile, c.EmailID, c.Fax, c.BranchName, c.SponsorId, s.SponsorName FROM Contact c WITH (NOLOCK)
			INNER JOIN Sponsor s WITH (NOLOCK) ON c.SponsorId = s.SponsorId
		END

		-- GetAllSiteTypes
		ELSE IF @TextCriteria = 11
		BEGIN
			SELECT st.SiteTypeID, st.[Description], st.SponsorID, s.SponsorName FROM SiteTypes st WITH (NOLOCK)
			INNER JOIN Sponsor s WITH (NOLOCK) ON st.SponsorId = s.SponsorId
		END

		-- GetAllBranchTypes
		ELSE IF @TextCriteria = 12
		BEGIN
			SELECT bt.BranchTypeId, bt.[Description] AS BranchDescription, bt.SiteTypeID, st.[Description] AS SiteDescription, st.SponsorID, s.SponsorName FROM BranchType bt WITH (NOLOCK)
			INNER JOIN SiteTypes st WITH (NOLOCK) ON bt.SiteTypeID = st.SiteTypeID
			INNER JOIN Sponsor s WITH (NOLOCK) ON st.SponsorID = s.SponsorId
		END

		-- GetAllVisitStatuses
		ELSE IF @TextCriteria = 13
		BEGIN
			SELECT vs.VisitStatusID, vs.VisitStatus FROM VisitStatus vs WITH (NOLOCK)
		END

		-- GetAllATMClasses
		ELSE IF @TextCriteria = 14
		BEGIN
			SELECT a.Class FROM ATMClass a WITH (NOLOCK)
		END

		-- GetAllPicCategories
		ELSE IF @TextCriteria = 15
		BEGIN
			SELECT spc.PicCatID, spc.[Description] FROM SitePicCategory spc WITH (NOLOCK)
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
