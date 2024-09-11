USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspRegisterNewUser]    Script Date: 11-09-2024 17:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Register New User
-- =============================================
ALTER PROCEDURE [dbo].[uspRegisterNewUser]
    -- Add the parameters for the stored procedure here
	@NewFullName VARCHAR(100),
	@NewEmail VARCHAR(50),
	@NewPasswordSalt VARCHAR(255),
	@NewPasswordHash VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @NewUserId INT = 0; -- Declare variable to store ProfileId

    BEGIN TRY
		-- Start the transaction
        BEGIN TRANSACTION;

        -- Insert User Profile
        INSERT INTO [dbo].[Users](FullName,EmailID,PasswordSalt,PasswordHash, CreateDate, CreateUserId, UpdateDate, UpdateUserId) 
		VALUES (@NewFullName,@NewEmail,@NewPasswordSalt,@NewPasswordHash, GETDATE(), -1, GETDATE(), -1);
		-- Retrieve the newly generated ProfileId
		SET @NewUserId = SCOPE_IDENTITY(); -- Get the last inserted identity value

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

		-- Set the output parameter to NULL in case of error
        SET @NewUserId = NULL;

    END CATCH;

	-- Check if @NewProfileId is initialized and return the appropriate message
	IF @NewUserId <> 0
	BEGIN
		-- If @NewProfileId is not Null , return the ProfileId
		SELECT * FROM Users u WHERE u.UserId = @NewUserId;
	END
	ELSE
	BEGIN
		-- If NewProfileId is NULL, return error
		SELECT 'Error' AS ErrorMessage;
	END
END
