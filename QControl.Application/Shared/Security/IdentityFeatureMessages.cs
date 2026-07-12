using Qcontrol.Domain.Resources;

namespace QControl.Application.Shared.Security;

internal static class IdentityFeatureMessages
{
    public static string BranchAdminUserNameRequired =>
        Get("BranchAdmin_UserName_Required", "Username is required.");

    public static string BranchAdminUserNameMaxLength =>
        Get("BranchAdmin_UserName_MaxLength", "Username must not exceed 100 characters.");

    public static string BranchAdminEmailRequired =>
        Get("BranchAdmin_Email_Required", "Email is required.");

    public static string BranchAdminEmailInvalid =>
        Get("BranchAdmin_Email_Invalid", "Email must be valid.");

    public static string BranchAdminEmailMaxLength =>
        Get("BranchAdmin_Email_MaxLength", "Email must not exceed 200 characters.");

    public static string BranchAdminNameEnRequired =>
        Get("BranchAdmin_NameEn_Required", "English name is required.");

    public static string BranchAdminNameEnMaxLength =>
        Get("BranchAdmin_NameEn_MaxLength", "English name must not exceed 200 characters.");

    public static string BranchAdminNameArMaxLength =>
        Get("BranchAdmin_NameAr_MaxLength", "Arabic name must not exceed 200 characters.");

    public static string BranchAdminPhoneMaxLength =>
        Get("BranchAdmin_Phone_MaxLength", "Phone number must not exceed 30 characters.");

    public static string TemporaryPasswordRequired =>
        Get("BranchAdmin_TemporaryPassword_Required", "Temporary password is required.");

    public static string PasswordRequired =>
        Get("Password_Required", "Password is required.");

    public static string PasswordMaxLength =>
        Get("Password_MaxLength", "Password must not exceed the configured maximum length.");

    public static string PasswordPolicy =>
        Get("Password_Policy_Invalid", "Password does not satisfy the configured password policy.");

    public static string BranchAdminCreated =>
        Get("BranchAdmin_Create_Success", "Branch administrator was created successfully.");

    public static string BranchInactive =>
        Get("Branch_Inactive", "The branch is inactive.");

    public static string ForbiddenActorType =>
        Get("BranchAdmin_Create_ForbiddenActorType", "Only a technical administrator can create branch administrators.");

    public static string RoleNotFound =>
        Get("BranchAdmin_Create_RoleNotFound", "The Branch Administrator system role is not configured.");

    public static string UserNameAlreadyExists =>
        Get("BranchAdmin_Create_UserNameAlreadyExists", "Username already exists.");

    public static string EmailAlreadyExists =>
        Get("BranchAdmin_Create_EmailAlreadyExists", "Email already exists.");

    public static string BranchAdminProfileNotFound =>
        Get("Login_BranchAdminProfile_NotFound", "The branch administrator profile could not be found.");

    public static string BranchAssignmentNotFound =>
        Get("Login_BranchAssignment_NotFound", "The branch administrator has no branch assignment.");

    public static string MultipleBranchAssignmentsNotSupported =>
        Get("Login_MultipleBranchAssignments_NotSupported", "Multiple branch assignments are not supported in the current login flow.");

    public static string LoginBranchNotFound =>
        Get("Login_Branch_NotFound", "The assigned branch could not be found.");

    public static string LoginBranchInactive =>
        Get("Login_Branch_Inactive", "The assigned branch is inactive.");

    public static string PasswordChangeRequired =>
        Get("Auth_PasswordChangeRequired", "Password change is required before accessing this endpoint.");

    public static string ChangeInitialPasswordUnauthenticated =>
        Get("ChangeInitialPassword_Unauthenticated", "Authentication is required.");

    public static string ChangeInitialPasswordUserNotFound =>
        Get("ChangeInitialPassword_UserNotFound", "The current user could not be found.");

    public static string ChangeInitialPasswordUserInactive =>
        Get("ChangeInitialPassword_UserInactive", "The current user is inactive.");

    public static string ChangeInitialPasswordNotRequired =>
        Get("ChangeInitialPassword_NotRequired", "Initial password change is not required.");

    public static string ChangeInitialPasswordInvalidTokenPurpose =>
        Get("ChangeInitialPassword_InvalidTokenPurpose", "The token is not valid for initial password change.");

    public static string SameAsTemporaryPassword =>
        Get("ChangeInitialPassword_SameAsTemporaryPassword", "New password must be different from the temporary password.");

    public static string ChangeInitialPasswordSuccess =>
        Get("ChangeInitialPassword_Success", "Initial password changed successfully.");

    public static string BranchScopeForbidden =>
        Get("BranchScope_Forbidden", "The current user cannot access this branch.");

    public static string ActiveBranchRequired =>
        Get("BranchScope_ActiveBranchRequired", "An active branch is required for this operation.");

    public static string UnsupportedActorType =>
        Get("BranchScope_UnsupportedActorType", "The current user type cannot perform this operation.");

    private static string Get(string resourceName, string fallback) =>
        ErrorMessage.ResourceManager.GetString(resourceName, ErrorMessage.Culture)
        ?? fallback;
}
