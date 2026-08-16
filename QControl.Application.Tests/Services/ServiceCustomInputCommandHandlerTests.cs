using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Command.CreateService;
using Qcontrol.Application.Features.Services.Command.UpdateService;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Services;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Services;

public sealed class ServiceCustomInputCommandHandlerTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public async Task Create_leaf_with_valid_string_input_persists_once()
    {
        var services = new List<Service>();
        var unitOfWork = new TestUnitOfWork();
        var result = await CreateHandler(services, unitOfWork).Handle(
            ValidCreate() with
            {
                IsClientInputRequired = true,
                CustomInputs = new[] { NewStringInput() }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var customInput = Assert.Single(Assert.Single(services).CustomInputs);
        Assert.Equal("NationalId", customInput.Name);
        Assert.Equal(14, customInput.MinLength);
        Assert.Null(customInput.MinValue);
        Assert.True(customInput.IsActive);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_leaf_with_valid_integer_input_normalizes_string_fields()
    {
        var services = new List<Service>();
        var result = await CreateHandler(services).Handle(
            ValidCreate() with
            {
                IsClientInputRequired = true,
                CustomInputs = new[] { NewIntegerInput() }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var customInput = Assert.Single(services[0].CustomInputs);
        Assert.Equal(18, customInput.MinValue);
        Assert.Equal(100, customInput.MaxValue);
        Assert.Null(customInput.MinLength);
        Assert.Null(customInput.StartWith);
    }

    [Fact]
    public async Task Create_rejects_missing_or_disallowed_inputs()
    {
        var missing = await CreateHandler(new List<Service>()).Handle(
            ValidCreate() with { IsClientInputRequired = true },
            CancellationToken.None);
        var disallowed = await CreateHandler(new List<Service>()).Handle(
            ValidCreate() with { CustomInputs = new[] { NewStringInput() } },
            CancellationToken.None);

        Assert.Contains(
            missing.Errors,
            x => x.Code == "Services.Create.CustomInputsRequired");
        Assert.Contains(
            disallowed.Errors,
            x => x.Code == "Services.Create.CustomInputsNotAllowed");
    }

    [Fact]
    public async Task Create_rejects_parent_with_active_custom_inputs()
    {
        var parent = EntityTestFactory.Service(1);
        EntityTestFactory.ServiceCustomInput(parent, 10, "LegacyInput");
        var services = new List<Service> { parent };

        var result = await CreateHandler(services).Handle(
            ValidCreate() with { ParentServiceId = parent.Id },
            CancellationToken.None);

        Assert.Contains(
            result.Errors,
            x => x.Code ==
                "Services.Create.ProposedParentHasActiveCustomInputs");
    }

    [Fact]
    public async Task Update_adds_a_new_input_and_commits_once()
    {
        var service = EntityTestFactory.Service(1);
        var services = new List<Service> { service };
        var unitOfWork = new TestUnitOfWork();

        var result = await UpdateHandler(services, unitOfWork).Handle(
            ValidUpdate(service.Id) with
            {
                IsClientInputRequired = true,
                CustomInputs = new[] { UpdateStringInput() }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(service.IsClientInputRequired);
        Assert.Single(service.CustomInputs);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_edits_existing_input_and_restores_inactive_input()
    {
        var service = EntityTestFactory.Service(
            1,
            isClientInputRequired: true);
        var customInput = EntityTestFactory.ServiceCustomInput(
            service,
            20,
            "OldName",
            isActive: false);
        var services = new List<Service> { service };

        var result = await UpdateHandler(services).Handle(
            ValidUpdate(service.Id) with
            {
                IsClientInputRequired = true,
                CustomInputs = new[]
                {
                    UpdateStringInput(
                        id: customInput.Id,
                        name: " RestoredName ",
                        order: 2)
                }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(customInput.IsActive);
        Assert.Equal("RestoredName", customInput.Name);
        Assert.Equal(2, customInput.Order);
    }

    [Fact]
    public async Task Update_deactivates_omitted_active_input()
    {
        var service = EntityTestFactory.Service(
            1,
            isClientInputRequired: true);
        var kept = EntityTestFactory.ServiceCustomInput(
            service,
            20,
            "Kept",
            order: 1);
        var omitted = EntityTestFactory.ServiceCustomInput(
            service,
            21,
            "Omitted",
            order: 2);

        var result = await UpdateHandler(new List<Service> { service }).Handle(
            ValidUpdate(service.Id) with
            {
                IsClientInputRequired = true,
                CustomInputs = new[]
                {
                    UpdateStringInput(kept.Id, kept.Name, kept.Order)
                }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(kept.IsActive);
        Assert.False(omitted.IsActive);
    }

    [Fact]
    public async Task Update_disabling_client_input_deactivates_history()
    {
        var service = EntityTestFactory.Service(
            1,
            isClientInputRequired: true);
        var first = EntityTestFactory.ServiceCustomInput(service, 20, "First");
        var second = EntityTestFactory.ServiceCustomInput(
            service,
            21,
            "Second",
            order: 2);

        var result = await UpdateHandler(new List<Service> { service }).Handle(
            ValidUpdate(service.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(service.IsClientInputRequired);
        Assert.False(first.IsActive);
        Assert.False(second.IsActive);
    }

    [Fact]
    public async Task Update_rejects_unknown_and_foreign_input_ids()
    {
        var target = EntityTestFactory.Service(
            1,
            isClientInputRequired: true);
        var other = EntityTestFactory.Service(
            2,
            isClientInputRequired: true);
        var foreign = EntityTestFactory.ServiceCustomInput(other, 30, "Foreign");
        var services = new List<Service> { target, other };
        var handler = UpdateHandler(services);

        var unknown = await handler.Handle(
            ValidUpdate(target.Id) with
            {
                IsClientInputRequired = true,
                CustomInputs = new[] { UpdateStringInput(999, "Unknown") }
            },
            CancellationToken.None);
        var ownership = await handler.Handle(
            ValidUpdate(target.Id) with
            {
                IsClientInputRequired = true,
                CustomInputs = new[]
                {
                    UpdateStringInput(foreign.Id, "Foreign")
                }
            },
            CancellationToken.None);

        Assert.Contains(
            unknown.Errors,
            x => x.Code == "Services.Update.CustomInputNotFound");
        Assert.Contains(
            ownership.Errors,
            x => x.Code == "Services.Update.CustomInputOwnershipConflict");
    }

    [Fact]
    public async Task Update_parent_rejects_client_input_configuration()
    {
        var parent = EntityTestFactory.Service(1);
        var child = EntityTestFactory.Service(2, parentServiceId: parent.Id);

        var result = await UpdateHandler(
            new List<Service> { parent, child }).Handle(
            ValidUpdate(parent.Id) with
            {
                IsClientInputRequired = true,
                CustomInputs = new[] { UpdateStringInput() }
            },
            CancellationToken.None);

        Assert.Contains(
            result.Errors,
            x => x.Code == "Services.Update.ParentClientInputNotAllowed");
    }

    [Fact]
    public async Task Update_preserves_concurrency_conflict_mapping()
    {
        var service = EntityTestFactory.Service(1);
        var unitOfWork = new TestUnitOfWork
        {
            SaveChangesException = new DbUpdateConcurrencyException()
        };

        var result = await UpdateHandler(
            new List<Service> { service },
            unitOfWork).Handle(
            ValidUpdate(service.Id),
            CancellationToken.None);

        Assert.Contains(
            result.Errors,
            x => x.Code == "Services.Update.ConcurrencyConflict");
    }

    private static CreateServiceCommandHandler CreateHandler(
        List<Service> services,
        TestUnitOfWork? unitOfWork = null) => new(
            new InMemoryWriteReadRepository<Service>(services),
            new InMemoryWriteRepository<Service>(services),
            new TicketUsageChecker(),
            new TestCurrentUser(),
            unitOfWork ?? new TestUnitOfWork());

    private static UpdateServiceCommandHandler UpdateHandler(
        List<Service> services,
        TestUnitOfWork? unitOfWork = null) => new(
            new InMemoryWriteReadRepository<Service>(services),
            new InMemoryWriteRepository<Service>(services),
            new TestConcurrencyTokenManager(),
            new TicketUsageChecker(),
            new TestCurrentUser(),
            unitOfWork ?? new TestUnitOfWork());

    private static CreateServiceCommand ValidCreate() => new()
    {
        ArabicName = "خدمة",
        EnglishName = "Service",
        IsTicketIssuable = false,
        IsClientInputRequired = false,
        OrderNo = 1,
        Priority = 0
    };

    private static UpdateServiceCommand ValidUpdate(int id) => new()
    {
        Id = id,
        ArabicName = $"خدمة {id}",
        EnglishName = $"Service {id}",
        IsTicketIssuable = false,
        IsClientInputRequired = false,
        OrderNo = 1,
        Priority = 0,
        RowVersion = ValidRowVersion
    };

    private static CreateServiceCustomInputCommand NewStringInput() => new()
    {
        Name = " NationalId ",
        LabelEn = " National ID ",
        Type = ServiceCustomInputType.String,
        IsRequired = true,
        MinLength = 14,
        MaxLength = 14,
        Order = 1
    };

    private static CreateServiceCustomInputCommand NewIntegerInput() => new()
    {
        Name = "Age",
        Type = ServiceCustomInputType.Integer,
        MinValue = 18,
        MaxValue = 100,
        Order = 1
    };

    private static UpdateServiceCustomInputCommand UpdateStringInput(
        int? id = null,
        string name = "NationalId",
        int order = 1) => new()
        {
            CustomInputId = id,
            Name = name,
            Type = ServiceCustomInputType.String,
            MinLength = 1,
            MaxLength = 100,
            Order = order
        };

    private sealed class TicketUsageChecker : IServiceTicketUsageChecker
    {
        public Task<bool> HasHistoricalTicketsAsync(
            int serviceId,
            CancellationToken cancellationToken) => Task.FromResult(false);
    }
}
