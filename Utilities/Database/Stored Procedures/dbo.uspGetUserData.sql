USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspGetUserData]    Script Date: 12-09-2024 15:31:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: For Fetching User Data
-- =============================================
ALTER PROCEDURE [dbo].[uspGetUserData]
    -- Add the parameters for the stored procedure here
	@TextCriteria INT = 0,
    @InputInt INT = NULL,
	@InputString NVARCHAR(200) = NULL,
	@InputFlag BIT = NULL,
	@NewUpdateUserId INT = 0,
	@RowsAffected INT OUTPUT 
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

		-- Validate the value of @TextCriteria currently 1 to 3 only
        IF @TextCriteria NOT BETWEEN 1 AND 3
		BEGIN
				
			SELECT -1 AS RowsAffected;
			RETURN;

		END
		-- Fetch user by email
		IF @TextCriteria = 1
		BEGIN
			SELECT * FROM Users u WHERE u.EmailID = @InputString;
		END

		-- Fetch user by ID
		ELSE IF @TextCriteria = 2
		BEGIN
			SELECT * FROM Users u WHERE u.UserId = @InputInt;
		END

		-- Fetch user roles by ID
		ELSE IF @TextCriteria = 3
		BEGIN
			SELECT DISTINCT p.ProfileName 
			FROM Profiles p
			INNER JOIN UserProfile up ON p.ProfileID = up.ProfileID
			INNER JOIN Users u ON up.UserID = @InputInt;
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
