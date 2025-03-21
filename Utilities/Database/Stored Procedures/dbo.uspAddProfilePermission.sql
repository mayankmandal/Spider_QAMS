USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspAddProfilePermission]    Script Date: 05-12-2024 11:28:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Create User Permissions for Profile
-- =============================================
ALTER PROCEDURE [dbo].[uspAddProfilePermission]
    -- Add the parameters for the stored procedure here
    @NewProfileId INT,
	@AllPageIds VARCHAR(MAX), -- Comma-separated list of Page IDs
	@NewCreateUserId INT,
	@NewUpdateUserId INT
AS
BEGIN

	SET NOCOUNT ON;

	BEGIN TRY
		-- Start the transaction
        BEGIN TRANSACTION;

        -- Insert User Permission
        INSERT INTO UserPermission
			(ProfileId, PageId, CreateDate, CreateUserId, UpdateDate, UpdateUserId) 
		SELECT 
			@NewProfileId AS ProfileId, 
			TRY_CAST(SplitValue AS INT) AS PageId, 
			GETDATE() AS CreateDate, 
			@NewCreateUserId AS CreateUserId, 
			GETDATE() AS UpdateDate, 
			@NewUpdateUserId AS UpdateUserId
		FROM
			[dbo].[udfSplitStringToTable](@AllPageIds,',')
		WHERE
			TRY_CAST(SplitValue AS INT) IS NOT NULL; -- Ensure only valid integers are inserted

		-- Commit the transaction if all is successful
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
