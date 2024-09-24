USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspGetCurrentUserProfilePagesCategories]    Script Date: 12-09-2024 15:31:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: For Fetching User Data
-- =============================================
ALTER PROCEDURE [dbo].[uspGetCurrentUserProfilePagesCategories]
    -- Add the parameters for the stored procedure here
	@TextCriteria INT = 0,
    @InputInt INT = NULL,
	@RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

		-- Validate the value of @TextCriteria currently 1 to 3 only
        IF @TextCriteria NOT BETWEEN 1 AND 5
		BEGIN
				
			SELECT -1 AS RowsAffected;
			RETURN;

		END
		-- Fetch user by email
		IF @TextCriteria = 1
		BEGIN
			select u.*, u.ProfileId,tp.ProfileName from Users u LEFT JOIN UserProfile tup on tup.UserId = u.UserId LEFT JOIN Profiles tp on tp.ProfileID = tup.ProfileID WHERE U.UserId = @InputInt
		END

		-- Fetch user by ID
		ELSE IF @TextCriteria = 2
		BEGIN
			SELECT tp.ProfileID, tp.ProfileName, tp.CreateDate, tp.UpdateDate, tp.CreateUserId, tp.UpdateUserId FROM Profiles tp INNER JOIN UserProfile tbup on tp.ProfileID = tbup.ProfileID WHERE tbup.UserId = @InputInt;
		END

		ELSE IF @TextCriteria = 3
		BEGIN
			SELECT * FROM vwUserPageAccess where UserId = @InputInt;
		END

		ELSE IF @TextCriteria = 4
		BEGIN
			SELECT * FROM vwUserPagesData where UserId = @InputInt;
		END

		ELSE IF @TextCriteria = 5
		BEGIN
			SELECT U.* from Users u where u.UserId = @InputInt;
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
