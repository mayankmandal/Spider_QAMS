USE [Spider_QAMS]
GO
/****** Object:  UserDefinedFunction [dbo].[udfSplitStringToTable]    Script Date: 05-12-2024 12:13:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Your Name
-- Create date: YYYY-MM-DD
-- Description:	Split Specified delimiter Splitted string
-- =============================================
ALTER FUNCTION [dbo].[udfSplitStringToTable]
(	
	-- Add the parameters for the function here
	@InputString VARCHAR(MAX),
    @Delimiter CHAR(1)
)
RETURNS @OutputTable TABLE
(SplitValue NVARCHAR(MAX)) -- Generic type for versatility; casting can be done externally
AS
BEGIN
	-- Use STRING_SPLIT to split the string
	INSERT INTO @OutputTable (SplitValue)
	SELECT 
		TRIM(VALUE) -- Clean up any extra whitespace
	FROM
		STRING_SPLIT(@InputString, @Delimiter);
	RETURN;
END
