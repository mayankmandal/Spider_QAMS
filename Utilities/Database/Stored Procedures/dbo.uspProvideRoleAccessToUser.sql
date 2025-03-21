USE [Spider_QAMS]
GO
/****** Object:  StoredProcedure [dbo].[uspProvideRoleAccessToUser]    Script Date: 08-08-2024 14:55:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Your Name
-- Create date: YYYY-MM-DD
-- Description: Insert New Role corresponding to User in [AspNetUserRoles] table
-- =============================================
CREATE PROCEDURE [dbo].[uspProvideRoleAccessToUser]
    -- Add the parameters for the stored procedure here
	@RoleName NVARCHAR(100),
    @EmailAddress NVARCHAR(200),
	@NewCreateUserId INT,
	@NewUpdateUserId INT
AS
BEGIN
    
    SET NOCOUNT ON;

	-- Validate parameters
    IF @RoleName IS NULL OR @EmailAddress IS NULL OR @NewCreateUserId IS NULL OR @NewUpdateUserId IS NULL
    BEGIN
        RAISERROR('One or more parameters are null.', 16, 1);
        RETURN;
    END

	DECLARE @UserIdentity INT; -- Declare variable to store UserId
	DECLARE @RoleId INT; -- Declare variable to store RoleId

    BEGIN TRY
		-- Start transaction
        BEGIN TRANSACTION;

        -- Search for User ID from Email Address
        SELECT @UserIdentity = tbu.Id 
        FROM AspNetUsers tbu WITH (NOLOCK)
        WHERE tbu.Email = @EmailAddress

        IF @UserIdentity IS NULL
        BEGIN
            RAISERROR('User not found.', 16, 1);
			ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Search for Role ID from Role Name
        SELECT @RoleId = ar.Id 
        FROM AspNetRoles ar WITH (NOLOCK)
        WHERE ar.Name = @RoleName

        IF @RoleId IS NULL
        BEGIN
            RAISERROR('Role not found.', 16, 1);
			ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Check if the user already has the role
        IF EXISTS (SELECT 1 FROM AspNetUserRoles anur WITH (NOLOCK) 
                   WHERE anur.UserId = @UserIdentity AND anur.RoleId = @RoleId)
        BEGIN
            RAISERROR('User already has this role.', 16, 1);
			ROLLBACK TRANSACTION;
            RETURN;
        END

		-- Disabled RoleAssignment for User since profile getting unassigned
		EXEC [dbo].[uspUpdateRoleAssignmentUser] @UserIdentity, 0, @NewUpdateUserId

        -- Delete User Profile, before inserting new user role
        EXEC [dbo].[uspDeleteUserProfile] @UserIdentity;

		-- Insert User Profile
        EXEC [dbo].[uspAddNewUserProfile] @RoleId, @UserIdentity, @NewUpdateUserId, @NewUpdateUserId;

		-- Enabled RoleAssignment for User since profile is assigned
		EXEC [dbo].[uspUpdateRoleAssignmentUser] @UserIdentity, 1, @NewUpdateUserId

		-- Commit transaction
        COMMIT TRANSACTION;

        SELECT 'Success' AS Message, @UserIdentity AS UserIdentity, @RoleId AS RoleId;

    END TRY

    BEGIN CATCH
		-- Handle exceptions and rollback transaction if needed
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

		RAISERROR('An error occurred while processing the request.', 16, 1);
    END CATCH;

END
