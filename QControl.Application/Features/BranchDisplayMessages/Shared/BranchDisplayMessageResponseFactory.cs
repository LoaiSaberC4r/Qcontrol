using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Shared;

internal static class BranchDisplayMessageResponseFactory
{
    public static BranchDisplayMessageResponse FromEntity(BranchDisplayMessage message) =>
        new()
        {
            Id = message.Id,
            BranchId = message.BranchId,
            TextAr = message.TextAr,
            TextEn = message.TextEn,
            DisplayOrder = message.DisplayOrder,
            IsActive = message.IsActive,
            RowVersion = RowVersionConverter.ToBase64(message.RowVersion)
        };

    public static BranchDisplayMessageStateResponse StateFromEntity(
        BranchDisplayMessage message,
        string resultMessage) =>
        new()
        {
            MessageId = message.Id,
            BranchId = message.BranchId,
            IsActive = message.IsActive,
            RowVersion = RowVersionConverter.ToBase64(message.RowVersion),
            Message = resultMessage
        };
}
