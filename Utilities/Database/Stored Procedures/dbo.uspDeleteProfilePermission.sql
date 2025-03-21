USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspDeleteProfilePermission]    Script Date: 05-12-2024 15:13:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Delete User Permissions for Profile
-- =============================================
ALTER PROCEDURE [dbo].[uspDeleteProfilePermission]
-- Add the parameters for the stored procedure here 
@ProfileId INT
AS
BEGIN

	SET NOCOUNT ON;

	BEGIN TRY
		-- Start the transaction
		BEGIN TRANSACTION;

		-- Delete User Permission for ProfileId
		IF EXISTS (SELECT
					1
				FROM UserPermission up WITH (NOLOCK)
				WHERE up.ProfileId = @ProfileId
				AND up.PageId IS NOT NULL)
		BEGIN
			DELETE FROM UserPermission
			WHERE ProfileId = @ProfileId
				AND PageId IS NOT NULL;

			SELECT
				@@rowcount AS RowsAffected;
		END

		ELSE
		BEGIN
			IF @@trancount > 0
				ROLLBACK TRANSACTION;

			-- ProfileId does not exist, return an error code
			SELECT
				-1 AS RowsAffected;
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

		-- Return an error code
		SELECT
			-1 AS RowsAffected;
	END CATCH;

END
