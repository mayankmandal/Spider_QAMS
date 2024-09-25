USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspUpdateEntityRecord]    Script Date: 29-05-2024 16:02:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Update Profiles & Categories & Regions
-- =============================================
ALTER PROCEDURE [dbo].[uspUpdateEntityRecord]
    -- Add the parameters for the stored procedure here
	@Id INT,
	@NewInput NVARCHAR(200),
	@NewIntInput INT = NULL,
	@Type VARCHAR(10)
AS
BEGIN
    
    SET NOCOUNT ON;

    BEGIN TRY
		-- Start the transaction
        BEGIN TRANSACTION;

		-- Region Updation
        IF @Type = 'Region'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM Region r WITH (NOLOCK) WHERE r.RegionID = @Id)
            BEGIN

                UPDATE Region SET RegionName = @NewInput
				WHERE RegionID = @Id
            END
        END

		-- City Updation
        IF @Type = 'City'
        BEGIN
			-- Check if a row exists and perform delete operation
            IF EXISTS (SELECT 1 FROM City c WITH (NOLOCK) WHERE c.CityID = @Id)
            BEGIN

                UPDATE City SET CityName = @NewInput, RegionID = @NewIntInput
				WHERE CityID = @Id
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
