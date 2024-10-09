USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspCheckUniqueness]    Script Date: 21-06-2024 17:48:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Check uniqueness in DB table - AspNetUsers for particular fields
-- =============================================
ALTER PROCEDURE [dbo].[uspCheckUniqueness]
    -- Add the parameters for the stored procedure here
	@TableId INT,
	@Field1 NVARCHAR(50) = NULL,
	@Field2 NVARCHAR(50) = NULL,
    @Value1 NVARCHAR(100) = NULL,
	@Value2 NVARCHAR(100) = NULL
AS
BEGIN
    
    SET NOCOUNT ON;

    BEGIN TRY
		DECLARE @query NVARCHAR(MAX);

		IF @TableId = 1
		BEGIN
			SET @query = N'SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WITH (NOLOCK) WHERE ' + @Field1 + ' = @Value1) THEN 0 ELSE 1 END AS IsUnique';
			EXEC sp_executesql @query, N'@Value1 VARCHAR(100)', @Value1;
		END
		ELSE IF @TableId = 2
		BEGIN
			SET @query = N'SELECT CASE WHEN EXISTS (SELECT 1 FROM Profiles WITH (NOLOCK) WHERE ' + @Field1 + ' = @Value1) THEN 0 ELSE 1 END AS IsUnique';
			EXEC sp_executesql @query, N'@Value1 VARCHAR(100)', @Value1;
		END
		ELSE IF @TableId = 3
		BEGIN
			SET @query = N'SELECT CASE WHEN EXISTS (SELECT 1 FROM tblPageCategory WITH (NOLOCK) WHERE ' + @Field1 + ' = @Value1) THEN 0 ELSE 1 END AS IsUnique';
			EXEC sp_executesql @query, N'@Value1 VARCHAR(100)', @Value1;
		END
		ELSE IF @TableId = 4
		BEGIN
			SET @query = N'SELECT CASE WHEN EXISTS (SELECT 1 FROM Region WITH (NOLOCK) WHERE ' + @Field1 + ' = @Value1) THEN 0 ELSE 1 END AS IsUnique';
			EXEC sp_executesql @query, N'@Value1 VARCHAR(100)', @Value1;
		END
		ELSE IF @TableId = 5
		BEGIN
			SET @query = N'SELECT CASE WHEN EXISTS (SELECT 1 FROM City WITH (NOLOCK) WHERE ' + @Field1 + ' = @Value1) THEN 0 ELSE 1 END AS IsUnique';
			EXEC sp_executesql @query, N'@Value1 VARCHAR(100)', @Value1;
		END
		ELSE IF @TableId = 6
		BEGIN
			SET @query = N'SELECT CASE WHEN EXISTS (SELECT 1 FROM SiteDetails WITH (NOLOCK) WHERE ' + @Field1 + ' = @Value1) THEN 0 ELSE 1 END AS IsUnique';
			EXEC sp_executesql @query, N'@Value1 VARCHAR(100)', @Value1;
		END
		ELSE IF @TableId = 7
		BEGIN
			SET @query = N'SELECT CASE WHEN EXISTS (SELECT 1 FROM SiteDetails WITH (NOLOCK) WHERE ' + @Field1 + ' = @Value1 AND ' + @Field2 + ' = @Value2) THEN 0 ELSE 1 END AS IsUnique';
			EXEC sp_executesql @query, N'@Value1 VARCHAR(100), @Value2 VARCHAR(100)', @Value1, @Value2;
		END

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
    END CATCH;

END
