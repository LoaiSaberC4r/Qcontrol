using BuildingBlock.Application.Abstraction.Media;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Qcontrol.Application.Features.Services.Command.PermanentDeleteService;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Abstraction.Services;
using QControl.Application.Shared.Operational;
using QControl.Application.Shared.Security;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Services;

public sealed class PermanentDeleteServiceTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public void Validator_rejects_invalid_id_and_row_version()
    {
        var validator = new PermanentDeleteServiceCommandValidator();

        var result = validator.TestValidate(
            new PermanentDeleteServiceCommand
            {
                Id = 0,
                RowVersion = "not-base64"
            });

        result.ShouldHaveValidationErrorFor(command => command.Id);
        result.ShouldHaveValidationErrorFor(command => command.RowVersion);
    }

    [Fact]
    public void Validator_requires_row_version()
    {
        var validator = new PermanentDeleteServiceCommandValidator();

        var result = validator.TestValidate(
            new PermanentDeleteServiceCommand
            {
                Id = 1,
                RowVersion = string.Empty
            });

        result.ShouldHaveValidationErrorFor(command => command.RowVersion);
    }

    [Fact]
    public async Task Unauthenticated_user_is_rejected_before_data_access()
    {
        var fixture = new Fixture(
            currentUser: new TestCurrentUser
            {
                IsAuthenticated = false,
                UserId = null
            });

        var result = await fixture.Handler.Handle(
            Command(1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.Unauthenticated");
        Assert.Equal(0, fixture.TicketUsageChecker.CallCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Missing_service_returns_not_found()
    {
        var fixture = new Fixture();

        var result = await fixture.Handler.Handle(
            Command(404),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.ServiceNotFound");
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Global_service_is_rejected()
    {
        var service = EntityTestFactory.Service(1);
        service.SoftDelete(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var fixture = new Fixture(services: new List<Service> { service });

        var result = await fixture.Handler.Handle(
            Command(service.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.GlobalServiceNotSupported");
        Assert.Contains(service, fixture.Services);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Another_branch_service_is_rejected_as_ownership_mismatch()
    {
        var service = SoftDeletedBranchService(1, ownerBranchId: 7);
        var accessValidator = new ServiceDefinitionAccessValidator(
            new TestCurrentBranchContext
            {
                UserType = UserType.BranchAdmin,
                ActiveBranchId = 8
            });
        var fixture = new Fixture(
            services: new List<Service> { service },
            accessValidator: accessValidator);

        var result = await fixture.Handler.Handle(
            Command(service.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.OwnershipMismatch");
        Assert.Equal(0, fixture.TicketUsageChecker.CallCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Active_branch_scoped_service_must_be_soft_deleted_first()
    {
        var service = EntityTestFactory.BranchScopedService(1, 7);
        var fixture = new Fixture(services: new List<Service> { service });

        var result = await fixture.Handler.Handle(
            Command(service.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.MustBeSoftDeletedFirst");
        Assert.Equal(0, fixture.TicketUsageChecker.CallCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Historical_tickets_block_delete_and_are_checked_once()
    {
        var service = SoftDeletedBranchService(1, 7);
        var fixture = new Fixture(
            services: new List<Service> { service },
            hasHistoricalTickets: true);

        var result = await fixture.Handler.Handle(
            Command(service.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.HasHistoricalTickets");
        Assert.Equal(1, fixture.TicketUsageChecker.CallCount);
        Assert.Equal(0, fixture.PermanentDeleteRepository.CallCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Child_service_blocks_delete_without_cascade_or_reparenting()
    {
        var parent = SoftDeletedBranchService(1, 7);
        var child = EntityTestFactory.BranchScopedService(
            2,
            7,
            parentServiceId: parent.Id);
        var fixture = new Fixture(
            services: new List<Service> { parent, child });

        var result = await fixture.Handler.Handle(
            Command(parent.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.HasChildren");
        Assert.Contains(parent, fixture.Services);
        Assert.Contains(child, fixture.Services);
        Assert.Equal(parent.Id, child.ParentServiceId);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Workflow_reference_blocks_delete()
    {
        var service = SoftDeletedBranchService(1, 7);
        var workflow = ServiceWorkflow.Create(
            branchId: 7,
            leafServiceId: service.Id,
            arabicName: null,
            englishName: null,
            isDefault: false,
            steps: Array.Empty<ServiceWorkflowStepData>(),
            createdByApplicationUserId: EntityTestFactory.CurrentUserId);
        var fixture = new Fixture(
            services: new List<Service> { service },
            workflows: new List<ServiceWorkflow> { workflow });

        var result = await fixture.Handler.Handle(
            Command(service.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.HasWorkflowReferences");
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Pending_globalization_request_blocks_delete()
    {
        var service = SoftDeletedBranchService(1, 7);
        var globalizationRequest = ServiceGlobalizationRequest.Create(
            branchId: 7,
            rootServiceId: service.Id,
            requestType: ServiceGlobalizationRequestType.BranchServiceTree,
            requestedByApplicationUserId: EntityTestFactory.CurrentUserId,
            requestedOnUtc: DateTime.UtcNow);
        var fixture = new Fixture(
            services: new List<Service> { service },
            globalizationRequests:
                new List<ServiceGlobalizationRequest>
                {
                    globalizationRequest
                });

        var result = await fixture.Handler.Handle(
            Command(service.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.HasPendingGlobalizationRequest");
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Eligible_service_removes_owned_configuration_and_commits_once()
    {
        var service = SoftDeletedBranchService(1, 7);
        var assignment = EntityTestFactory.BranchService(1, 7, service.Id);
        var schedule = EntityTestFactory.ServiceSchedule(1, 7, service.Id);
        var image = EntityTestFactory.ServiceImage(
            1,
            service.Id,
            "services/1/logo.png");
        var command = Command(service.Id);
        var fixture = new Fixture(
            services: new List<Service> { service },
            assignments: new List<BranchService> { assignment },
            schedules: new List<ServiceSchedule> { schedule },
            images: new List<ServiceImage> { image });

        var result = await fixture.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(service.Id, result.Value.ServiceId);
        Assert.Empty(fixture.Services);
        Assert.Empty(fixture.Assignments);
        Assert.Empty(fixture.Schedules);
        Assert.Empty(fixture.Images);
        Assert.Equal(1, fixture.TicketUsageChecker.CallCount);
        Assert.Equal(1, fixture.PermanentDeleteRepository.CallCount);
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCallCount);
        Assert.Equal(1, fixture.UnitOfWork.CommitTransactionCallCount);
        Assert.Equal(0, fixture.UnitOfWork.RollbackTransactionCallCount);
        Assert.Equal(
            1,
            fixture.MediaService.CommitCountAtRemove);
        Assert.Contains(
            "services/1/logo.png",
            fixture.MediaService.RemovedPaths);
        Assert.Contains(
            OperationalCacheTags.BranchServicesForBranch(7),
            command.Tags);
        Assert.Contains(
            OperationalCacheTags.ServiceSchedule(7, service.Id),
            command.Tags);
    }

    [Fact]
    public async Task Stale_row_version_rolls_back_as_concurrency_conflict()
    {
        var service = SoftDeletedBranchService(1, 7);
        var fixture = new Fixture(services: new List<Service> { service });
        fixture.PermanentDeleteRepository.ExceptionToThrow =
            new ServicePermanentDeleteConcurrencyException();

        var result = await fixture.Handler.Handle(
            Command(service.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.ConcurrencyConflict");
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCallCount);
        Assert.Equal(1, fixture.UnitOfWork.RollbackTransactionCallCount);
    }

    [Fact]
    public async Task Unsupported_dependency_is_mapped_to_conflict()
    {
        var service = SoftDeletedBranchService(1, 7);
        var fixture = new Fixture(services: new List<Service> { service });
        fixture.PermanentDeleteRepository.ExceptionToThrow =
            new ServicePermanentDeleteConflictException(
                new InvalidOperationException("FK conflict"));

        var result = await fixture.Handler.Handle(
            Command(service.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "Services.PermanentDelete.HasDependencies");
        Assert.Equal(1, fixture.UnitOfWork.RollbackTransactionCallCount);
    }

    [Fact]
    public async Task Media_cleanup_failure_does_not_change_committed_success()
    {
        var service = SoftDeletedBranchService(1, 7);
        var image = EntityTestFactory.ServiceImage(
            1,
            service.Id,
            "services/1/icon.png",
            ServiceImageType.Icon);
        var fixture = new Fixture(
            services: new List<Service> { service },
            images: new List<ServiceImage> { image });
        fixture.MediaService.ExceptionToThrow =
            new IOException("storage unavailable");

        var result = await fixture.Handler.Handle(
            Command(service.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(fixture.Services);
        Assert.Equal(1, fixture.UnitOfWork.CommitTransactionCallCount);
        Assert.Equal(1, fixture.MediaService.RemoveRangeCallCount);
    }

    private static PermanentDeleteServiceCommand Command(int serviceId)
        => new()
        {
            Id = serviceId,
            RowVersion = ValidRowVersion
        };

    private static Service SoftDeletedBranchService(
        int serviceId,
        int ownerBranchId)
    {
        var service = EntityTestFactory.BranchScopedService(
            serviceId,
            ownerBranchId);
        service.SoftDelete(
            DateTime.UtcNow,
            EntityTestFactory.CurrentUserId);
        return service;
    }

    private sealed class Fixture
    {
        public Fixture(
            List<Service>? services = null,
            List<BranchService>? assignments = null,
            List<ServiceSchedule>? schedules = null,
            List<ServiceImage>? images = null,
            List<ServiceWorkflow>? workflows = null,
            List<ServiceWorkflowStep>? workflowSteps = null,
            List<ServiceGlobalizationRequest>? globalizationRequests = null,
            List<ServiceGlobalizationRequestItem>?
                globalizationRequestItems = null,
            bool hasHistoricalTickets = false,
            BuildingBlock.Application.Abstraction.Security.ICurrentUser?
                currentUser = null,
            IServiceDefinitionAccessValidator? accessValidator = null)
        {
            Services = services ?? new List<Service>();
            Assignments = assignments ?? new List<BranchService>();
            Schedules = schedules ?? new List<ServiceSchedule>();
            Images = images ?? new List<ServiceImage>();
            UnitOfWork = new TestUnitOfWork();
            TicketUsageChecker = new TestTicketUsageChecker
            {
                HasHistoricalTickets = hasHistoricalTickets
            };
            PermanentDeleteRepository =
                new TestServicePermanentDeleteRepository(Services);
            MediaService = new TestMediaService(
                () => UnitOfWork.CommitTransactionCallCount);

            Handler = new PermanentDeleteServiceCommandHandler(
                new InMemoryWriteReadRepository<Service>(Services),
                new InMemoryWriteReadRepository<BranchService>(Assignments),
                new InMemoryWriteRepository<BranchService>(Assignments),
                new InMemoryWriteReadRepository<ServiceSchedule>(Schedules),
                new InMemoryWriteRepository<ServiceSchedule>(Schedules),
                new InMemoryWriteReadRepository<ServiceImage>(Images),
                new InMemoryWriteRepository<ServiceImage>(Images),
                new InMemoryWriteReadRepository<ServiceWorkflow>(
                    workflows ?? new List<ServiceWorkflow>()),
                new InMemoryWriteReadRepository<ServiceWorkflowStep>(
                    workflowSteps ?? new List<ServiceWorkflowStep>()),
                new InMemoryWriteReadRepository<ServiceGlobalizationRequest>(
                    globalizationRequests ??
                        new List<ServiceGlobalizationRequest>()),
                new InMemoryWriteReadRepository<ServiceGlobalizationRequestItem>(
                    globalizationRequestItems ??
                        new List<ServiceGlobalizationRequestItem>()),
                PermanentDeleteRepository,
                TicketUsageChecker,
                accessValidator ??
                    AllowAllServiceDefinitionAccessValidator.Instance,
                new TestConcurrencyTokenManager(),
                currentUser ?? new TestCurrentUser(),
                UnitOfWork,
                MediaService,
                NullLogger<PermanentDeleteServiceCommandHandler>.Instance);
        }

        public List<Service> Services { get; }

        public List<BranchService> Assignments { get; }

        public List<ServiceSchedule> Schedules { get; }

        public List<ServiceImage> Images { get; }

        public TestUnitOfWork UnitOfWork { get; }

        public TestTicketUsageChecker TicketUsageChecker { get; }

        public TestServicePermanentDeleteRepository
            PermanentDeleteRepository { get; }

        public TestMediaService MediaService { get; }

        public PermanentDeleteServiceCommandHandler Handler { get; }
    }

    private sealed class TestTicketUsageChecker
        : IServiceTicketUsageChecker
    {
        public bool HasHistoricalTickets { get; init; }

        public int CallCount { get; private set; }

        public Task<bool> HasHistoricalTicketsAsync(
            int serviceId,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(HasHistoricalTickets);
        }
    }

    private sealed class TestServicePermanentDeleteRepository
        : IServicePermanentDeleteRepository
    {
        private readonly List<Service> _services;

        public TestServicePermanentDeleteRepository(List<Service> services)
        {
            _services = services;
        }

        public int CallCount { get; private set; }

        public Exception? ExceptionToThrow { get; set; }

        public Task DeletePermanentlyAsync(
            int serviceId,
            byte[] rowVersion,
            CancellationToken cancellationToken)
        {
            CallCount++;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            var service = _services.Single(item => item.Id == serviceId);
            _services.Remove(service);
            return Task.CompletedTask;
        }
    }

    private sealed class TestMediaService : IMediaService
    {
        private readonly Func<int> _commitCount;

        public TestMediaService(Func<int> commitCount)
        {
            _commitCount = commitCount;
        }

        public int RemoveRangeCallCount { get; private set; }

        public int? CommitCountAtRemove { get; private set; }

        public List<string> RemovedPaths { get; } = new();

        public Exception? ExceptionToThrow { get; set; }

        public void Remove(string filePath)
            => throw new NotSupportedException();

        public void RemoveRange(IEnumerable<string> filePaths)
        {
            RemoveRangeCallCount++;
            CommitCountAtRemove = _commitCount();
            RemovedPaths.AddRange(filePaths);

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }
        }

        public Task<string> SaveAsync(
            IFormFile mediaFile,
            string folderName)
            => throw new NotSupportedException();

        public Task<List<string>> SaveAsync(
            List<IFormFile> formFiles,
            string folderName)
            => throw new NotSupportedException();

        public Task<Stream> GetStream(IFormFile formFile)
            => throw new NotSupportedException();

        public Task<string> SaveVideoAsync(
            IFormFile videoFile,
            string folderName)
            => throw new NotSupportedException();
    }
}
