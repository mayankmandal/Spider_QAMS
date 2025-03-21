USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspCreateEntityRecord]    Script Date: 29-05-2024 16:02:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Insert new site details along with related tables data
-- =============================================
ALTER PROCEDURE [dbo].[uspCreateSiteDetails]
-- Parameters for SiteDetails table
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
	@NewSiteID BIGINT OUTPUT 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		-- Start the transaction
		BEGIN TRANSACTION;

		-- Insert into SiteDetails table
        INSERT INTO [dbo].[SiteDetails](SiteCode, SiteName, SiteCategory, SponsorID, RegionID, CityID, LocationID, ContactID, SiteTypeID, GPSLong, GPSLatt, VisitUserID, VisitedDate, ApprovedUserID, ApprovalDate, VisitStatusID, IsActive, BranchNo, BranchTypeId, AtmClass)
        VALUES (@SiteCode, @SiteName, @SiteCategory, @SponsorID, @RegionID, @CityID, @LocationID, @ContactID, @SiteTypeID, @GPSLong, @GPSLatt, @VisitUserID, @VisitedDate, @ApprovedUserID, @ApprovalDate, @VisitStatusID, @IsActive, @BranchNo, @BranchTypeId, @AtmClass);

        -- Get the newly inserted SiteID
        SET @NewSiteID = SCOPE_IDENTITY();

        -- Insert into GeographicalDetails table
        INSERT INTO [dbo].[GeographicalDetails](SiteID, NearestLandmark, NumberofKmNearestCity, BranchConstructionType, BranchIsLocatedAt, HowToReachThere, SiteisonServiceRoad, Howtogetthere)
        VALUES (@NewSiteID, @NearestLandmark, @NumberofKmNearestCity, @BranchConstructionType, @BranchIsLocatedAt, @HowToReachThere, @SiteisonServiceRoad, @Howtogetthere);

        -- Insert into BranchMiscInformation table
        INSERT INTO [dbo].[BranchMiscInformation](SiteID, Noofcleaners, Frequencyofdailymailingservice, ElectricSupply, WaterSupply, BranchOpenDate, TellersCounter, NoofSalesmanageroffices, ExistVIPsection, ContractStartDate, NoofRenovationRetouchtime, LeasedOwbuilding, Noofteaboys, Frequencyofmonthlycleaningservice, DrainSewerage, CentralAC, SplitAC, WindowAC, Cashcountertype, NoofTellerCounters, Noofaffluentrelationshipmanageroffices, SeperateVIPsection, ContractEndDate, RenovationRetouchDate, NoofTCRmachines, NoOfTotem)
        VALUES (@NewSiteID, @Noofcleaners, @Frequencyofdailymailingservice, @ElectricSupply, @WaterSupply, @BranchOpenDate, @TellersCounter, @NoofSalesmanageroffices, @ExistVIPsection, @ContractStartDate, @NoofRenovationRetouchtime, @LeasedOwbuilding, @Noofteaboys, @Frequencyofmonthlycleaningservice, @DrainSewerage, @CentralAC, @SplitAC, @WindowAC, @Cashcountertype, @NoofTellerCounters, @Noofaffluentrelationshipmanageroffices, @SeperateVIPsection, @ContractEndDate, @RenovationRetouchDate, @NoofTCRmachines, @NoOfTotem);

        -- Insert into SiteBranchFacilities table
        INSERT INTO [dbo].[SiteBranchFacilities](SiteID, Parking, Landscape, Elevator, VIPSection, SafeBox, ICAP)
        VALUES (@NewSiteID, @Parking, @Landscape, @Elevator, @VIPSection, @SafeBox, @ICAP);

        -- Insert into SiteContactInformation table
        INSERT INTO [dbo].[SiteContactInformation](SiteID, BranchTelephoneNumber, BranchFaxNumber, EmailAddress)
        VALUES (@NewSiteID, @BranchTelephoneNumber, @BranchFaxNumber, @EmailAddress);

        -- Insert into SiteDataCenter table
        INSERT INTO [dbo].[SiteDataCenter](SiteID, UPSBrand, UPSCapacity, PABXBrand, StabilizerBrand, StabilizerCapacity, SecurityAccessSystemBrand)
        VALUES (@NewSiteID, @UPSBrand, @UPSCapacity, @PABXBrand, @StabilizerBrand, @StabilizerCapacity, @SecurityAccessSystemBrand);

        -- Insert into SiteMiscInformation table
        INSERT INTO [dbo].[SiteMiscInformation](SiteID, TypeofATMLocation, NoofExternalCameras, NoofInternalCameras, TrackingSystem)
        VALUES (@NewSiteID, @TypeofATMLocation, @NoofExternalCameras, @NoofInternalCameras, @TrackingSystem);

		-- Insert into SignBoardType table
		INSERT INTO [dbo].[SignBoardType](SiteID, Cylinder, StraightOrTotem)
		VALUES (@NewSiteID, @Cylinder, @StraightOrTotem)

        -- Commit the transaction
        COMMIT TRANSACTION;

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

	END CATCH;

END
