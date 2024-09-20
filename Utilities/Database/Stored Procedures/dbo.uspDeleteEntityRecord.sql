USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspDeleteEntityRecord]    Script Date: 29-05-2024 16:02:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Delete Users & Profiles & Categories
-- =============================================
ALTER PROCEDURE [dbo].[uspDeleteEntityRecord]
    -- Add the parameters for the stored procedure here
	@Id INT,
	@Type VARCHAR(10)
AS
BEGIN
    
    SET NOCOUNT ON;

    BEGIN TRY
		-- Start the transaction
        BEGIN TRANSACTION;

        -- For User Deletion
        IF @Type = 'User'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM UserProfile WITH (NOLOCK) WHERE UserId = @Id)
            BEGIN
                DELETE FROM UserProfile
				WHERE UserId = @Id
            END

			IF EXISTS (SELECT 1 FROM Users WITH (NOLOCK) WHERE UserId = @Id)
            BEGIN
                DELETE FROM Users
				WHERE UserId = @Id
            END
        END

        -- For Profile Deletion
        ELSE IF @Type = 'Profile'
        BEGIN
            -- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM UserPermission WITH (NOLOCK) WHERE ProfileId = @Id)
            BEGIN
                DELETE FROM UserPermission
				WHERE ProfileId = @Id
            END

			IF EXISTS (SELECT 1 FROM UserProfile WITH (NOLOCK) WHERE ProfileID = @Id)
            BEGIN
                DELETE FROM UserProfile
				WHERE ProfileID = @Id
            END

			IF EXISTS (SELECT 1 FROM Profiles WITH (NOLOCK) WHERE ProfileID = @Id)
            BEGIN
                DELETE FROM Profiles
				WHERE ProfileID = @Id
            END
        END

        -- For Category Deletion
        ELSE IF @Type = 'Category'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM tblPageCategory WITH (NOLOCK) WHERE PageCatId = @Id)
            BEGIN
				-- Remove Foreign Key Constraint for delete category Id
				UPDATE tblPage SET PageCatId = NULL WHERE
				PageCatId = @Id

                DELETE FROM tblPageCategory
				WHERE PageCatId = @Id
            END
        END

        -- Invalid Type
        ELSE
        BEGIN
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;
            SELECT -1 AS RowsAffected; -- Indicate invalid type
            RETURN; -- Exit the stored procedure
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
