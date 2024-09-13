USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspUpdateUserFlags]    Script Date: 12-09-2024 15:31:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Update Flag for Users Table
-- =============================================
ALTER PROCEDURE [dbo].[uspUpdateUserFlags]
    -- Add the parameters for the stored procedure here
	@TextCriteria INT = 0,
    @InputFlag BIT = NULL,
    @UserId INT = 0,
	@NewUpdateUserId INT = 0,
	@RowsAffected INT OUTPUT 
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
		-- Start transaction
        BEGIN TRANSACTION;

        -- Check if UserId exists
        IF EXISTS (SELECT 1 FROM Users u WHERE u.UserId = @UserId)
		BEGIN
			-- Validate the value of @TextCriteria currently 1 to 6 only
            IF @TextCriteria NOT BETWEEN 1 AND 6
			BEGIN
				-- Invalid TextCriterial, rollback transaction and return -1
				IF @@TRANCOUNT > 0
					ROLLBACK TRANSACTION;
				
				SELECT -1 AS RowsAffected;
				RETURN;

			END

			-- Perform update based on valid TextCriteria
			UPDATE Users
			SET
				IsADUser = CASE WHEN @TextCriteria = 1 THEN @InputFlag ELSE IsADUser END,
				IsActive = CASE WHEN @TextCriteria = 2 THEN @InputFlag ELSE IsActive END,
				UserVerificationSetupEnabled = CASE WHEN @TextCriteria = 3 THEN @InputFlag ELSE UserVerificationSetupEnabled END,
				RoleAssignmentEnabled = CASE WHEN @TextCriteria = 4 THEN @InputFlag ELSE RoleAssignmentEnabled END,
				EmailConfirmed = CASE WHEN @TextCriteria = 5 THEN @InputFlag ELSE EmailConfirmed END,
				PhoneNumberConfirmed = CASE WHEN @TextCriteria = 6 THEN @InputFlag ELSE PhoneNumberConfirmed END,
				UpdateDate = GETDATE(),
				UpdateUserId = @NewUpdateUserId,
				CreateUserId = CASE WHEN @TextCriteria = 5 THEN @UserId ELSE CreateUserId END
			WHERE UserId = @UserId;

			-- Capture number of rows affected
            SET @RowsAffected = @@ROWCOUNT; -- Set output parameter

			-- Commit transaction
            COMMIT TRANSACTION;

            -- Return the number of rows affected
            SELECT @RowsAffected AS RowsAffected;

		END
		ELSE
        BEGIN
			-- UserId does not exist
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            -- UserId does not exist
            SELECT -1 AS RowsAffected;
        END
    END TRY
    BEGIN CATCH
		-- Rollback transaction if there is an error
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

        -- Return an error code
        SELECT -1 AS RowsAffected;
    END CATCH;
END
