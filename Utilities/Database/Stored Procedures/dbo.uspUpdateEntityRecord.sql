USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspUpdateEntityRecord]    Script Date: 29-05-2024 16:02:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Update Profiles & Categories & Regions & Contacts
-- =============================================
ALTER PROCEDURE [dbo].[uspUpdateEntityRecord]
    -- Add the parameters for the stored procedure here
	@NewInput1 NVARCHAR(MAX) = NULL,
	@NewInput2 NVARCHAR(MAX) = NULL,
	@NewInput3 NVARCHAR(200) = NULL,
	@NewInput4 NVARCHAR(200) = NULL,
	@NewInput5 NVARCHAR(200) = NULL,
	@NewInput6 NVARCHAR(200) = NULL,
	@NewInput7 NVARCHAR(200) = NULL,
	@NewIntInput1 INT = NULL,
	@NewIntInput2 INT = NULL,
	@NewIntInput3 INT = NULL,
	@NewIntInput4 INT = NULL,
	@Type VARCHAR(10),
	@RowsAffected INT OUTPUT  -- New Output Parameter
AS
BEGIN
    
    SET NOCOUNT ON;

    BEGIN TRY
		-- Start the transaction
        BEGIN TRANSACTION;

		-- Initialize RowsAffected to 0
        SET @RowsAffected = 0;

		-- Region Updation
        IF @Type = 'Region'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM Region r WITH (NOLOCK) WHERE r.RegionID = @NewIntInput1)
            BEGIN

                UPDATE Region SET RegionName = @NewInput1
				WHERE RegionID = @NewIntInput1

				-- Capture rows affected
                SET @RowsAffected = @@ROWCOUNT;
            END
        END

		-- City Updation
        ELSE IF @Type = 'City'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM City c WITH (NOLOCK) WHERE c.CityID = @NewIntInput1)
            BEGIN

                UPDATE City SET CityName = @NewInput1, RegionID = @NewIntInput2
				WHERE CityID = @NewIntInput1

				-- Capture rows affected
                SET @RowsAffected = @@ROWCOUNT;
            END
        END

		-- Location Updation
        ELSE IF @Type = 'Location'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM [LOCATION] l WITH (NOLOCK) WHERE l.LocationID = @NewIntInput1)
            BEGIN

                UPDATE Location SET [Location] = @NewInput1, StreetName = @NewInput2, CityID = @NewIntInput2, RegionID = @NewIntInput3, DistrictName = @NewInput3, BranchName = @NewInput4, SponsorId = @NewIntInput4
				WHERE LocationID = @NewIntInput1

				-- Capture rows affected
                SET @RowsAffected = @@ROWCOUNT;
            END
        END

		-- Contact Updation
        ELSE IF @Type = 'Contact'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM Contact c WITH (NOLOCK) WHERE c.ContactID = @NewIntInput1)
            BEGIN

                UPDATE Contact SET [NAME] = @NewInput1, Designation = @NewInput2, OfficePhone = @NewInput3, Mobile = @NewInput4, EmailID = @NewInput5, Fax = @NewInput6, BranchName = @NewInput7, SponsorId = @NewIntInput2
				WHERE ContactID = @NewIntInput1

				-- Capture rows affected
                SET @RowsAffected = @@ROWCOUNT;
            END
        END

		-- Profile Updation
        ELSE IF @Type = 'Profile'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM Profiles p WITH (NOLOCK) WHERE P.ProfileID = @NewIntInput1)
            BEGIN

				-- Profile Data Update
                UPDATE Profiles SET ProfileName = @NewInput1 -- New ProfileName
				WHERE ProfileID = @NewIntInput1 -- ProfileId

				-- Capture rows affected
                SET @RowsAffected = @@ROWCOUNT;

				-- Delete Existing Records of Profile with Pages
				EXEC [uspDeleteProfilePermission] @ProfileId = @NewIntInput1 -- ProfileId

				-- Add New Records of Profile with Pages
				EXEC uspAddProfilePermission 
                @NewProfileId = @NewIntInput1, -- ProfileId
                @AllPageIds = @NewInput2, -- New Comma-separated list of Page IDs
                @NewCreateUserId = @NewIntInput2, -- UserId
                @NewUpdateUserId = @NewIntInput3; -- UserId

            END
        END

        -- Invalid Type
        ELSE
        BEGIN
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;
            SELECT -1 AS RowsAffected; -- Indicate invalid type
            RETURN; -- Exit the stored procedure
        END

		-- If everything is successful, commit the transaction
        COMMIT TRANSACTION;

		-- If no rows were updated, set RowsAffected to 0
        IF @RowsAffected = 0
            SET @RowsAffected = 0;

    END TRY

    BEGIN CATCH
		-- If an error occurs, rollback the transaction
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Handle exceptions
        SELECT
            ERROR_NUMBER() AS ErrorNumber,
            ERROR_MESSAGE() AS ErrorMessage,
            ERROR_SEVERITY() AS ErrorSeverity,
            ERROR_STATE() AS ErrorState,
            ERROR_LINE() AS ErrorLine,
            ERROR_PROCEDURE() AS ErrorProcedure;

    END CATCH;

END
