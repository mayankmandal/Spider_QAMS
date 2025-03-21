USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspDeleteUserProfile]    Script Date: 21-05-2024 15:30:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Insert New Relationship between User & Profile
-- =============================================
ALTER PROCEDURE [dbo].[uspDeleteUserProfile]
    -- Add the parameters for the stored procedure here
	@UserId INT
AS
BEGIN
    
    SET NOCOUNT ON;

    BEGIN TRY
		-- Start the transaction
        BEGIN TRANSACTION;

        -- Delete User Profile
		IF EXISTS (SELECT 1 FROM UserProfile WITH (NOLOCK) WHERE UserId = @UserId)
			BEGIN
				DELETE FROM UserProfile where UserId = @UserId
			END

		-- If everything is successful, commit the transaction
        COMMIT TRANSACTION;

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
