namespace Qcontrol.infrastructure.Seeders;

internal static class SeedConstants
{
    public static class TechnicalAdminSeed
    {
        public static readonly Guid ApplicationUserId =
            Guid.Parse(
                "10000000-0000-0000-0000-000000000001");

        public static readonly Guid TechnicalAdminId =
            Guid.Parse(
                "10000000-0000-0000-0000-000000000002");

        public const string Email =
            "technicaladmin@qcontrol.local";

        public const string UserName =
            "technicaladmin";

        public const string NameEn =
            "System Technical Admin";

        // Development only.
        public const string DefaultPassword =
            "Admin@123456";
    }

    public static class RoleNames
    {
        public const string TechnicalAdministrator =
            "Technical Administrator";
    }

    public static class PermissionNames
    {
        public const string BranchesViewAll =
            "Branches.ViewAll";

        public const string BranchesViewDetails =
            "Branches.ViewDetails";

        public const string BranchesCreate =
            "Branches.Create";

        public const string BranchesUpdate =
            "Branches.Update";

        public const string BranchesDelete =
            "Branches.Delete";

        public const string GlobalConfigurationsViewAll =
            "GlobalConfigurations.ViewAll";

        public const string GlobalConfigurationsViewDetails =
            "GlobalConfigurations.ViewDetails";

        public const string GlobalConfigurationsCreate =
            "GlobalConfigurations.Create";

        public const string GlobalConfigurationsUpdate =
            "GlobalConfigurations.Update";

        public const string GlobalConfigurationsDelete =
            "GlobalConfigurations.Delete";
    }

    public static class SeedIds
    {
        public static class Roles
        {
            public static readonly Guid TechnicalAdministrator =
                Guid.Parse(
                    "20000000-0000-0000-0000-000000000001");
        }

        public static class Permissions
        {
            public static readonly Guid BranchesViewAll =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000001");

            public static readonly Guid BranchesViewDetails =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000002");

            public static readonly Guid BranchesCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000003");

            public static readonly Guid BranchesUpdate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000004");

            public static readonly Guid BranchesDelete =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000005");

            public static readonly Guid GlobalConfigurationsViewAll =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000006");

            public static readonly Guid GlobalConfigurationsViewDetails =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000007");

            public static readonly Guid GlobalConfigurationsCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000008");

            public static readonly Guid GlobalConfigurationsUpdate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000009");

            public static readonly Guid GlobalConfigurationsDelete =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000010");
        }
    }

    public static class SeedCatalog
    {
        public static IReadOnlyCollection<RoleSeedItem> Roles { get; } =
            new[]
            {
                new RoleSeedItem(
                    SeedIds.Roles.TechnicalAdministrator,
                    RoleNames.TechnicalAdministrator)
            };

        public static IReadOnlyCollection<PermissionSeedItem>
            Permissions
        { get; } =
            new[]
            {
                new PermissionSeedItem(
                    SeedIds.Permissions.BranchesViewAll,
                    PermissionNames.BranchesViewAll),

                new PermissionSeedItem(
                    SeedIds.Permissions.BranchesViewDetails,
                    PermissionNames.BranchesViewDetails),

                new PermissionSeedItem(
                    SeedIds.Permissions.BranchesCreate,
                    PermissionNames.BranchesCreate),

                new PermissionSeedItem(
                    SeedIds.Permissions.BranchesUpdate,
                    PermissionNames.BranchesUpdate),

                new PermissionSeedItem(
                    SeedIds.Permissions.BranchesDelete,
                    PermissionNames.BranchesDelete),

                new PermissionSeedItem(
                    SeedIds.Permissions.GlobalConfigurationsViewAll,
                    PermissionNames.GlobalConfigurationsViewAll),

                new PermissionSeedItem(
                    SeedIds.Permissions.GlobalConfigurationsViewDetails,
                    PermissionNames.GlobalConfigurationsViewDetails),

                new PermissionSeedItem(
                    SeedIds.Permissions.GlobalConfigurationsCreate,
                    PermissionNames.GlobalConfigurationsCreate),

                new PermissionSeedItem(
                    SeedIds.Permissions.GlobalConfigurationsUpdate,
                    PermissionNames.GlobalConfigurationsUpdate),

                new PermissionSeedItem(
                    SeedIds.Permissions.GlobalConfigurationsDelete,
                    PermissionNames.GlobalConfigurationsDelete)
            };

        public static IReadOnlyCollection<RolePermissionSeedItem>
            RolePermissions
        { get; } =
            Permissions
                .Select(permission =>
                    new RolePermissionSeedItem(
                        SeedIds.Roles.TechnicalAdministrator,
                        permission.Id))
                .ToArray();
    }

    internal sealed record RoleSeedItem(
        Guid Id,
        string Name);

    internal sealed record PermissionSeedItem(
        Guid Id,
        string Name);

    internal sealed record RolePermissionSeedItem(
        Guid RoleId,
        Guid PermissionId);
}