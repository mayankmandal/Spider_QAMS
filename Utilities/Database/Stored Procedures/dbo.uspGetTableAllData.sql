USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspGetTableAllData]    Script Date: 12-09-2024 15:31:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: For Fetching All Data for a Table
-- =============================================
ALTER PROCEDURE [dbo].[uspGetTableAllData]
    -- Add the parameters for the stored procedure here
	@TextCriteria INT = 0,
	@RowsAffected INT OUTPUT 
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

		-- Validate the value of @TextCriteria currently 1 to 4 only
        IF @TextCriteria NOT BETWEEN 1 AND 4
		BEGIN
				
			SELECT -1 AS RowsAffected;
			RETURN;

		END
		-- GetAllUsersData
		IF @TextCriteria = 1
		BEGIN
			SELECT p.ProfileID, p.ProfileName, u.UserId, u.Designation, u.FullName, u.EmailID, u.PhoneNumber, u.Username, u.ProfilePicName, u.IsActive, U.IsADUser, U.IsDeleted FROM Users u WITH (NOLOCK) INNER JOIN UserProfile up WITH (NOLOCK) on up.UserId = u.UserId INNER JOIN Profiles p WITH (NOLOCK) on p.ProfileID = up.ProfileID AND u.IsDeleted != 1
		END

		-- GetAllProfiles
		ELSE IF @TextCriteria = 2
		BEGIN
			SELECT ProfileId, ProfileName, CreateDate, CreateUserId, UpdateDate, UpdateUserId FROM Profiles p WITH (NOLOCK)
		END

		-- GetAllPages
		ELSE IF @TextCriteria = 3
		BEGIN
			SELECT PageId, PageUrl, PageDesc, PageSeq, PageCatId, PageImgUrl, PageName FROM tblPage WITH (NOLOCK)
		END

		-- GetAllCategories
		ELSE IF @TextCriteria = 4
		BEGIN
			SELECT PageCatId,CategoryName FROM tblPageCategory WITH (NOLOCK)
		END

		-- Capture number of rows affected
        SET @RowsAffected = @@ROWCOUNT; -- Set output parameter

        -- Return the number of rows affected
        SELECT @RowsAffected AS RowsAffected;

    END TRY
    BEGIN CATCH

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
