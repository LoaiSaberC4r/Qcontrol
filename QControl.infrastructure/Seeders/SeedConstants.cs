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

        public const string WaitingAreasViewAll =
            "WaitingAreas.ViewAll";

        public const string WaitingAreasViewDetails =
            "WaitingAreas.ViewDetails";

        public const string WaitingAreasCreate =
            "WaitingAreas.Create";

        public const string WaitingAreasUpdate =
            "WaitingAreas.Update";

        public const string WaitingAreasDelete =
            "WaitingAreas.Delete";

        public const string WindowsViewAll =
            "Windows.ViewAll";

        public const string WindowsViewDetails =
            "Windows.ViewDetails";

        public const string WindowsCreate =
            "Windows.Create";

        public const string WindowsUpdate =
            "Windows.Update";

        public const string WindowsDelete =
            "Windows.Delete";

        public const string WindowsViewDeleted =
            "Windows.ViewDeleted";

        public const string WindowsDeletePermanent =
            "Windows.DeletePermanent";

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

            public static readonly Guid WaitingAreasViewAll =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000011");

            public static readonly Guid WaitingAreasViewDetails =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000012");

            public static readonly Guid WaitingAreasCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000013");

            public static readonly Guid WaitingAreasUpdate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000014");

            public static readonly Guid WaitingAreasDelete =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000015");

            public static readonly Guid WindowsViewAll =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000016");

            public static readonly Guid WindowsViewDetails =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000017");

            public static readonly Guid WindowsCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000018");

            public static readonly Guid WindowsUpdate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000019");

            public static readonly Guid WindowsDelete =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000020");

            public static readonly Guid WindowsViewDeleted =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000021");

            public static readonly Guid WindowsDeletePermanent =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000022");
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
                    SeedIds.Permissions.WaitingAreasViewAll,
                    PermissionNames.WaitingAreasViewAll),

                new PermissionSeedItem(
                    SeedIds.Permissions.WaitingAreasViewDetails,
                    PermissionNames.WaitingAreasViewDetails),

                new PermissionSeedItem(
                    SeedIds.Permissions.WaitingAreasCreate,
                    PermissionNames.WaitingAreasCreate),

                new PermissionSeedItem(
                    SeedIds.Permissions.WaitingAreasUpdate,
                    PermissionNames.WaitingAreasUpdate),

                new PermissionSeedItem(
                    SeedIds.Permissions.WaitingAreasDelete,
                    PermissionNames.WaitingAreasDelete),

                new PermissionSeedItem(
                    SeedIds.Permissions.WindowsViewAll,
                    PermissionNames.WindowsViewAll),

                new PermissionSeedItem(
                    SeedIds.Permissions.WindowsViewDetails,
                    PermissionNames.WindowsViewDetails),

                new PermissionSeedItem(
                    SeedIds.Permissions.WindowsCreate,
                    PermissionNames.WindowsCreate),

                new PermissionSeedItem(
                    SeedIds.Permissions.WindowsUpdate,
                    PermissionNames.WindowsUpdate),

                new PermissionSeedItem(
                    SeedIds.Permissions.WindowsDelete,
                    PermissionNames.WindowsDelete),

                new PermissionSeedItem(
                    SeedIds.Permissions.WindowsViewDeleted,
                    PermissionNames.WindowsViewDeleted),

                new PermissionSeedItem(
                    SeedIds.Permissions.WindowsDeletePermanent,
                    PermissionNames.WindowsDeletePermanent),

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
