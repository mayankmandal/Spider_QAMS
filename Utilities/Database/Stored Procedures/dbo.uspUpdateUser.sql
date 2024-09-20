USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspUpdateUser]    Script Date: 21-05-2024 15:31:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Update User Profile
-- =============================================
ALTER PROCEDURE [dbo].[uspUpdateUser]
    -- Add the parameters for the stored procedure here
    @UserId INT,
	@NewDesignation VARCHAR(10) = NULL,
	@NewFullName VARCHAR(200) = NULL,
	@NewEmailID VARCHAR(100) = NULL,
	@NewPhoneNumber VARCHAR(15) = NULL,
	@NewProfileId INT = NULL,
	@NewUserName VARCHAR(100) = NULL,
	@NewProfilePicName VARCHAR(255) = NULL,
	@NewIsActive BIT = NULL,
	@NewIsADUser BIT = NULL,
	@NewUpdateUserId INT
AS
BEGIN
    
    SET NOCOUNT ON;
	DECLARE @OldProfileId INT;
	-- Construct the dynamic update query
	DECLARE @sqlUpdate NVARCHAR(MAX);

    BEGIN TRY
		-- Start transaction
        BEGIN TRANSACTION;

        -- Check if UserId exists
        IF EXISTS (SELECT 1 FROM Users WITH (NOLOCK) WHERE UserId = @UserId)
        BEGIN

			-- Get the current ProfileId
            SELECT @OldProfileId = anur.ProfileID FROM Users u with (NOLOCK) INNER JOIN UserProfile anur WITH (NOLOCK) ON u.UserId = anur.UserId WHERE u.UserId = @UserId;

			-- Convert NULL to a default value
			SET @OldProfileId = ISNULL(@OldProfileId, 0);
			SET @NewProfileId = ISNULL(@NewProfileId, 0);

			-- Check if the ProfileId needs to be updated
            IF @OldProfileId <> @NewProfileId AND @NewProfileId <> 0
			BEGIN
                -- Update the profile using the existing stored procedures
                EXEC [dbo].[uspDeleteUserProfile] @UserId;
                EXEC [dbo].[uspAddNewUserProfile] @NewProfileId, @UserId, @NewUpdateUserId, @NewUpdateUserId;
            END

			SET @sqlUpdate = 'UPDATE Users SET ';

			-- Append each field if the corresponding parameter is NOT NULL

			IF @NewDesignation IS NOT NULL
                SET @sqlUpdate += 'Designation = @NewDesignation, ';
            IF @NewFullName IS NOT NULL
                SET @sqlUpdate += 'FullName = @NewFullName, ';
            IF @NewEmailID IS NOT NULL
                SET @sqlUpdate += 'EmailID = @NewEmailID, ';
            IF @NewPhoneNumber IS NOT NULL
                SET @sqlUpdate += 'PhoneNumber = @NewPhoneNumber, ';
            IF @NewUsername IS NOT NULL
                SET @sqlUpdate += 'UserName = @NewUserName, ';
            IF @NewUpdateUserId IS NOT NULL
                SET @sqlUpdate += 'UpdateUserId = @NewUpdateUserId, ';
            IF @NewProfilePicName IS NOT NULL
                SET @sqlUpdate += 'ProfilePicName = @NewProfilePicName, ';
            IF @NewIsActive IS NOT NULL
                SET @sqlUpdate += 'IsActive = @NewIsActive, ';
            IF @NewIsADUser IS NOT NULL
                SET @sqlUpdate += 'IsADUser = @NewIsADUser, ';

			SET @sqlUpdate += 'UpdateDate = GETDATE(), ';
			-- Remove the trailing comma and space
			SET @sqlUpdate = LEFT(@sqlUpdate, LEN(@sqlUpdate) - 1);

			-- Add WHERE clause to restrict update to specific UserId
			SET @sqlUpdate += ' WHERE UserId = @UserId';

			-- Execute the update query
            EXEC sp_executesql @sqlUpdate,
                N'@NewDesignation VARCHAR(10), @NewFullName VARCHAR(200), @NewEmailID VARCHAR(100), @NewPhoneNumber VARCHAR(15), 
                  @NewUsername VARCHAR(100), @NewUpdateUserId INT, @NewProfilePicName VARCHAR(255), 
				  @NewIsActive BIT, @NewIsADUser BIT, @UserId INT',
                @NewDesignation, @NewFullName, @NewEmailID, @NewPhoneNumber, 
                @NewUsername, @NewUpdateUserId, @NewProfilePicName,
				@NewIsActive, @NewIsADUser, @UserId;

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
