USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspUpdateSiteDetails]    Script Date: 29-10-2024 16:02:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Update site details along with related tables data with SiteId
-- =============================================
ALTER PROCEDURE [dbo].[uspUpdateSiteDetails]
-- Parameters for SiteDetails table
	@SiteID BIGINT,  -- Existing SiteID for update
    @SiteCode VARCHAR(50),
    @SiteName VARCHAR(50),
    @SiteCategory VARCHAR(50) = NULL,
    @SponsorID INT = NULL,
    @RegionID INT = NULL,
    @CityID INT = NULL,
    @LocationID INT = NULL,
    @ContactID INT = NULL,
    @SiteTypeID INT = NULL,
    @GPSLong VARCHAR(30) = NULL,
    @GPSLatt VARCHAR(30) = NULL,
    @VisitUserID INT = NULL,
    @VisitedDate DATETIME = NULL,
    @ApprovedUserID INT = NULL,
    @ApprovalDate DATETIME = NULL,
    @VisitStatusID INT = NULL,
    @IsActive BIT = NULL,
    @BranchNo VARCHAR(50) = NULL,
    @BranchTypeId INT = NULL,
    @AtmClass CHAR(1) = NULL,
    
    -- Parameters for other related tables (use NULL for optional fields)
    -- GeographicalDetails
    @NearestLandmark VARCHAR(50) = NULL,
    @NumberofKmNearestCity VARCHAR(50) = NULL,
    @BranchConstructionType VARCHAR(50) = NULL,
    @BranchIsLocatedAt VARCHAR(50) = NULL,
    @HowToReachThere VARCHAR(50) = NULL,
    @SiteisonServiceRoad BIT = NULL,
    @Howtogetthere VARCHAR(50) = NULL,

    -- SiteBranchFacilities
    @Parking BIT = NULL,
    @Landscape BIT = NULL,
    @Elevator BIT = NULL,
    @VIPSection BIT = NULL,
    @SafeBox BIT = NULL,
    @ICAP BIT = NULL,

    -- SiteContactInformation
    @BranchTelephoneNumber VARCHAR(20) = NULL,
    @BranchFaxNumber VARCHAR(20) = NULL,
    @EmailAddress TEXT = NULL,

    -- SiteDataCenter
    @UPSBrand VARCHAR(50) = NULL,
    @UPSCapacity VARCHAR(50) = NULL,
    @PABXBrand VARCHAR(50) = NULL,
    @StabilizerBrand VARCHAR(50) = NULL,
    @StabilizerCapacity VARCHAR(50) = NULL,
    @SecurityAccessSystemBrand VARCHAR(50) = NULL,

	-- SignBoardType
	@Cylinder BIT = NULL,
    @StraightOrTotem BIT = NULL,

    -- SiteMiscInformation
    @TypeofATMLocation VARCHAR(50) = NULL,
    @NoofExternalCameras INT = NULL,
    @NoofInternalCameras INT = NULL,
    @TrackingSystem VARCHAR(50) = NULL,

	-- BranchMiscInformation
    @Noofcleaners INT = NULL,
    @Frequencyofdailymailingservice INT = NULL,
    @ElectricSupply VARCHAR(50) = NULL,
    @WaterSupply VARCHAR(50) = NULL,
    @BranchOpenDate DATE = NULL,
    @TellersCounter INT = NULL,
    @NoofSalesmanageroffices INT = NULL,
    @ExistVIPsection BIT = NULL,
    @ContractStartDate DATE = NULL,
    @NoofRenovationRetouchtime INT = NULL,
    @LeasedOwbuilding BIT = NULL,
    @Noofteaboys INT = NULL,
    @Frequencyofmonthlycleaningservice INT = NULL,
    @DrainSewerage VARCHAR(50) = NULL,
    @CentralAC BIT = NULL,
    @SplitAC BIT = NULL,
    @WindowAC BIT = NULL,
    @Cashcountertype INT = NULL,
    @NoofTellerCounters INT = NULL,
    @Noofaffluentrelationshipmanageroffices INT = NULL,
    @SeperateVIPsection BIT = NULL,
    @ContractEndDate DATE = NULL,
    @RenovationRetouchDate DATE = NULL,
    @NoofTCRmachines INT = NULL,
    @NoOfTotem INT = NULL,
	@RowsAffected INT OUTPUT 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		-- Start the transaction
		BEGIN TRANSACTION;

		-- Update SiteDetails table
        UPDATE [dbo].[SiteDetails]
        SET SiteCode = @SiteCode,
            SiteName = @SiteName,
            SiteCategory = @SiteCategory,
            SponsorID = @SponsorID,
            RegionID = @RegionID,
            CityID = @CityID,
            LocationID = @LocationID,
            ContactID = @ContactID,
            SiteTypeID = @SiteTypeID,
            GPSLong = @GPSLong,
            GPSLatt = @GPSLatt,
            VisitUserID = @VisitUserID,
            VisitedDate = @VisitedDate,
            ApprovedUserID = @ApprovedUserID,
            ApprovalDate = @ApprovalDate,
            VisitStatusID = @VisitStatusID,
            IsActive = @IsActive,
            BranchNo = @BranchNo,
            BranchTypeId = @BranchTypeId,
            AtmClass = @AtmClass
        WHERE SiteID = @SiteID;

        -- Update GeographicalDetails table
        UPDATE [dbo].[GeographicalDetails]
        SET NearestLandmark = @NearestLandmark,
            NumberofKmNearestCity = @NumberofKmNearestCity,
            BranchConstructionType = @BranchConstructionType,
            BranchIsLocatedAt = @BranchIsLocatedAt,
            HowToReachThere = @HowToReachThere,
            SiteisonServiceRoad = @SiteisonServiceRoad,
            Howtogetthere = @Howtogetthere
        WHERE SiteID = @SiteID;

        -- Update SiteBranchFacilities table
        UPDATE [dbo].[SiteBranchFacilities]
        SET Parking = @Parking,
            Landscape = @Landscape,
            Elevator = @Elevator,
            VIPSection = @VIPSection,
            SafeBox = @SafeBox,
            ICAP = @ICAP
        WHERE SiteID = @SiteID;

        -- Update SiteContactInformation table
        UPDATE [dbo].[SiteContactInformation]
        SET BranchTelephoneNumber = @BranchTelephoneNumber,
            BranchFaxNumber = @BranchFaxNumber,
            EmailAddress = @EmailAddress
        WHERE SiteID = @SiteID;

        -- Update SiteDataCenter table
        UPDATE [dbo].[SiteDataCenter]
        SET UPSBrand = @UPSBrand,
            UPSCapacity = @UPSCapacity,
            PABXBrand = @PABXBrand,
            StabilizerBrand = @StabilizerBrand,
            StabilizerCapacity = @StabilizerCapacity,
            SecurityAccessSystemBrand = @SecurityAccessSystemBrand
        WHERE SiteID = @SiteID;

        -- Update SignBoardType table
        UPDATE [dbo].[SignBoardType]
        SET Cylinder = @Cylinder,
            StraightOrTotem = @StraightOrTotem
        WHERE SiteID = @SiteID;

        -- Update SiteMiscInformation table
        UPDATE [dbo].[SiteMiscInformation]
        SET TypeofATMLocation = @TypeofATMLocation,
            NoofExternalCameras = @NoofExternalCameras,
            NoofInternalCameras = @NoofInternalCameras,
            TrackingSystem = @TrackingSystem
        WHERE SiteID = @SiteID;

        -- Update BranchMiscInformation table
        UPDATE [dbo].[BranchMiscInformation]
        SET Noofcleaners = @Noofcleaners,
            Frequencyofdailymailingservice = @Frequencyofdailymailingservice,
            ElectricSupply = @ElectricSupply,
            WaterSupply = @WaterSupply,
            BranchOpenDate = @BranchOpenDate,
            TellersCounter = @TellersCounter,
            NoofSalesmanageroffices = @NoofSalesmanageroffices,
            ExistVIPsection = @ExistVIPsection,
            ContractStartDate = @ContractStartDate,
            NoofRenovationRetouchtime = @NoofRenovationRetouchtime,
            LeasedOwbuilding = @LeasedOwbuilding,
            Noofteaboys = @Noofteaboys,
            Frequencyofmonthlycleaningservice = @Frequencyofmonthlycleaningservice,
            DrainSewerage = @DrainSewerage,
            CentralAC = @CentralAC,
            SplitAC = @SplitAC,
            WindowAC = @WindowAC,
            Cashcountertype = @Cashcountertype,
            NoofTellerCounters = @NoofTellerCounters,
            Noofaffluentrelationshipmanageroffices = @Noofaffluentrelationshipmanageroffices,
            SeperateVIPsection = @SeperateVIPsection,
            ContractEndDate = @ContractEndDate,
            RenovationRetouchDate = @RenovationRetouchDate,
            NoofTCRmachines = @NoofTCRmachines,
            NoOfTotem = @NoOfTotem
        WHERE SiteID = @SiteID;

        -- Commit the transaction
        COMMIT TRANSACTION;

		SET @RowsAffected = 1;

	END TRY

	BEGIN CATCH
		-- If an error occurs, rollback the transaction
		IF @@trancount > 0
			ROLLBACK TRANSACTION;

		-- Return the error details
		SELECT
			ERROR_NUMBER() AS ErrorNumber
		   ,ERROR_MESSAGE() AS ErrorMessage
		   ,ERROR_SEVERITY() AS ErrorSeverity
		   ,ERROR_STATE() AS ErrorState
		   ,ERROR_LINE() AS ErrorLine
		   ,ERROR_PROCEDURE() AS ErrorProcedure;

		   -- Return -1 to indicate failure
		SET @RowsAffected = -1;
        RETURN;

	END CATCH;

END
