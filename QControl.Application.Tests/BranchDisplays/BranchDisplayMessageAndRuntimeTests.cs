using System.Text.Json;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayRuntimeConfiguration;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.UpdateBranchDisplayMessage;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.DeactivateBranchDisplayMessage;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.ReactivateBranchDisplayMessage;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.ReorderBranchDisplayMessages;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.BranchDisplays;

public sealed class BranchDisplayMessageAndRuntimeTests
{
    private static readonly byte[] RowVersion = [1, 2, 3, 4, 5, 6, 7, 8];

    [Fact]
    public async Task Update_rejects_cross_branch_message_mutation()
    {
        var messages = new List<BranchDisplayMessage>
        {
            EntityTestFactory.BranchDisplayMessage(7, 2, 1, rowVersion: RowVersion)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = new UpdateBranchDisplayMessageCommandHandler(
            new InMemoryWriteReadRepository<Branch>(new() { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<BranchDisplayMessage>(messages),
            new InMemoryWriteRepository<BranchDisplayMessage>(messages),
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);

        var result = await handler.Handle(new UpdateBranchDisplayMessageCommand
        {
            BranchId = 1,
            MessageId = 7,
            TextAr = "رسالة",
            TextEn = "Message",
            DisplayOrder = 1,
            RowVersion = Convert.ToBase64String(RowVersion)
        }, CancellationToken.None);

        Assert.Contains(result.Errors, x => x.Code == "BranchDisplayMessage.DoesNotBelongToBranch");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Message_can_be_reactivated_and_deactivated_with_concurrency_tokens()
    {
        var message = EntityTestFactory.BranchDisplayMessage(
            7,
            1,
            1,
            isActive: false,
            rowVersion: RowVersion);
        var messages = new List<BranchDisplayMessage> { message };
        var branches = new InMemoryWriteReadRepository<Branch>(new() { EntityTestFactory.Branch(1) });
        var messageReader = new InMemoryWriteReadRepository<BranchDisplayMessage>(messages);
        var writer = new InMemoryWriteRepository<BranchDisplayMessage>(messages);
        var concurrency = new TestConcurrencyTokenManager();
        var unitOfWork = new TestUnitOfWork();
        var rowVersion = Convert.ToBase64String(RowVersion);

        var reactivate = new ReactivateBranchDisplayMessageCommandHandler(
            branches,
            messageReader,
            writer,
            concurrency,
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);
        var reactivated = await reactivate.Handle(new ReactivateBranchDisplayMessageCommand
        {
            BranchId = 1,
            MessageId = 7,
            RowVersion = rowVersion
        }, CancellationToken.None);

        Assert.True(reactivated.IsSuccess);
        Assert.True(message.IsActive);

        var deactivate = new DeactivateBranchDisplayMessageCommandHandler(
            branches,
            messageReader,
            writer,
            concurrency,
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);
        var deactivated = await deactivate.Handle(new DeactivateBranchDisplayMessageCommand
        {
            BranchId = 1,
            MessageId = 7,
            RowVersion = rowVersion
        }, CancellationToken.None);

        Assert.True(deactivated.IsSuccess);
        Assert.False(message.IsActive);
        Assert.Equal(2, concurrency.SetOriginalRowVersionCallCount);
        Assert.Equal(2, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public void Reorder_validator_rejects_duplicate_ids_and_orders()
    {
        var token = Convert.ToBase64String(RowVersion);
        var result = new ReorderBranchDisplayMessagesCommandValidator().Validate(
            new ReorderBranchDisplayMessagesCommand
            {
                BranchId = 1,
                Items =
                [
                    new ReorderBranchDisplayMessageItem { MessageId = 5, DisplayOrder = 1, RowVersion = token },
                    new ReorderBranchDisplayMessageItem { MessageId = 5, DisplayOrder = 1, RowVersion = token }
                ]
            });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ReorderBranchDisplayMessagesCommand.Items));
    }

    [Fact]
    public async Task Runtime_returns_logo_configuration_and_only_active_ordered_messages_without_admin_metadata()
    {
        var branch = EntityTestFactory.Branch(1);
        EntityTestFactory.AttachBranding(
            branch,
            EntityTestFactory.BranchBranding(2, 1, logoPath: "Branches/1/Logo/logo.png"));
        EntityTestFactory.AttachDisplayConfiguration(
            branch,
            EntityTestFactory.BranchDisplayConfiguration(3, 1, RowVersion));
        var messages = new List<BranchDisplayMessage>
        {
            EntityTestFactory.BranchDisplayMessage(12, 1, 3, rowVersion: RowVersion),
            EntityTestFactory.BranchDisplayMessage(10, 1, 1, rowVersion: RowVersion),
            EntityTestFactory.BranchDisplayMessage(11, 1, 2, isActive: false, rowVersion: RowVersion),
            EntityTestFactory.BranchDisplayMessage(20, 2, 1, rowVersion: RowVersion)
        };
        var handler = new GetBranchDisplayRuntimeConfigurationQueryHandler(
            new InMemoryWriteReadRepository<Branch>(new() { branch }),
            new InMemoryWriteReadRepository<BranchDisplayMessage>(messages),
            new TestCacheService());

        var result = await handler.Handle(
            new GetBranchDisplayRuntimeConfigurationQuery { BranchId = 1 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("/Media/Branches/1/Logo/logo.png", result.Value.Branding.LogoUrl);
        Assert.NotNull(result.Value.DisplayConfiguration);
        Assert.Equal(new[] { 10, 12 }, result.Value.Messages.Select(x => x.Id));
        var json = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain("RowVersion", json, StringComparison.Ordinal);
        Assert.DoesNotContain("CreatedBy", json, StringComparison.Ordinal);
        Assert.DoesNotContain("IsActive", json, StringComparison.Ordinal);
    }
}
