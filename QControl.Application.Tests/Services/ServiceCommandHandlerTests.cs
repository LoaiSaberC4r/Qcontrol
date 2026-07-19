using Qcontrol.Application.Features.Services.Command.CreateService;
using Qcontrol.Application.Features.Services.Command.DeleteService;
using Qcontrol.Application.Features.Services.Command.RestoreService;
using Qcontrol.Application.Features.Services.Command.UpdateService;
using QControl.Application.Abstraction.Services;
using QControl.Application.Tests.TestSupport;
using ServiceEntity = QControl.Domain.Entities.Service;

namespace QControl.Application.Tests.Services;

public sealed class ServiceCommandHandlerTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public async Task Create_root_ticket_issuable_service_succeeds_and_can_issue_ticket()
    {
        var services = new List<ServiceEntity>();
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<ServiceEntity>(services);
        var handler = CreateHandler(
            services,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            ValidCreate() with
            {
                ArabicName = " خدمة المرضى ",
                EnglishName = " Patient Services ",
                IsTicketIssuable = true
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(services);
        Assert.Equal("خدمة المرضى", services[0].ArabicName);
        Assert.Equal("Patient Services", services[0].EnglishName);
        Assert.True(result.Value.EffectiveIsActive);
        Assert.True(result.Value.CanIssueTicket);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_child_under_ticket_issuable_parent_is_rejected()
    {
        var parent = Service(
            1,
            isTicketIssuable: true);
        var services = new List<ServiceEntity> { parent };
        var handler = CreateHandler(
            services,
            new InMemoryWriteRepository<ServiceEntity>(services),
            new TestUnitOfWork());

        var result = await handler.Handle(
            ValidCreate() with
            {
                ParentServiceId = parent.Id
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Services.Create.ParentTicketIssuable");
    }

    [Fact]
    public async Task Create_non_ticket_issuable_service_allows_null_ticket_settings()
    {
        var services = new List<ServiceEntity>();
        var handler = CreateHandler(
            services,
            new InMemoryWriteRepository<ServiceEntity>(services),
            new TestUnitOfWork());

        var result = await handler.Handle(
            ValidCreate() with
            {
                IsTicketIssuable = false,
                RangePrefix = null,
                RangeStartNumber = null,
                RangeEndNumber = null,
                WaitingDuration = null,
                NoOfTicketCopies = null
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(services[0].RangePrefix);
        Assert.Null(services[0].RangeStartNumber);
        Assert.Null(services[0].RangeEndNumber);
        Assert.Null(services[0].WaitingDuration);
        Assert.Null(services[0].NoOfTicketCopies);
    }

    [Fact]
    public async Task Create_with_unique_service_code_trims_and_returns_code()
    {
        var services = new List<ServiceEntity>();
        var handler = CreateHandler(
            services,
            new InMemoryWriteRepository<ServiceEntity>(services),
            new TestUnitOfWork());

        var result = await handler.Handle(
            ValidCreate() with
            {
                ServiceCode = " MED-001 ",
                IsServiceCodeRequired = true
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("MED-001", services[0].ServiceCode);
        Assert.True(services[0].IsServiceCodeRequired);
        Assert.Equal("MED-001", result.Value.ServiceCode);
        Assert.True(result.Value.IsServiceCodeRequired);
    }

    [Fact]
    public async Task Create_rejects_service_code_reserved_by_soft_deleted_service()
    {
        var existing = Service(
            1,
            serviceCode: "MED-001",
            isServiceCodeRequired: true);
        existing.SoftDelete(
            DateTime.UtcNow,
            EntityTestFactory.CurrentUserId);
        var services = new List<ServiceEntity> { existing };
        var handler = CreateHandler(
            services,
            new InMemoryWriteRepository<ServiceEntity>(services),
            new TestUnitOfWork());

        var result = await handler.Handle(
            ValidCreate() with
            {
                ServiceCode = "MED-001",
                IsServiceCodeRequired = true
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.Create.ServiceCodeAlreadyExists");
    }

    [Fact]
    public async Task Update_rejects_moving_service_under_its_descendant()
    {
        var root = Service(1);
        var child = Service(2, parentServiceId: root.Id);
        var grandChild = Service(3, parentServiceId: child.Id);
        var services = new List<ServiceEntity>
        {
            root,
            child,
            grandChild
        };
        var handler = UpdateHandler(
            services,
            new InMemoryWriteRepository<ServiceEntity>(services),
            new TestUnitOfWork());

        var result = await handler.Handle(
            ValidUpdate(root.Id) with
            {
                ParentServiceId = grandChild.Id
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Services.Update.CircularHierarchy");
    }

    [Fact]
    public async Task Update_rejects_ticket_issuable_when_service_has_children()
    {
        var parent = Service(1);
        var child = Service(2, parentServiceId: parent.Id);
        var services = new List<ServiceEntity>
        {
            parent,
            child
        };
        var handler = UpdateHandler(
            services,
            new InMemoryWriteRepository<ServiceEntity>(services),
            new TestUnitOfWork());

        var result = await handler.Handle(
            ValidUpdate(parent.Id) with
            {
                IsTicketIssuable = true
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Services.Update.HasChildrenCannotBeTicketIssuable");
    }

    [Fact]
    public async Task Update_allows_service_to_keep_its_current_code()
    {
        var service = Service(
            1,
            serviceCode: "MED-001",
            isServiceCodeRequired: true);
        var services = new List<ServiceEntity> { service };
        var handler = UpdateHandler(
            services,
            new InMemoryWriteRepository<ServiceEntity>(services),
            new TestUnitOfWork());

        var result = await handler.Handle(
            ValidUpdate(service.Id) with
            {
                ServiceCode = " MED-001 ",
                IsServiceCodeRequired = true
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("MED-001", service.ServiceCode);
        Assert.True(service.IsServiceCodeRequired);
    }

    [Fact]
    public async Task Update_removes_service_code_atomically()
    {
        var service = Service(
            1,
            serviceCode: "MED-001",
            isServiceCodeRequired: true);
        var services = new List<ServiceEntity> { service };
        var handler = UpdateHandler(
            services,
            new InMemoryWriteRepository<ServiceEntity>(services),
            new TestUnitOfWork());

        var result = await handler.Handle(
            ValidUpdate(service.Id) with
            {
                ServiceCode = " ",
                IsServiceCodeRequired = false
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(service.ServiceCode);
        Assert.False(service.IsServiceCodeRequired);
    }

    [Fact]
    public async Task Delete_parent_does_not_modify_children()
    {
        var parent = Service(1);
        var child = Service(2, parentServiceId: parent.Id, isTicketIssuable: true);
        var services = new List<ServiceEntity>
        {
            parent,
            child
        };
        var writeRepository =
            new InMemoryWriteRepository<ServiceEntity>(services);
        var unitOfWork = new TestUnitOfWork();
        var handler = new DeleteServiceCommandHandler(
            new InMemoryWriteReadRepository<ServiceEntity>(services),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);

        var result = await handler.Handle(
            new DeleteServiceCommand
            {
                Id = parent.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(parent.IsDeleted);
        Assert.False(parent.IsActive);
        Assert.False(child.IsDeleted);
        Assert.True(child.IsActive);
        Assert.True(child.IsTicketIssuable);
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Restore_child_under_deleted_parent_is_allowed_but_not_effectively_active()
    {
        var parent = Service(1);
        var child = Service(2, parentServiceId: parent.Id, isTicketIssuable: true);
        parent.SoftDelete(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        child.SoftDelete(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var services = new List<ServiceEntity>
        {
            parent,
            child
        };
        var writeRepository =
            new InMemoryWriteRepository<ServiceEntity>(services);
        var unitOfWork = new TestUnitOfWork();
        var handler = new RestoreServiceCommandHandler(
            new InMemoryWriteReadRepository<ServiceEntity>(services),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);

        var result = await handler.Handle(
            new RestoreServiceCommand
            {
                Id = child.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(child.IsDeleted);
        Assert.True(child.IsActive);
        Assert.False(result.Value.EffectiveIsActive);
        Assert.False(result.Value.CanIssueTicket);
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private static CreateServiceCommand ValidCreate()
        => new()
        {
            ArabicName = "Arabic Service",
            EnglishName = "English Service",
            IsTicketIssuable = false,
            IsClientInputRequired = false,
            HasReservation = false,
            OrderNo = 1,
            Priority = 0,
            RangePrefix = "A",
            RangeStartNumber = 1,
            RangeEndNumber = 999,
            WaitingDuration = 0,
            NoOfTicketCopies = 1
        };

    private static UpdateServiceCommand ValidUpdate(int serviceId)
        => new()
        {
            Id = serviceId,
            ArabicName = $"Arabic Service {serviceId}",
            EnglishName = $"English Service {serviceId}",
            IsTicketIssuable = false,
            IsClientInputRequired = false,
            HasReservation = false,
            OrderNo = 1,
            Priority = 0,
            RangePrefix = "A",
            RangeStartNumber = 1,
            RangeEndNumber = 999,
            WaitingDuration = 0,
            NoOfTicketCopies = 1,
            RowVersion = ValidRowVersion
        };

    private static ServiceEntity Service(
        int id,
        int? parentServiceId = null,
        bool isTicketIssuable = false,
        string? serviceCode = null,
        bool isServiceCodeRequired = false)
        => EntityTestFactory.Service(
            id,
            parentServiceId,
            isTicketIssuable: isTicketIssuable,
            serviceCode: serviceCode,
            isServiceCodeRequired: isServiceCodeRequired);

    private static CreateServiceCommandHandler CreateHandler(
        List<ServiceEntity> services,
        InMemoryWriteRepository<ServiceEntity> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<ServiceEntity>(services),
            writeRepository,
            new TestServiceTicketUsageChecker(),
            new TestCurrentUser(),
            unitOfWork);

    private static UpdateServiceCommandHandler UpdateHandler(
        List<ServiceEntity> services,
        InMemoryWriteRepository<ServiceEntity> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<ServiceEntity>(services),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestServiceTicketUsageChecker(),
            new TestCurrentUser(),
            unitOfWork);

    private sealed class TestServiceTicketUsageChecker
        : IServiceTicketUsageChecker
    {
        public Task<bool> HasHistoricalTicketsAsync(
            int serviceId,
            CancellationToken cancellationToken)
            => Task.FromResult(false);
    }
}
