USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspUpsertDeleteSitePictures]    Script Date: 29-05-2024 16:02:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Insert, update, or delete site pictures based on conditions
-- =============================================
ALTER PROCEDURE [dbo].[uspUpsertDeleteSitePictures]
-- Parameters for SiteDetails table
@SiteID BIGINT,
@PicCatID INT = NULL,
@SitePicID INT = NULL,
@Description VARCHAR(MAX) = NULL,
@PicPath VARCHAR(MAX) = NULL,
@IsDeleted BIT = 0,
@NewSitePicID INT OUTPUT,
@RowsAffected INT OUTPUT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		-- Start the transaction
		BEGIN TRANSACTION;

		IF @SitePicID IS NULL
		BEGIN
			-- Insert new record if SitePicId is not provided
			INSERT INTO [dbo].[SitePictures] (SiteID, PicCatID, [Description], PicPath)
				VALUES (@SiteID, @PicCatID, @Description, @PicPath);

			-- Capture the new SitePicID and RowsAffected
			SET @NewSitePicID = SCOPE_IDENTITY();
			SET @RowsAffected = @@rowcount;
		END
		ELSE
		BEGIN
			-- Check if the image is marked for deletion
			IF @IsDeleted = 1
			BEGIN
				-- Delete the record with specified SitePicID and SiteID
				DELETE FROM SitePictures
				WHERE SitePicID = @SitePicID
					AND SiteID = @SiteID;

				-- Capture RowsAffected
				SET @RowsAffected = 1;
				-- Return the existing SitePicID as output
				SET @NewSitePicID = @SitePicID;
			END
			ELSE
			BEGIN
				UPDATE SitePictures
				SET PicCatID = @PicCatID
				   ,[Description] = @Description
				   ,PicPath = @PicPath
				WHERE SitePicID = @SitePicID
				AND SiteID = @SiteID;

				-- Capture RowsAffected
				SET @RowsAffected = @@rowcount;

				-- Return the existing SitePicID as output
				SET @NewSitePicID = @SitePicID;
			END
		END

		-- Commit the transaction
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		-- Rollback the transaction in case of an error
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