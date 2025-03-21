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
-- Description: Delete Users & Profiles & Categories & Regions
-- =============================================
ALTER PROCEDURE [dbo].[uspDeleteEntityRecord]
    -- Add the parameters for the stored procedure here
	@Id INT,
	@Type VARCHAR(50),
	@RowsAffected INT OUTPUT
AS
BEGIN
    
    SET NOCOUNT ON;

    BEGIN TRY
		-- Start the transaction
        BEGIN TRANSACTION;

        -- For User Deletion
        IF @Type = 'User'
        BEGIN
			IF EXISTS (SELECT 1 FROM Users u WITH (NOLOCK) WHERE U.UserId = @Id)
			BEGIN
				SELECT DISTINCT U.ProfilePicName AS FilePath FROM Users u WITH (NOLOCK)
				WHERE u.UserID = @Id
			END

			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM UserProfile WITH (NOLOCK) WHERE UserId = @Id)
            BEGIN
				-- Update the profile using the existing stored procedures
				EXEC [dbo].[uspDeleteUserProfile] @Id;
            END

			IF EXISTS (SELECT 1 FROM Users WITH (NOLOCK) WHERE UserId = @Id)
            BEGIN
                UPDATE Users SET IsDeleted = 1, ProfileId = NULL
				WHERE UserId = @Id
            END
        END

        -- For Profile Deletion
        ELSE IF @Type = 'Profile'
        BEGIN
            -- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM UserPermission WITH (NOLOCK) WHERE ProfileId = @Id AND PageId IS NOT NULL)
            BEGIN
                DELETE FROM UserPermission
				WHERE ProfileId = @Id AND PageId IS NOT NULL
            END

			IF EXISTS (SELECT 1 FROM UserProfile WITH (NOLOCK) WHERE ProfileID = @Id)
            BEGIN
                DELETE FROM UserProfile WHERE ProfileID = @Id
            END

			IF EXISTS (SELECT 1 FROM Users u WITH (NOLOCK) WHERE u.ProfileId = @Id)
			BEGIN
				UPDATE Users SET ProfileId = NULL WHERE ProfileId = @Id
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

		-- For Category Deletion
        ELSE IF @Type = 'Region'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM Region r WITH (NOLOCK) WHERE r.RegionID = @Id)
            BEGIN

                DELETE FROM Region
				WHERE RegionID = @Id
            END
        END

		-- For Category Deletion
        ELSE IF @Type = 'City'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM City c WITH (NOLOCK) WHERE c.CityID = @Id)
            BEGIN

                DELETE FROM City
				WHERE CityID = @Id
            END
        END

		-- For Location Deletion
        ELSE IF @Type = 'Location'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM [Location] l WITH (NOLOCK) WHERE L.LocationID = @Id)
            BEGIN

                DELETE FROM [LOCATION]
				WHERE LocationID = @Id
            END
        END

		-- For Location Deletion
        ELSE IF @Type = 'Contact'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM Contact c WITH (NOLOCK) WHERE c.ContactID = @Id)
            BEGIN

                DELETE FROM Contact
				WHERE ContactID = @Id
            END
        END

		-- For SiteDetail Deletion
        ELSE IF @Type = 'SiteDetail'
        BEGIN
			IF EXISTS (SELECT 1 FROM SitePictures sp with (NOLOCK) WHERE sp.SiteID = @Id)
			BEGIN 
				SELECT DISTINCT CAST(spc.PicCatID AS VARCHAR(5)) + '/' + sp.PicPath AS FilePath FROM SitePictures sp WITH (NOLOCK)
				INNER JOIN SitePicCategory spc WITH (NOLOCK) ON sp.PicCatID = spc.PicCatID
				WHERE sp.SiteID = @Id
			END

			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM SiteDetails sd WITH (NOLOCK) WHERE sd.SiteID = @Id)
            BEGIN

                DELETE FROM SiteDetails
				WHERE SiteID = @Id
            END
        END

		-- For SitePicture Deletion
        ELSE IF @Type = 'SitePicture'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM SitePictures sp WITH (NOLOCK) WHERE sp.SitePicID = @Id)
            BEGIN

                DELETE FROM SitePictures
				WHERE SitePicID = @Id
            END
        END

        -- Invalid Type
        ELSE
        BEGIN
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;
			SET @RowsAffected = -1;
            RETURN; -- Exit the stored procedure
        END

		-- If everything is successful, commit the transaction
        COMMIT TRANSACTION;

		SET @RowsAffected = 1;

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

		-- Return -1 to indicate failure
		SET @RowsAffected = -1;
        RETURN;

    END CATCH;

END
