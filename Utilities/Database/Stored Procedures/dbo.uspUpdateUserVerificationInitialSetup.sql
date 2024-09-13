USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspUpdateUserVerificationInitialSetup]    Script Date: 25-07-2024 17:38:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Update User Verification Details at Initial Setup
-- =============================================
ALTER PROCEDURE [dbo].[uspUpdateUserVerificationInitialSetup]
    -- Add the parameters for the stored procedure here
    @UserId INT,
	@NewDesignation VARCHAR(10) = NULL,
	@NewFullName VARCHAR(200) = NULL,
	@NewPhoneNumber VARCHAR(15) = NULL,
	@NewUsername VARCHAR(100) = NULL,
	@NewProfilePicName VARCHAR(255) = NULL,
	@NewIsActive BIT = NULL,
	@NewUpdateUserId INT = 0,
	@NewCreateUserId INT = 0,
	@TextCriteria INT = 0,
    @InputFlag BIT = NULL,
	@BaseUserRoleName VARCHAR(200) = NULL
AS
BEGIN

    SET NOCOUNT ON;
	-- Construct the dynamic update query
	DECLARE @sqlUpdate NVARCHAR(MAX);

    BEGIN TRY
		-- Start transaction
        BEGIN TRANSACTION;

        -- Check if UserId exists
        IF EXISTS (SELECT 1 FROM Users WITH (NOLOCK) WHERE UserId = @UserId)
        BEGIN
			
			SET @sqlUpdate = 'UPDATE [AspNetUsers] SET ';

			--- Append fields only if parameters are provided
            IF @NewDesignation IS NOT NULL SET @sqlUpdate += 'Designation = @NewDesignation, ';
            IF @NewFullName IS NOT NULL SET @sqlUpdate += 'FullName = @NewFullName, ';
            IF @NewPhoneNumber IS NOT NULL SET @sqlUpdate += 'PhoneNumber = @NewPhoneNumber, ';
            IF @NewUsername IS NOT NULL SET @sqlUpdate += 'Username = @NewUsername, ';
            IF @NewUpdateUserId IS NOT NULL SET @sqlUpdate += 'UpdateUserId = @NewUpdateUserId, ';
            IF @NewProfilePicName IS NOT NULL SET @sqlUpdate += 'ProfilePicName = @NewProfilePicName, ';
            IF @NewIsActive IS NOT NULL SET @sqlUpdate += 'IsActive = @NewIsActive, ';

			-- Always update UpdateDate
            SET @sqlUpdate += 'UpdateDate = GETDATE() ';

			-- Remove trailing comma and add WHERE clause
            SET @sqlUpdate = LEFT(@sqlUpdate, LEN(@sqlUpdate) - 1) + ' WHERE Id = @UserId';

			-- Execute dynamic SQL with sp_executesql
            EXEC sp_executesql @sqlUpdate,
                N'@NewDesignation VARCHAR(10), @NewFullName VARCHAR(200), @NewPhoneNumber VARCHAR(15), 
                  @NewUsername VARCHAR(100), @NewProfilePicName VARCHAR(255), @NewIsActive BIT, 
                  @NewUpdateUserId INT, @UserId INT',
                @NewDesignation, @NewFullName, @NewPhoneNumber, 
                @NewUsername, @NewProfilePicName, @NewIsActive, 
                @NewUpdateUserId, @UserId;

			-- Handle affected rows
			IF @@rowcount = 1
			BEGIN
				-- Verify if user meets criteria
                IF EXISTS (
                    SELECT 1 FROM Users WITH (NOLOCK)
                    WHERE UserId = @UserId AND EmailConfirmed = 1 
                          AND UserName IS NOT NULL AND PhoneNumber IS NOT NULL 
                          AND EmailID IS NOT NULL AND Designation IS NOT NULL
                )
				BEGIN
					DECLARE @RowsAffected INT;
					-- Update UserVerificationSetupEnabled to 1

					-- Call the second stored procedure and capture the output
					EXEC uspUpdateUserFlags @TextCriteria, @InputFlag, @UserId, @NewUpdateUserId, @RowsAffected OUTPUT;

					-- Check RoleAssignmentEnabled
                    IF EXISTS(SELECT 1 FROM Users WHERE UserVerificationSetupEnabled = 1 
                              AND (RoleAssignmentEnabled IS NULL OR RoleAssignmentEnabled = 0))
                    BEGIN
                        DECLARE @BaseProfileId INT;
                        SELECT @BaseProfileId = ProfileID FROM Profiles WHERE ProfileName = @BaseUserRoleName;

                        EXEC uspAddNewUserProfile @BaseProfileId, @UserId, @UserId, @UserId;
                    END

					-- Commit transaction
                    COMMIT TRANSACTION;

					-- Return the number of rows affected
					SELECT @RowsAffected AS RowsAffected;
					
				END
				ELSE
				BEGIN
					-- Rollback transaction as condition is not met
                    ROLLBACK TRANSACTION;

					-- Condition not met, return -1
					SELECT -1 AS RowsAffected;
				END
			END
			ELSE
			BEGIN
				-- Rollback transaction if no rows were affected
                ROLLBACK TRANSACTION;

				-- No rows affected, return -1
				SELECT -1 AS RowsAffected;
			END
		END
		ELSE
        BEGIN
			-- Rollback transaction if UserId does not exist
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
