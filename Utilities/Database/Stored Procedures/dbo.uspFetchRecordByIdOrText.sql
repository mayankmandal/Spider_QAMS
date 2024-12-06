USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspFetchRecordByIdOrText]    Script Date: 12-09-2024 15:31:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: For Fetching Data by using Integer Type Id
-- =============================================
ALTER PROCEDURE [dbo].[uspFetchRecordByIdOrText]
    -- Add the parameters for the stored procedure here
	@TextCriteria INT = 0,
    @InputInt INT = NULL,
	@InputText NVARCHAR(100) = NULL,
	@RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
		
		DECLARE @NewSiteId BIGINT;

		-- Validate the value of @TextCriteria currently 1 to 15 only
        IF @TextCriteria NOT BETWEEN 1 AND 15
		BEGIN
				
			SELECT -1 AS RowsAffected;
			RETURN;

		END
		-- GetCurrentUserDetails
		IF @TextCriteria = 1
		BEGIN
			select u.*, u.ProfileId,tp.ProfileName from Users u WITH (NOLOCK) 
			LEFT JOIN UserProfile tup WITH (NOLOCK) on tup.UserId = u.UserId 
			LEFT JOIN Profiles tp WITH (NOLOCK) on tp.ProfileID = tup.ProfileID WHERE U.UserId = @InputInt AND u.IsDeleted != 1
		END

		-- GetCurrentUserProfile
		ELSE IF @TextCriteria = 2
		BEGIN
			SELECT tp.ProfileID, tp.ProfileName FROM Profiles tp WITH (NOLOCK)
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
			SELECT u.UserId, u.FullName, u.UserName, u.PasswordHash, u.PasswordSalt, u.EmailID, u.PhoneNumber, u.ProfilePicName, u.ProfileId, P.ProfileName, u.Designation, u.[Location], u.IsADUser, u.IsActive, u.CreateDate, u.CreateUserId, u.UpdateDate, u.UpdateUserId, u.IsDeleted from Users u WITH (NOLOCK)
			LEFT JOIN Profiles p WITH (NOLOCK) ON u.ProfileId = p.ProfileID
			where u.UserId = @InputInt;
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
			SELECT l.LocationID, l.[Location], l.StreetName, l.DistrictName, l.BranchName, l.CityID, c.CityName, c.RegionID, r.RegionName, l.SponsorId, s.SponsorName from [LOCATION] l WITH (NOLOCK)
			INNER JOIN City c WITH (NOLOCK) ON l.CityID = c.CityID
			INNER JOIN Region r WITH (NOLOCK) ON c.RegionID = r.RegionID
			INNER JOIN Sponsor s WITH (NOLOCK) ON l.SponsorId = s.SponsorId
			where L.LocationID = @InputInt;
		END

		-- GetContactData
		ELSE IF @TextCriteria = 11
		BEGIN
			SELECT c.ContactID, c.[Name], c.Designation, c.OfficePhone, c.Mobile, c.EmailID, c.Fax, c.BranchName, c.SponsorId, s.SponsorName FROM Contact c WITH (NOLOCK)
			INNER JOIN Sponsor s WITH (NOLOCK) ON c.SponsorId = s.SponsorId
			where c.ContactID = @InputInt;
		END

		-- GetSiteDetail
		ELSE IF @TextCriteria = 12
		BEGIN

			-- Retrieve the site details using the assigned SiteID
			SELECT 
				sd.SiteID, sd.SiteCode, sd.SiteName, sd.SiteCategory, 
				sd.SponsorID, spr.SponsorName, sd.RegionID, rgn.RegionName, 
				sd.CityID, cty.CityName, sd.LocationID, loc.[Location] AS LocationName, 
				sd.ContactID, ct.[Name] AS ContactName, sd.SiteTypeID, 
				stype.[Description] AS SiteTypeDescription, sd.GPSLong, 
				sd.GPSLatt, sd.VisitUserID, sd.VisitedDate, 
				sd.ApprovedUserID, sd.ApprovalDate, sd.VisitStatusID, 
				vst.VisitStatus, sd.IsActive, sd.BranchNo, 
				sd.BranchTypeId, bt.[Description] AS BranchTypeDescription, 
				sd.AtmClass,

				-- Branch Misc Information
				bmi.NoOfCleaners, bmi.FrequencyOfDailyMailingService, 
				bmi.ElectricSupply, bmi.WaterSupply, bmi.BranchOpenDate, 
				bmi.TellersCounter, bmi.NoOfSalesManagerOffices, 
				bmi.ExistVIPSection, bmi.ContractStartDate, 
				bmi.NoOfRenovationRetouchTime, bmi.LeasedOwBuilding, 
				bmi.NoOfTeaBoys, bmi.FrequencyOfMonthlyCleaningService, 
				bmi.DrainSewerage, bmi.CentralAC, bmi.SplitAC, 
				bmi.WindowAC, bmi.CashCounterType, bmi.NoOfTellerCounters, 
				bmi.NoOfAffluentRelationshipManagerOffices, bmi.SeperateVIPSection, 
				bmi.ContractEndDate, bmi.RenovationRetouchDate, 
				bmi.NoOfTCRMachines, bmi.NoOfTotem,

				-- Geographical Details
				gd.NearestLandmark, gd.NumberOfKmNearestCity, 
				gd.BranchConstructionType, gd.BranchIsLocatedAt, 
				gd.HowToReachThere, gd.SiteIsOnServiceRoad, gd.HowToGetThere,

				-- Site Branch Facilities
				sbf.Parking, sbf.Landscape, sbf.Elevator, 
				sbf.VIPSection, sbf.SafeBox, sbf.ICAP,

				-- Site Data Center
				sdc.UPSBrand, sdc.UPSCapacity, sdc.PABXBrand, 
				sdc.StabilizerBrand, sdc.StabilizerCapacity, 
				sdc.SecurityAccessSystemBrand,

				-- Sign Board Type
				sbt.Cylinder, sbt.StraightOrTotem,

				-- Site Contact Information
				sci.BranchTelephoneNumber, sci.BranchFaxNumber, sci.EmailAddress,

				-- Site Misc Information
				smi.TypeOfATMLocation, smi.NoOfExternalCameras, 
				smi.NoOfInternalCameras, smi.TrackingSystem
			FROM 
				SiteDetails sd WITH (NOLOCK)
				LEFT JOIN Sponsor spr WITH (NOLOCK) ON sd.SponsorID = spr.SponsorID
				LEFT JOIN Region rgn WITH (NOLOCK) ON sd.RegionID = rgn.RegionID
				LEFT JOIN City cty WITH (NOLOCK) ON sd.CityID = cty.CityID
				LEFT JOIN [LOCATION] loc WITH (NOLOCK) ON sd.LocationID = loc.LocationID
				LEFT JOIN Contact ct WITH (NOLOCK) ON sd.ContactID = ct.ContactID
				LEFT JOIN SiteTypes stype WITH (NOLOCK) ON sd.SiteTypeID = stype.SiteTypeID
				LEFT JOIN VisitStatus vst WITH (NOLOCK) ON sd.VisitStatusID = vst.VisitStatusID
				LEFT JOIN BranchType bt WITH (NOLOCK) ON sd.BranchTypeId = bt.BranchTypeId
				LEFT JOIN SiteContactInformation sci WITH (NOLOCK) ON sd.SiteID = sci.SiteID
				LEFT JOIN GeographicalDetails gd WITH (NOLOCK) ON sd.SiteID = gd.SiteID
				LEFT JOIN SiteBranchFacilities sbf WITH (NOLOCK) ON sd.SiteID = sbf.SiteID
				LEFT JOIN SiteDataCenter sdc WITH (NOLOCK) ON sd.SiteID = sdc.SiteID
				LEFT JOIN SignBoardType sbt WITH (NOLOCK) ON sd.SiteID = sbt.SiteID
				LEFT JOIN SiteMiscInformation smi WITH (NOLOCK) ON sd.SiteID = smi.SiteID
				LEFT JOIN BranchMiscInformation bmi WITH (NOLOCK) ON sd.SiteID = bmi.SiteID
			WHERE 
				sd.SiteID = @InputInt;

			-- Retrieve site pictures using the assigned SiteID
			SELECT 
				sp.SiteID, sp.SitePicID, sp.PicCatID, 
				spc.[Description] AS SitePicCategoryDescription, 
				sp.[Description] AS SitePicturesDescription, sp.PicPath
			FROM 
				SitePictures sp WITH (NOLOCK)
				LEFT JOIN SitePicCategory spc WITH (NOLOCK) ON sp.PicCatID = spc.PicCatID
			WHERE 
				sp.SiteID = @InputInt AND spc.[Description] = @InputText;
		END

		-- GetSitePictureBySiteId
		ELSE IF @TextCriteria = 13
		BEGIN
			-- Retrieve site pictures using the assigned SiteID
			SELECT 
				sp.SiteID, sp.SitePicID, sp.PicCatID, 
				spc.[Description] AS SitePicCategoryDescription, 
				sp.[Description] AS SitePicturesDescription, sp.PicPath
			FROM 
				SitePictures sp WITH (NOLOCK)
				LEFT JOIN SitePicCategory spc WITH (NOLOCK) ON sp.PicCatID = spc.PicCatID
			WHERE 
				sp.SiteID = @InputInt;
		END

		-- GetSitePictureBySitePicID
		ELSE IF @TextCriteria = 14
		BEGIN
			-- Retrieve site pictures using the assigned SiteID
			SELECT 
				sp.SiteID, sp.SitePicID, sp.PicCatID, 
				sp.[Description] AS SitePicturesDescription, sp.PicPath
			FROM 
				SitePictures sp WITH (NOLOCK)
			WHERE 
				sp.SitePicID = @InputInt;
		END

		-- GetProfilePagesData
		ELSE IF @TextCriteria = 15
		BEGIN
			-- Retrieve profile pages datat using ProfileId
			SELECT DISTINCT upa.ProfileId, p.ProfileName, upa.PageId, upa.PageUrl, upa.PageDesc FROM vwUserPageAccess upa WITH (NOLOCK) 
			INNER JOIN Profiles p WITH (NOLOCK) ON upa.ProfileId = p.ProfileID
			WHERE upa.ProfileId = @InputInt
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
