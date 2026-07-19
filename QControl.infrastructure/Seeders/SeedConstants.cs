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

        public const string BranchAdministrator =
            "Branch Administrator";
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

        public const string BranchesDeactivate =
            "Branches.Deactivate";

        public const string BranchesReactivate =
            "Branches.Reactivate";

        public const string BranchesDeletePermanent =
            "Branches.DeletePermanent";

        public const string BranchServicesUnassign =
    "BranchServices.Unassign";

        public const string BranchAdminsCreate =
            "BranchAdmins.Create";

        public const string WaitingAreasViewAll =
            "WaitingAreas.ViewAll";

        public const string WaitingAreasViewDetails =
            "WaitingAreas.ViewDetails";

        public const string WaitingAreasCreate =
            "WaitingAreas.Create";

        public const string WaitingAreasUpdate =
            "WaitingAreas.Update";

        public const string WaitingAreasDeactivate =
            "WaitingAreas.Deactivate";

        public const string WaitingAreasReactivate =
            "WaitingAreas.Reactivate";

        public const string WaitingAreasDeletePermanent =
            "WaitingAreas.DeletePermanent";

        public const string WindowsViewAll =
            "Windows.ViewAll";

        public const string WindowsViewDetails =
            "Windows.ViewDetails";

        public const string WindowsCreate =
            "Windows.Create";

        public const string WindowsUpdate =
            "Windows.Update";

        public const string WindowsDeactivate =
            "Windows.Deactivate";

        public const string WindowsReactivate =
            "Windows.Reactivate";

        public const string WindowsDeletePermanent =
            "Windows.DeletePermanent";

        public const string TerminalsViewAll =
            "Terminals.ViewAll";

        public const string TerminalsViewDetails =
            "Terminals.ViewDetails";

        public const string TerminalsCreate =
            "Terminals.Create";

        public const string TerminalsUpdate =
            "Terminals.Update";

        public const string TerminalsDeactivate =
            "Terminals.Deactivate";

        public const string TerminalsReactivate =
            "Terminals.Reactivate";

        public const string TerminalsDeletePermanent =
            "Terminals.DeletePermanent";

        public const string DisplaysViewAll =
            "Displays.ViewAll";

        public const string DisplaysViewDetails =
            "Displays.ViewDetails";

        public const string DisplaysCreate =
            "Displays.Create";

        public const string DisplaysUpdate =
            "Displays.Update";

        public const string DisplaysDeactivate =
            "Displays.Deactivate";

        public const string DisplaysReactivate =
            "Displays.Reactivate";

        public const string DisplaysDeletePermanent =
            "Displays.DeletePermanent";

        public const string ServicesViewAll =
            "Services.ViewAll";

        public const string ServicesViewDetails =
            "Services.ViewDetails";

        public const string ServicesCreate =
            "Services.Create";

        public const string ServicesUpdate =
            "Services.Update";

        public const string ServicesDelete =
            "Services.Delete";

        public const string ServicesRestore =
            "Services.Restore";

        public const string BranchServicesView =
            "BranchServices.View";

        public const string BranchServicesAssign =
            "BranchServices.Assign";

        public const string BranchServiceTreesCreate =
            "BranchServiceTrees.Create";

        public const string ServiceGlobalizationRequestsViewAll =
            "ServiceGlobalizationRequests.ViewAll";

        public const string ServiceGlobalizationRequestsViewDetails =
            "ServiceGlobalizationRequests.ViewDetails";

        public const string ServiceGlobalizationRequestsViewOwn =
            "ServiceGlobalizationRequests.ViewOwn";

        public const string ServiceGlobalizationRequestsApprove =
            "ServiceGlobalizationRequests.Approve";

        public const string ServiceGlobalizationRequestsReject =
            "ServiceGlobalizationRequests.Reject";

        public const string ServicesViewCentralTree =
            "Services.ViewCentralTree";

        public const string ServiceWorkflowsViewAll =
            "ServiceWorkflows.ViewAll";

        public const string ServiceWorkflowsViewDetails =
            "ServiceWorkflows.ViewDetails";

        public const string ServiceWorkflowsCreate =
            "ServiceWorkflows.Create";

        public const string ServiceWorkflowsUpdate =
            "ServiceWorkflows.Update";

        public const string ServiceWorkflowsDeactivate =
            "ServiceWorkflows.Deactivate";

        public const string ServiceWorkflowsReactivate =
            "ServiceWorkflows.Reactivate";

        public const string ServiceWorkflowsSetDefault =
            "ServiceWorkflows.SetDefault";

        public const string ServiceWorkflowsViewCandidateServices =
            "ServiceWorkflows.ViewCandidateServices";

        public const string ServiceWorkflowsViewStartOptions =
            "ServiceWorkflows.ViewStartOptions";

        public const string ServiceSchedulesView =
            "ServiceSchedules.View";

        public const string ServiceSchedulesCreate =
            "ServiceSchedules.Create";

        public const string ServiceSchedulesUpdate =
            "ServiceSchedules.Update";

        public const string DisplayWindowsViewLinked =
            "DisplayWindows.ViewLinked";

        public const string DisplayWindowsViewAvailable =
            "DisplayWindows.ViewAvailable";

        public const string DisplayWindowsAssign =
            "DisplayWindows.Assign";

        public const string DisplayWindowsUnassign =
            "DisplayWindows.Unassign";

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

            public static readonly Guid BranchAdministrator =
                Guid.Parse(
                    "20000000-0000-0000-0000-000000000002");
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

            public static readonly Guid BranchesDeactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000005");

            public static readonly Guid BranchesReactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000043");

            public static readonly Guid BranchesDeletePermanent =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000044");

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

            public static readonly Guid WaitingAreasDeactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000015");

            public static readonly Guid WaitingAreasReactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000045");

            public static readonly Guid WaitingAreasDeletePermanent =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000046");

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

            public static readonly Guid WindowsDeactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000020");

            public static readonly Guid WindowsReactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000021");

            public static readonly Guid WindowsDeletePermanent =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000022");

            public static readonly Guid TerminalsViewAll =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000023");

            public static readonly Guid TerminalsViewDetails =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000024");

            public static readonly Guid TerminalsCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000025");

            public static readonly Guid TerminalsUpdate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000026");

            public static readonly Guid TerminalsDeactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000027");

            public static readonly Guid TerminalsReactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000028");

            public static readonly Guid TerminalsDeletePermanent =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000030");

            public static readonly Guid DisplaysViewAll =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000031");

            public static readonly Guid DisplaysViewDetails =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000032");

            public static readonly Guid DisplaysCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000033");

            public static readonly Guid DisplaysUpdate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000034");

            public static readonly Guid DisplaysDeactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000035");

            public static readonly Guid DisplaysReactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000036");

            public static readonly Guid DisplaysDeletePermanent =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000038");

            public static readonly Guid DisplayWindowsViewLinked =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000039");

            public static readonly Guid DisplayWindowsViewAvailable =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000040");

            public static readonly Guid DisplayWindowsAssign =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000041");

            public static readonly Guid DisplayWindowsUnassign =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000042");

            public static readonly Guid ServicesViewAll =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000047");

            public static readonly Guid ServicesViewDetails =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000048");

            public static readonly Guid ServicesCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000049");

            public static readonly Guid ServicesUpdate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000050");

            public static readonly Guid ServicesDelete =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000051");

            public static readonly Guid ServicesRestore =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000052");

            public static readonly Guid ServiceWorkflowsViewAll =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000053");

            public static readonly Guid ServiceWorkflowsViewDetails =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000054");

            public static readonly Guid ServiceWorkflowsCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000055");

            public static readonly Guid ServiceWorkflowsUpdate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000056");

            public static readonly Guid ServiceWorkflowsDeactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000057");

            public static readonly Guid ServiceWorkflowsReactivate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000058");

            public static readonly Guid ServiceWorkflowsViewCandidateServices =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000059");

            public static readonly Guid ServiceWorkflowsViewStartOptions =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000061");

            public static readonly Guid BranchAdminsCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000062");

            public static readonly Guid BranchServicesView =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000063");

            public static readonly Guid BranchServicesAssign =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000064");

            public static readonly Guid BranchServiceTreesCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000065");

            public static readonly Guid BranchServicesUnassign =
    Guid.Parse(
        "30000000-0000-0000-0000-000000000066");

            public static readonly Guid ServiceGlobalizationRequestsViewAll =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000067");

            public static readonly Guid ServiceGlobalizationRequestsViewDetails =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000068");

            public static readonly Guid ServiceGlobalizationRequestsViewOwn =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000069");

            public static readonly Guid ServiceGlobalizationRequestsApprove =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000070");

            public static readonly Guid ServiceGlobalizationRequestsReject =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000071");

            public static readonly Guid ServicesViewCentralTree =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000072");

            public static readonly Guid ServiceWorkflowsSetDefault =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000073");

            public static readonly Guid ServiceSchedulesView =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000074");

            public static readonly Guid ServiceSchedulesCreate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000075");

            public static readonly Guid ServiceSchedulesUpdate =
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000076");
        }
    }

    public static class SeedCatalog
    {
        public static IReadOnlyCollection<RoleSeedItem> Roles { get; } =
            new[]
            {
                new RoleSeedItem(
                    SeedIds.Roles.TechnicalAdministrator,
                    RoleNames.TechnicalAdministrator),

                new RoleSeedItem(
                    SeedIds.Roles.BranchAdministrator,
                    RoleNames.BranchAdministrator)
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
                    SeedIds.Permissions.BranchesDeactivate,
                    PermissionNames.BranchesDeactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.BranchesReactivate,
                    PermissionNames.BranchesReactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.BranchesDeletePermanent,
                    PermissionNames.BranchesDeletePermanent),

                new PermissionSeedItem(
                    SeedIds.Permissions.BranchAdminsCreate,
                    PermissionNames.BranchAdminsCreate),

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
                    SeedIds.Permissions.WaitingAreasDeactivate,
                    PermissionNames.WaitingAreasDeactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.WaitingAreasReactivate,
                    PermissionNames.WaitingAreasReactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.WaitingAreasDeletePermanent,
                    PermissionNames.WaitingAreasDeletePermanent),

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
                    SeedIds.Permissions.WindowsDeactivate,
                    PermissionNames.WindowsDeactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.WindowsReactivate,
                    PermissionNames.WindowsReactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.WindowsDeletePermanent,
                    PermissionNames.WindowsDeletePermanent),

                new PermissionSeedItem(
                    SeedIds.Permissions.TerminalsViewAll,
                    PermissionNames.TerminalsViewAll),

                new PermissionSeedItem(
                    SeedIds.Permissions.TerminalsViewDetails,
                    PermissionNames.TerminalsViewDetails),

                new PermissionSeedItem(
                    SeedIds.Permissions.TerminalsCreate,
                    PermissionNames.TerminalsCreate),

                new PermissionSeedItem(
                    SeedIds.Permissions.TerminalsUpdate,
                    PermissionNames.TerminalsUpdate),

                new PermissionSeedItem(
                    SeedIds.Permissions.TerminalsDeactivate,
                    PermissionNames.TerminalsDeactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.TerminalsReactivate,
                    PermissionNames.TerminalsReactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.TerminalsDeletePermanent,
                    PermissionNames.TerminalsDeletePermanent),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplaysViewAll,
                    PermissionNames.DisplaysViewAll),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplaysViewDetails,
                    PermissionNames.DisplaysViewDetails),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplaysCreate,
                    PermissionNames.DisplaysCreate),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplaysUpdate,
                    PermissionNames.DisplaysUpdate),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplaysDeactivate,
                    PermissionNames.DisplaysDeactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplaysReactivate,
                    PermissionNames.DisplaysReactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplaysDeletePermanent,
                    PermissionNames.DisplaysDeletePermanent),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServicesViewAll,
                    PermissionNames.ServicesViewAll),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServicesViewDetails,
                    PermissionNames.ServicesViewDetails),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServicesCreate,
                    PermissionNames.ServicesCreate),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServicesUpdate,
                    PermissionNames.ServicesUpdate),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServicesDelete,
                    PermissionNames.ServicesDelete),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServicesRestore,
                    PermissionNames.ServicesRestore),

                new PermissionSeedItem(
                    SeedIds.Permissions.BranchServicesView,
                    PermissionNames.BranchServicesView),

                new PermissionSeedItem(
                    SeedIds.Permissions.BranchServicesAssign,
                    PermissionNames.BranchServicesAssign),

                new PermissionSeedItem(
                    SeedIds.Permissions.BranchServiceTreesCreate,
                    PermissionNames.BranchServiceTreesCreate),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceGlobalizationRequestsViewAll,
                    PermissionNames.ServiceGlobalizationRequestsViewAll),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceGlobalizationRequestsViewDetails,
                    PermissionNames.ServiceGlobalizationRequestsViewDetails),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceGlobalizationRequestsViewOwn,
                    PermissionNames.ServiceGlobalizationRequestsViewOwn),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceGlobalizationRequestsApprove,
                    PermissionNames.ServiceGlobalizationRequestsApprove),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceGlobalizationRequestsReject,
                    PermissionNames.ServiceGlobalizationRequestsReject),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServicesViewCentralTree,
                    PermissionNames.ServicesViewCentralTree),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceWorkflowsViewAll,
                    PermissionNames.ServiceWorkflowsViewAll),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceWorkflowsViewDetails,
                    PermissionNames.ServiceWorkflowsViewDetails),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceWorkflowsCreate,
                    PermissionNames.ServiceWorkflowsCreate),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceWorkflowsUpdate,
                    PermissionNames.ServiceWorkflowsUpdate),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceWorkflowsDeactivate,
                    PermissionNames.ServiceWorkflowsDeactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceWorkflowsReactivate,
                    PermissionNames.ServiceWorkflowsReactivate),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceWorkflowsViewCandidateServices,
                    PermissionNames.ServiceWorkflowsViewCandidateServices),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceWorkflowsViewStartOptions,
                    PermissionNames.ServiceWorkflowsViewStartOptions),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceWorkflowsSetDefault,
                    PermissionNames.ServiceWorkflowsSetDefault),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceSchedulesView,
                    PermissionNames.ServiceSchedulesView),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceSchedulesCreate,
                    PermissionNames.ServiceSchedulesCreate),

                new PermissionSeedItem(
                    SeedIds.Permissions.ServiceSchedulesUpdate,
                    PermissionNames.ServiceSchedulesUpdate),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplayWindowsViewLinked,
                    PermissionNames.DisplayWindowsViewLinked),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplayWindowsViewAvailable,
                    PermissionNames.DisplayWindowsViewAvailable),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplayWindowsAssign,
                    PermissionNames.DisplayWindowsAssign),

                new PermissionSeedItem(
                    SeedIds.Permissions.DisplayWindowsUnassign,
                    PermissionNames.DisplayWindowsUnassign),

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
                    PermissionNames.GlobalConfigurationsDelete) ,
                new PermissionSeedItem(
    SeedIds.Permissions.BranchServicesUnassign,
    PermissionNames.BranchServicesUnassign),
            };

        public static IReadOnlyCollection<RolePermissionSeedItem>
            RolePermissions
        { get; } =
            BuildRolePermissions();

        private static IReadOnlyCollection<RolePermissionSeedItem>
            BuildRolePermissions()
        {
            var technicalAdministratorPermissions = Permissions
                .Where(permission =>
                    permission.Id !=
                    SeedIds.Permissions.BranchServiceTreesCreate)
                .Select(permission =>
                    new RolePermissionSeedItem(
                        SeedIds.Roles.TechnicalAdministrator,
                        permission.Id));

            var branchAdministratorPermissions = new[]
            {
        SeedIds.Permissions.ServicesViewAll,
        SeedIds.Permissions.ServicesViewDetails,
        SeedIds.Permissions.ServicesUpdate,

        // Allow branch admins to soft-delete services
        // owned by their active branch.
        SeedIds.Permissions.ServicesDelete,

        // Allow branch admins to restore services
        // owned by their active branch.
        SeedIds.Permissions.ServicesRestore,

        SeedIds.Permissions.BranchServicesView,
        SeedIds.Permissions.BranchServicesAssign,
        SeedIds.Permissions.BranchServiceTreesCreate,
        SeedIds.Permissions.ServiceGlobalizationRequestsViewOwn,
        SeedIds.Permissions.ServiceWorkflowsViewAll,
        SeedIds.Permissions.ServiceWorkflowsViewDetails,
        SeedIds.Permissions.ServiceWorkflowsCreate,
        SeedIds.Permissions.ServiceWorkflowsUpdate,
        SeedIds.Permissions.ServiceWorkflowsDeactivate,
        SeedIds.Permissions.ServiceWorkflowsReactivate,
        SeedIds.Permissions.ServiceWorkflowsSetDefault,
        SeedIds.Permissions.ServiceWorkflowsViewCandidateServices,
        SeedIds.Permissions.ServiceWorkflowsViewStartOptions,
        SeedIds.Permissions.ServiceSchedulesView,
        SeedIds.Permissions.ServiceSchedulesCreate,
        SeedIds.Permissions.ServiceSchedulesUpdate
    }
            .Select(permissionId =>
                new RolePermissionSeedItem(
                    SeedIds.Roles.BranchAdministrator,
                    permissionId));

            return technicalAdministratorPermissions
                .Concat(branchAdministratorPermissions)
                .ToArray();
        }
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
