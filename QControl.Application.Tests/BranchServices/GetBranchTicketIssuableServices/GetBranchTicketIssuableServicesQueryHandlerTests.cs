using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.BranchServices
    .GetBranchTicketIssuableServices;

public sealed class GetBranchTicketIssuableServicesQueryHandlerTests
{
    private static readonly DateTime SundayAtTen =
        new(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Allows_an_unauthenticated_user()
    {
        var fixture = Fixture.Default();
        var handler = CreateHandler(
            fixture,
            currentUser: new TestCurrentUser
            {
                IsAuthenticated = false,
                UserId = null
            });

        var result = await Handle(handler);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Anonymous_request_does_not_apply_branch_access_policy()
    {
        var accessValidator = new TestBranchAccessValidator
        {
            Result = Result.Fail(new Error(
                "Access.ForeignBranch",
                "Forbidden",
                ErrorType.Security))
        };
        var handler = CreateHandler(
            Fixture.Default(),
            accessValidator: accessValidator);

        var result = await Handle(handler);

        Assert.True(result.IsSuccess);
        Assert.Null(accessValidator.LastBranchId);
    }

    [Fact]
    public async Task Returns_not_found_when_branch_does_not_exist()
    {
        var fixture = Fixture.Default() with { Branches = new List<Branch>() };

        var result = await Handle(CreateHandler(fixture));

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal(
            "BranchTicketIssuableServices.View.BranchNotFound",
            error.Code);
    }

    [Fact]
    public async Task Returns_conflict_when_branch_is_inactive()
    {
        var branch = EntityTestFactory.Branch(1);
        branch.Deactivate(SundayAtTen, EntityTestFactory.CurrentUserId);
        var fixture = Fixture.Default() with
        {
            Branches = new List<Branch> { branch }
        };

        var result = await Handle(CreateHandler(fixture));

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal(
            "BranchTicketIssuableServices.View.BranchInactive",
            error.Code);
        Assert.Contains("cannot currently issue tickets", error.Message);
    }

    [Fact]
    public async Task Missing_branding_returns_null()
    {
        var fixture = Fixture.Default() with
        {
            Assignments = new List<BranchService>(),
            Schedules = new List<ServiceSchedule>()
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.BranchBranding);
    }

    [Fact]
    public async Task General_brand_is_used_when_branch_has_no_branding()
    {
        var fixture = Fixture.Default() with
        {
            GeneralBrands = new List<GeneralBrand> { GeneralBrand() }
        };
        var generalBrandRepository =
            new InMemoryWriteReadRepository<GeneralBrand>(
                fixture.GeneralBrands);

        var result = await Handle(CreateHandler(
            fixture,
            generalBrandRepository: generalBrandRepository));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.BranchBranding);
        Assert.Null(result.Value.BranchBranding!.LogoUrl);
        Assert.Equal(
            "#0070C4",
            result.Value.BranchBranding.Theme.MainColor);
        Assert.Equal(
            "Default language",
            result.Value.BranchBranding.LanguageButton.Text);
        Assert.Equal(1, generalBrandRepository
            .FirstOrDefaultProjectionSpecCallCount);
    }

    [Fact]
    public async Task Branch_branding_wins_without_querying_general_brand()
    {
        var branch = EntityTestFactory.Branch(1);
        var branding = EntityTestFactory.BranchBranding(
            5,
            branch.Id);
        branding.UpdateLayout(
            new BrandingLayoutSettings { MainColor = "#0057B8" },
            EntityTestFactory.CurrentUserId);
        EntityTestFactory.AttachBranding(branch, branding);
        var fixture = Fixture.Default() with
        {
            Branches = new List<Branch> { branch },
            GeneralBrands = new List<GeneralBrand> { GeneralBrand() }
        };
        var generalBrandRepository =
            new InMemoryWriteReadRepository<GeneralBrand>(
                fixture.GeneralBrands);

        var result = await Handle(CreateHandler(
            fixture,
            generalBrandRepository: generalBrandRepository));

        Assert.Equal(
            "#0057B8",
            result.Value.BranchBranding!.Theme.MainColor);
        Assert.Null(result.Value.BranchBranding.Theme.HeaderColor);
        Assert.Equal(0, generalBrandRepository
            .FirstOrDefaultProjectionSpecCallCount);
    }

    [Fact]
    public async Task Logo_only_branch_branding_prevents_general_fallback()
    {
        var branch = EntityTestFactory.Branch(1);
        var branding = EntityTestFactory.BranchBranding(
            5,
            branch.Id,
            logoPath: "Branches/1/Logo/branch.png");
        EntityTestFactory.AttachBranding(branch, branding);
        var fixture = Fixture.Default() with
        {
            Branches = new List<Branch> { branch },
            GeneralBrands = new List<GeneralBrand> { GeneralBrand() }
        };
        var generalBrandRepository =
            new InMemoryWriteReadRepository<GeneralBrand>(
                fixture.GeneralBrands);

        var result = await Handle(CreateHandler(
            fixture,
            generalBrandRepository: generalBrandRepository));

        Assert.Equal(
            "/Media/Branches/1/Logo/branch.png",
            result.Value.BranchBranding!.LogoUrl);
        Assert.Null(result.Value.BranchBranding.Theme.MainColor);
        Assert.Equal(0, generalBrandRepository
            .FirstOrDefaultProjectionSpecCallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task General_brand_is_returned_when_services_are_unavailable(
        bool removeAssignments)
    {
        var fixture = Fixture.Default() with
        {
            GeneralBrands = new List<GeneralBrand> { GeneralBrand() },
            Assignments = removeAssignments
                ? new List<BranchService>()
                : Fixture.Default().Assignments,
            Schedules = removeAssignments
                ? Fixture.Default().Schedules
                : new List<ServiceSchedule>()
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.NotNull(result.Value.BranchBranding);
        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Configured_branding_maps_logo_and_theme_once_at_root()
    {
        var branch = EntityTestFactory.Branch(1);
        var branding = EntityTestFactory.BranchBranding(
            5,
            branch.Id,
            logoPath: "Media\\Branches\\1\\Logo\\branch.png",
            mainColor: "#0057B8",
            secondaryColor: "#F4B400",
            backgroundColor: "#FFFFFF");
        branding.UpdateLayout(
            new BrandingLayoutSettings
            {
                MainColor = "#0057B8",
                SecondaryColor = "#F4B400",
                BackgroundColor = "#FFFFFF",
                HeaderColor = "#111111",
                FooterColor = "#222222",
                MainTextColor = "#333333",
                ShowLanguagePage = true,
                DefaultLanguageIsArabic = true,
                ShowServiceNavigationPath = true,
                AllowRequestMoreServices = true,
                LanguageButtonBackgroundColor = "#444444",
                LanguageButtonTextColor = "#555555",
                LanguageButtonWidth = 40m,
                LanguageButtonHeight = 22m,
                LanguageButtonText = "اختيار اللغة",
                ServiceButtonBackgroundColor = "#666666",
                ServiceButtonTextColor = "#777777",
                ServiceButtonWidth = 40m,
                ServiceButtonHeight = 20m,
                ServiceButtonSpace = 2m,
                ServiceButtonFontSize = 1.8m,
                ServiceButtonText = "اختيار الخدمة",
                KeypadButtonWidth = 45m,
                KeypadButtonHeight = 10m,
                KeypadButtonText = "تأكيد",
                FooterButtonWidth = 10m,
                FooterButtonHeight = 12m,
                FooterButtonText = "رجوع"
            },
            EntityTestFactory.CurrentUserId);
        EntityTestFactory.AttachBranding(branch, branding);
        var fixture = Fixture.Default() with
        {
            Branches = new List<Branch> { branch }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.BranchBranding);
        Assert.Equal(
            "/Media/Branches/1/Logo/branch.png",
            result.Value.BranchBranding!.LogoUrl);
        Assert.Equal("#0057B8", result.Value.BranchBranding.Theme.MainColor);
        Assert.Equal(
            "#F4B400",
            result.Value.BranchBranding.Theme.SecondaryColor);
        Assert.Equal(
            "#FFFFFF",
            result.Value.BranchBranding.Theme.BackgroundColor);
        Assert.Equal("#111111", result.Value.BranchBranding.Theme.HeaderColor);
        Assert.True(result.Value.BranchBranding.Behavior.ShowLanguagePage);
        Assert.True(
            result.Value.BranchBranding.Behavior.DefaultLanguageIsArabic);
        Assert.Equal(
            "اختيار اللغة",
            result.Value.BranchBranding.LanguageButton.Text);
        Assert.Equal(1.8m, result.Value.BranchBranding.ServiceButton.FontSize);
        Assert.Equal("تأكيد", result.Value.BranchBranding.KeypadButton.Text);
        Assert.Equal("رجوع", result.Value.BranchBranding.FooterButton.Text);
    }

    [Fact]
    public async Task Branding_colors_are_returned_when_logo_is_missing()
    {
        var branch = EntityTestFactory.Branch(1);
        var branding = EntityTestFactory.BranchBranding(
            5,
            branch.Id,
            mainColor: "#0057B8",
            secondaryColor: "#F4B400",
            backgroundColor: "#FFFFFF");
        EntityTestFactory.AttachBranding(branch, branding);
        var fixture = Fixture.Default() with
        {
            Branches = new List<Branch> { branch }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.NotNull(result.Value.BranchBranding);
        Assert.Null(result.Value.BranchBranding!.LogoUrl);
        Assert.Equal("#0057B8", result.Value.BranchBranding.Theme.MainColor);
    }

    [Fact]
    public async Task Logo_only_branding_is_returned_when_services_are_empty()
    {
        var branch = EntityTestFactory.Branch(1);
        var branding = EntityTestFactory.BranchBranding(
            5,
            branch.Id,
            logoPath: "Branches/1/Logo/branch.png");
        EntityTestFactory.AttachBranding(branch, branding);
        var fixture = Fixture.Default() with
        {
            Branches = new List<Branch> { branch },
            Assignments = new List<BranchService>(),
            Schedules = new List<ServiceSchedule>()
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.BranchBranding);
        Assert.Equal(
            "/Media/Branches/1/Logo/branch.png",
            result.Value.BranchBranding!.LogoUrl);
        Assert.Null(result.Value.BranchBranding.Theme.MainColor);
        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Branding_is_returned_when_no_schedule_is_available()
    {
        var branch = EntityTestFactory.Branch(1);
        var branding = EntityTestFactory.BranchBranding(
            5,
            branch.Id,
            mainColor: "#0057B8",
            secondaryColor: "#F4B400",
            backgroundColor: "#FFFFFF");
        EntityTestFactory.AttachBranding(branch, branding);
        var fixture = Fixture.Default() with
        {
            Branches = new List<Branch> { branch },
            Schedules = new List<ServiceSchedule>()
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.NotNull(result.Value.BranchBranding);
        Assert.Empty(result.Value.Services);
    }

    [Theory]
    [InlineData(8, 59, false)]
    [InlineData(9, 0, true)]
    [InlineData(11, 59, true)]
    [InlineData(12, 0, false)]
    public async Task Schedule_uses_start_inclusive_end_exclusive_interval(
        int hour,
        int minute,
        bool expected)
    {
        var fixture = Fixture.Default(
            timeSlots: new[]
            {
                Slot(DayOfWeek.Sunday, 9, 12)
            });
        var now = new DateTime(
            2026,
            8,
            2,
            hour,
            minute,
            0,
            DateTimeKind.Utc);

        var result = await Handle(CreateHandler(
            fixture,
            dateTimeProvider: new CountingDateTimeProvider(now)));

        Assert.Equal(expected, result.Value.Services.Count == 1);
    }

    [Fact]
    public async Task Missing_configuration_uses_zero_allowed_time()
    {
        var fixture = Fixture.Default(
            timeSlots: new[]
            {
                Slot(DayOfWeek.Sunday, 10, 20, 11, 0)
            });

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Theory]
    [InlineData(0, 9, 0, 11, 0, true)]
    [InlineData(30, 10, 20, 11, 0, true)]
    [InlineData(30, 10, 45, 11, 30, false)]
    [InlineData(30, 8, 0, 9, 59, false)]
    [InlineData(30, 10, 30, 11, 0, true)]
    public async Task Allowed_time_extends_only_the_same_day_schedule_window(
        int allowedMinutes,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute,
        bool expected)
    {
        var fixture = Fixture.Default(
            timeSlots: new[]
            {
                Slot(
                    DayOfWeek.Sunday,
                    startHour,
                    startMinute,
                    endHour,
                    endMinute)
            });
        AttachConfiguration(
            fixture,
            TimeSpan.FromMinutes(allowedMinutes));

        var result = await Handle(CreateHandler(fixture));

        Assert.Equal(expected, result.Value.Services.Count == 1);
    }

    [Fact]
    public async Task Midnight_window_includes_same_day_slot_before_day_end()
    {
        var fixture = Fixture.Default(
            timeSlots: new[]
            {
                Slot(DayOfWeek.Sunday, 23, 55, 23, 59)
            });
        AttachConfiguration(fixture, TimeSpan.FromMinutes(30));

        var result = await Handle(CreateHandler(
            fixture,
            dateTimeProvider: new CountingDateTimeProvider(
                new DateTime(
                    2026,
                    8,
                    2,
                    23,
                    50,
                    0,
                    DateTimeKind.Utc))));

        Assert.Single(result.Value.Services);
    }

    [Fact]
    public async Task Midnight_window_does_not_evaluate_tomorrow()
    {
        var fixture = Fixture.Default(
            timeSlots: new[]
            {
                Slot(DayOfWeek.Monday, 0, 5, 0, 20)
            });
        AttachConfiguration(fixture, TimeSpan.FromMinutes(30));

        var result = await Handle(CreateHandler(
            fixture,
            dateTimeProvider: new CountingDateTimeProvider(
                new DateTime(
                    2026,
                    8,
                    2,
                    23,
                    50,
                    0,
                    DateTimeKind.Utc))));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Excludes_schedule_for_another_day()
    {
        var fixture = Fixture.Default(
            timeSlots: new[] { Slot(DayOfWeek.Monday, 9, 12) });

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Includes_service_when_any_one_of_multiple_slots_matches()
    {
        var fixture = Fixture.Default(
            timeSlots: new[]
            {
                Slot(DayOfWeek.Sunday, 7, 8),
                Slot(DayOfWeek.Sunday, 10, 11),
                Slot(DayOfWeek.Sunday, 14, 15)
            });

        var result = await Handle(CreateHandler(fixture));

        Assert.Single(result.Value.Services);
    }

    [Fact]
    public async Task Adjacent_slots_have_only_one_logical_match_at_boundary()
    {
        var fixture = Fixture.Default(
            timeSlots: new[]
            {
                Slot(DayOfWeek.Sunday, 9, 10),
                Slot(DayOfWeek.Sunday, 10, 12)
            });

        var result = await Handle(CreateHandler(fixture));

        Assert.Single(result.Value.Services);
    }

    [Fact]
    public async Task Excludes_service_without_schedule()
    {
        var fixture = Fixture.Default() with
        {
            Schedules = new List<ServiceSchedule>()
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Excludes_schedule_belonging_to_another_branch()
    {
        var fixture = Fixture.Default() with
        {
            Schedules = new List<ServiceSchedule>
            {
                EntityTestFactory.ServiceSchedule(
                    1,
                    branchId: 2,
                    serviceId: 10,
                    timeSlots: new[]
                    {
                        Slot(DayOfWeek.Sunday, 9, 12)
                    })
            }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Uses_one_stable_time_provider_value_per_request()
    {
        var provider = new CountingDateTimeProvider(SundayAtTen);

        var result = await Handle(CreateHandler(
            Fixture.Default(),
            dateTimeProvider: provider));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, provider.AccessCount);
    }

    [Fact]
    public async Task Includes_assigned_active_ticket_issuing_global_leaf()
    {
        var result = await Handle(CreateHandler(Fixture.Default()));

        var leaf = Assert.Single(result.Value.Services);
        Assert.Null(result.Value.BranchBranding);
        Assert.Equal(10, leaf.ServiceId);
        Assert.True(leaf.IsTicketIssuable);
        Assert.Empty(leaf.Children);
    }

    [Fact]
    public async Task Excludes_unassigned_service_even_when_currently_scheduled()
    {
        var fixture = Fixture.Default() with
        {
            Assignments = new List<BranchService>()
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Excludes_inactive_service()
    {
        var service = EntityTestFactory.Service(10, isTicketIssuable: true);
        EntityTestFactory.SetServiceActive(service, false);
        var fixture = Fixture.Default() with
        {
            Services = new List<Service> { service }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Excludes_deleted_service()
    {
        var service = EntityTestFactory.Service(10, isTicketIssuable: true);
        service.SoftDelete(SundayAtTen, EntityTestFactory.CurrentUserId);
        var fixture = Fixture.Default() with
        {
            Services = new List<Service> { service }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Excludes_non_ticket_issuing_leaf()
    {
        var fixture = Fixture.Default() with
        {
            Services = new List<Service>
            {
                EntityTestFactory.Service(10, isTicketIssuable: false)
            }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Excludes_ticket_issuing_service_that_has_children()
    {
        var fixture = Fixture.Default() with
        {
            Services = new List<Service>
            {
                EntityTestFactory.Service(10, isTicketIssuable: true),
                EntityTestFactory.Service(11, parentServiceId: 10)
            }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Excludes_leaf_when_parent_is_inactive()
    {
        var parent = EntityTestFactory.Service(9);
        EntityTestFactory.SetServiceActive(parent, false);
        var fixture = Fixture.Default() with
        {
            Services = new List<Service>
            {
                parent,
                EntityTestFactory.Service(
                    10,
                    parentServiceId: 9,
                    isTicketIssuable: true)
            }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Excludes_leaf_when_ancestor_is_deleted()
    {
        var ancestor = EntityTestFactory.Service(8);
        ancestor.SoftDelete(SundayAtTen, EntityTestFactory.CurrentUserId);
        var fixture = Fixture.Default() with
        {
            Services = new List<Service>
            {
                ancestor,
                EntityTestFactory.Service(9, parentServiceId: 8),
                EntityTestFactory.Service(
                    10,
                    parentServiceId: 9,
                    isTicketIssuable: true)
            }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Empty(result.Value.Services);
    }

    [Fact]
    public async Task Includes_owned_branch_scoped_service_and_excludes_foreign_one()
    {
        var fixture = Fixture.Default() with
        {
            Services = new List<Service>
            {
                EntityTestFactory.BranchScopedService(
                    10,
                    ownerBranchId: 1,
                    isTicketIssuable: true),
                EntityTestFactory.BranchScopedService(
                    11,
                    ownerBranchId: 2,
                    isTicketIssuable: true)
            },
            Assignments = new List<BranchService>
            {
                EntityTestFactory.BranchService(1, 1, 10),
                EntityTestFactory.BranchService(2, 1, 11)
            },
            Schedules = new List<ServiceSchedule>
            {
                Schedule(1, 10),
                Schedule(2, 11)
            }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Equal(new[] { 10 }, result.Value.Services.Select(x => x.ServiceId));
    }

    [Fact]
    public async Task Builds_pruned_tree_and_merges_shared_parent_paths()
    {
        var fixture = Fixture.Default() with
        {
            Services = new List<Service>
            {
                EntityTestFactory.Service(1),
                EntityTestFactory.Service(2, parentServiceId: 1),
                EntityTestFactory.Service(
                    3,
                    parentServiceId: 2,
                    isTicketIssuable: true),
                EntityTestFactory.Service(
                    4,
                    parentServiceId: 2,
                    isTicketIssuable: true),
                EntityTestFactory.Service(
                    5,
                    parentServiceId: 1,
                    isTicketIssuable: true)
            },
            Assignments = new List<BranchService>
            {
                EntityTestFactory.BranchService(1, 1, 3),
                EntityTestFactory.BranchService(2, 1, 4),
                EntityTestFactory.BranchService(3, 1, 5)
            },
            Schedules = new List<ServiceSchedule>
            {
                Schedule(1, 3),
                Schedule(2, 4)
            }
        };

        var result = await Handle(CreateHandler(fixture));

        var root = Assert.Single(result.Value.Services);
        Assert.Equal(1, root.ServiceId);
        var parent = Assert.Single(root.Children);
        Assert.Equal(2, parent.ServiceId);
        Assert.Equal(new[] { 3, 4 }, parent.Children.Select(x => x.ServiceId));
    }

    [Fact]
    public async Task Duplicate_assignments_and_schedule_rows_do_not_duplicate_nodes()
    {
        var fixture = Fixture.Default() with
        {
            Assignments = new List<BranchService>
            {
                EntityTestFactory.BranchService(1, 1, 10),
                EntityTestFactory.BranchService(2, 1, 10)
            },
            Schedules = new List<ServiceSchedule>
            {
                Schedule(1, 10),
                Schedule(2, 10)
            }
        };

        var result = await Handle(CreateHandler(fixture));

        Assert.Single(result.Value.Services);
    }

    [Fact]
    public async Task Stops_before_hierarchy_and_schedule_queries_when_unassigned()
    {
        var fixture = Fixture.Default() with
        {
            Assignments = new List<BranchService>()
        };
        var serviceRepository =
            new InMemoryWriteReadRepository<Service>(fixture.Services);
        var scheduleRepository =
            new InMemoryWriteReadRepository<ServiceSchedule>(fixture.Schedules);
        var handler = new GetBranchTicketIssuableServicesQueryHandler(
            new InMemoryWriteReadRepository<Branch>(fixture.Branches),
            new InMemoryWriteReadRepository<BranchService>(fixture.Assignments),
            serviceRepository,
            scheduleRepository,
            new InMemoryWriteReadRepository<GeneralBrand>(
                fixture.GeneralBrands),
            new TestCurrentUser(),
            new TestBranchAccessValidator(),
            new CountingDateTimeProvider(SundayAtTen));

        var result = await Handle(handler);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, serviceRepository.ListProjectionSpecCallCount);
        Assert.Equal(0, scheduleRepository.ListProjectionSpecCallCount);
    }

    private static Task<Result<GetBranchTicketIssuableServicesResponse>> Handle(
        GetBranchTicketIssuableServicesQueryHandler handler) =>
        handler.Handle(
            new GetBranchTicketIssuableServicesQuery { BranchId = 1 },
            CancellationToken.None);

    private static GetBranchTicketIssuableServicesQueryHandler CreateHandler(
        Fixture fixture,
        TestCurrentUser? currentUser = null,
        TestBranchAccessValidator? accessValidator = null,
        IDateTimeProvider? dateTimeProvider = null,
        InMemoryWriteReadRepository<GeneralBrand>?
            generalBrandRepository = null) =>
        new(
            new InMemoryWriteReadRepository<Branch>(fixture.Branches),
            new InMemoryWriteReadRepository<BranchService>(fixture.Assignments),
            new InMemoryWriteReadRepository<Service>(fixture.Services),
            new InMemoryWriteReadRepository<ServiceSchedule>(fixture.Schedules),
            generalBrandRepository ??
                new InMemoryWriteReadRepository<GeneralBrand>(
                    fixture.GeneralBrands),
            currentUser ?? new TestCurrentUser(),
            accessValidator ?? new TestBranchAccessValidator(),
            dateTimeProvider ?? new CountingDateTimeProvider(SundayAtTen));

    private static GeneralBrand GeneralBrand() =>
        EntityTestFactory.GeneralBrand(
            1,
            new BrandingLayoutSettings
            {
                MainColor = "#0070c4",
                SecondaryColor = "#ffffff",
                BackgroundColor = "#f0f0f0",
                HeaderColor = "#111111",
                ShowLanguagePage = true,
                LanguageButtonText = "Default language"
            });

    private static ServiceSchedule Schedule(int id, int serviceId) =>
        EntityTestFactory.ServiceSchedule(
            id,
            branchId: 1,
            serviceId,
            timeSlots: new[] { Slot(DayOfWeek.Sunday, 9, 12) });

    private static ServiceScheduleTimeSlotDefinition Slot(
        DayOfWeek day,
        int startHour,
        int endHour) =>
        new(day, new TimeOnly(startHour, 0), new TimeOnly(endHour, 0));

    private static ServiceScheduleTimeSlotDefinition Slot(
        DayOfWeek day,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute) =>
        new(
            day,
            new TimeOnly(startHour, startMinute),
            new TimeOnly(endHour, endMinute));

    private static void AttachConfiguration(
        Fixture fixture,
        TimeSpan allowedTime)
    {
        var branch = Assert.Single(fixture.Branches);
        var configuration = EntityTestFactory.BranchConfiguration(
            1,
            branch.Id,
            allowedTime,
            new byte[8]);
        EntityTestFactory.AttachConfiguration(branch, configuration);
    }

    private sealed record Fixture(
        List<Branch> Branches,
        List<Service> Services,
        List<BranchService> Assignments,
        List<ServiceSchedule> Schedules,
        List<GeneralBrand> GeneralBrands)
    {
        public static Fixture Default(
            IReadOnlyCollection<ServiceScheduleTimeSlotDefinition>? timeSlots = null)
        {
            var service = EntityTestFactory.Service(
                10,
                isTicketIssuable: true);

            return new Fixture(
                new List<Branch> { EntityTestFactory.Branch(1) },
                new List<Service> { service },
                new List<BranchService>
                {
                    EntityTestFactory.BranchService(1, 1, service.Id)
                },
                new List<ServiceSchedule>
                {
                    EntityTestFactory.ServiceSchedule(
                        1,
                        branchId: 1,
                        serviceId: service.Id,
                        timeSlots: timeSlots ?? new[]
                        {
                            Slot(DayOfWeek.Sunday, 9, 12)
                        })
                },
                new List<GeneralBrand>());
        }
    }

    private sealed class CountingDateTimeProvider : IDateTimeProvider
    {
        private readonly DateTime _utcNow;

        public CountingDateTimeProvider(DateTime utcNow)
        {
            _utcNow = utcNow;
        }

        public int AccessCount { get; private set; }

        public DateTime UtcNow
        {
            get
            {
                AccessCount++;
                return _utcNow;
            }
        }
    }
}
