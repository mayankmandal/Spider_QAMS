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
-- Description: Create Profiles & Categories & Regions
-- =============================================
ALTER PROCEDURE [dbo].[uspCreateEntityRecord]
-- Add the parameters for the stored procedure here
	@NewInput NVARCHAR(200),
	@NewIntInput INT = NULL,
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
				VALUES (@NewInput);
			-- Retrieve the newly inserted ID
            SET @NewID = SCOPE_IDENTITY();
		END

		-- For City Insertion
		IF @Type = 'City'
		BEGIN
			-- Insert the new City and get the new ID
			INSERT INTO City (RegionID, CityName)
				VALUES (@NewIntInput,@NewInput);
			-- Retrieve the newly inserted ID
            SET @NewID = SCOPE_IDENTITY();
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
