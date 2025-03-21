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
-- Description: Create Locations & Categories & Regions & Contacts
-- =============================================
ALTER PROCEDURE [dbo].[uspCreateEntityRecord]
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
	@NewID INT OUTPUT 
AS
BEGIN

	SET NOCOUNT ON;

	BEGIN TRY
		-- Start the transaction
		BEGIN TRANSACTION;

		-- For Region Insertion
		IF @Type = 'Region'
		BEGIN
			-- Insert the new Region and get the new ID
			INSERT INTO Region (RegionName)
				VALUES (@NewInput1);
			-- Retrieve the newly inserted ID
            SET @NewID = SCOPE_IDENTITY();
		END

		-- For City Insertion
		ELSE IF @Type = 'City'
		BEGIN
			-- Insert the new City and get the new ID
			INSERT INTO City (RegionID, CityName)
				VALUES (@NewIntInput1,@NewInput1);
			-- Retrieve the newly inserted ID
            SET @NewID = SCOPE_IDENTITY();
		END

		-- For Location Insertion
		ELSE IF @Type = 'Location'
		BEGIN
			-- Insert the new City and get the new ID
			INSERT INTO Location ([Location], StreetName, CityID, RegionID, DistrictName, BranchName, SponsorId)
				VALUES (@NewInput1,@NewInput2,@NewIntInput1,@NewIntInput2,@NewInput3,@NewInput4, @NewIntInput3);
			-- Retrieve the newly inserted ID
            SET @NewID = SCOPE_IDENTITY();
		END

		-- For Contact Insertion
		ELSE IF @Type = 'Contact'
		BEGIN
			-- Insert the new City and get the new ID
			INSERT INTO Contact ([NAME], Designation, OfficePhone, Mobile, EmailID, Fax, BranchName, SponsorId)
				VALUES (@NewInput1,@NewInput2,@NewInput3,@NewInput4,@NewInput5,@NewInput6, @NewInput7, @NewIntInput1);
			-- Retrieve the newly inserted ID
            SET @NewID = SCOPE_IDENTITY();
		END
		
		-- For Profile Insertion
		ELSE IF @Type = 'Profile'
		BEGIN
            -- Insert the new Profile and get the new ID
            INSERT INTO Profiles (ProfileName)
                VALUES (@NewInput1);
            SET @NewID = SCOPE_IDENTITY();

            -- Execute the Profile Permissions procedure 
            EXEC uspAddProfilePermission 
                @NewProfileId = @NewID,
                @AllPageIds = @NewInput2, -- Comma-separated list of Page IDs
                @NewCreateUserId = @NewIntInput1,
                @NewUpdateUserId = @NewIntInput2;
        END

		-- Invalid Type
		ELSE
		BEGIN
			IF @@trancount > 0
				ROLLBACK TRANSACTION;
			SELECT
				-1 AS RowsAffected; -- Indicate invalid type
			RETURN; -- Exit the stored procedure
		END

		-- If everything is successful, commit the transaction
		COMMIT TRANSACTION;

	END TRY

	BEGIN CATCH
		-- If an error occurs, rollback the transaction
		IF @@trancount > 0
			ROLLBACK TRANSACTION;

		-- Handle exceptions
		SELECT
			ERROR_NUMBER() AS ErrorNumber
		   ,ERROR_MESSAGE() AS ErrorMessage
		   ,ERROR_SEVERITY() AS ErrorSeverity
		   ,ERROR_STATE() AS ErrorState
		   ,ERROR_LINE() AS ErrorLine
		   ,ERROR_PROCEDURE() AS ErrorProcedure;

	END CATCH;

END
