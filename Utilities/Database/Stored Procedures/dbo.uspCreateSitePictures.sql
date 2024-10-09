USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspCreateSitePictures]    Script Date: 29-05-2024 16:02:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Insert new site pictures for a site
-- =============================================
ALTER PROCEDURE [dbo].[uspCreateSitePictures]
-- Parameters for SiteDetails table
    @SiteID BIGINT,
    @PicCatID INT = NULL,
    @Description VARCHAR(MAX) = NULL,
    @PicPath VARCHAR(MAX),
	@NewSitePicID INT OUTPUT 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		-- Start the transaction
		BEGIN TRANSACTION;

		-- Insert into SiteDetails table
        INSERT INTO [dbo].[SitePictures](SiteID, PicCatID, [Description], PicPath)
        VALUES (@SiteID, @PicCatID, @Description, @PicPath);

        -- Get the newly inserted SiteID
        SET @NewSitePicID = SCOPE_IDENTITY();

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
