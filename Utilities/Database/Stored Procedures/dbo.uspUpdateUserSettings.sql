USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspUpdateUserSettings]    Script Date: 07-06-2024 11:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Update User Settings
-- =============================================
ALTER PROCEDURE [dbo].[uspUpdateUserSettings]
    -- Add the parameters for the stored procedure here
    @UserId INT,
	@NewUserName VARCHAR(100) = NULL,
	@NewFullName VARCHAR(200) = NULL,
	@NewEmailID VARCHAR(100) = NULL,
	@NewProfilePicName VARCHAR(255) = NULL,
	@NewPasswordSalt VARCHAR(255) = NULL,
	@NewPasswordHash VARCHAR(255) = NULL,
	@NewUpdateUserId INT = NULL
AS
BEGIN
    
    SET NOCOUNT ON;
	DECLARE @OldProfilePhotoPath VARCHAR(255);

    BEGIN TRY
		-- Start transaction
        BEGIN TRANSACTION;

        -- Check if UserId exists
        IF EXISTS (SELECT 1 FROM Users u WITH (NOLOCK) WHERE u.UserId = @UserId)
        BEGIN
			-- Construct the dynamic update query
			DECLARE @sqlUpdate NVARCHAR(MAX);

			SET @sqlUpdate = 'UPDATE [Users] SET ';

			-- Append each field if the corresponding parameter is NOT NULL and not empty
            IF @NewUserName IS NOT NULL AND LTRIM(RTRIM(@NewUserName)) <> ''
                SET @sqlUpdate += 'UserName = @NewUserName, ';
            IF @NewFullName IS NOT NULL AND LTRIM(RTRIM(@NewFullName)) <> ''
                SET @sqlUpdate += 'FullName = @NewFullName, ';
            IF @NewEmailID IS NOT NULL AND LTRIM(RTRIM(@NewEmailID)) <> ''
                SET @sqlUpdate += 'EmailID = @NewEmailID, ';
            IF @NewProfilePicName IS NOT NULL AND LTRIM(RTRIM(@NewProfilePicName)) <> ''
                SET @sqlUpdate += 'ProfilePicName = @NewProfilePicName, ';
            IF @NewPasswordSalt IS NOT NULL AND LTRIM(RTRIM(@NewPasswordSalt)) <> ''
                SET @sqlUpdate += 'PasswordSalt = @NewPasswordSalt, ';
            IF @NewPasswordHash IS NOT NULL AND LTRIM(RTRIM(@NewPasswordHash)) <> ''
                SET @sqlUpdate += 'PasswordHash = @NewPasswordHash, ';
            IF @NewUpdateUserId IS NOT NULL
                SET @sqlUpdate += 'UpdateUserId = @NewUpdateUserId, ';

            -- Always update the UpdateDate field
            SET @sqlUpdate += 'UpdateDate = GETDATE(), ';

            -- Remove the trailing comma and space
            SET @sqlUpdate = LEFT(@sqlUpdate, LEN(@sqlUpdate) - 1);

            -- Add WHERE clause to restrict update to specific UserId
            SET @sqlUpdate += ' WHERE UserId = @UserId';

			-- Execute the update query
            EXEC sp_executesql @sqlUpdate,
                N'@NewUserName VARCHAR(100), @NewFullName VARCHAR(200), @NewEmailID VARCHAR(100),@NewProfilePicName VARCHAR(255),
				@NewPasswordSalt VARCHAR(255), @NewPasswordHash VARCHAR(255), @NewUpdateUserId INT, @UserId INT',
                @NewUsername, @NewFullName, @NewEmailID, @NewProfilePicName, 
				@NewPasswordSalt, @NewPasswordHash, @NewUpdateUserId, @UserId;

			-- Commit transaction
            COMMIT TRANSACTION;

            -- Return the number of rows affected
            SELECT @@ROWCOUNT AS RowsAffected;

		END
		ELSE
        BEGIN
            -- UserId does not exist
			IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

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
