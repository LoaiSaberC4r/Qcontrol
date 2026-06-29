using BuildingBlock.Domain.Specification;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Query.GetTerminalById;

internal sealed class GetTerminalByIdSpec
    : Specification<Terminal, TerminalDetailsResponse>
{
    public GetTerminalByIdSpec(int terminalId)
    {
        AddCriteria(x => x.Id == terminalId);
        UseNoTracking();

        Select(x => new TerminalDetailsResponse
        {
            Id = x.Id,
            WindowId = x.WindowId,
            WindowNumber = x.Window.Number,
            WaitingAreaId = x.Window.WaitingAreaId,
            WaitingAreaNumber = x.Window.WaitingArea.Number,
            BranchId = x.BranchId,
            BranchArabicName = x.Window.WaitingArea.Branch.ArabicName,
            BranchEnglishName = x.Window.WaitingArea.Branch.EnglishName,
            Number = x.Number,
            IPAddress = x.IPAddress,
            SerialNo = x.SerialNo,
            Type = x.Type,
            IsActive = x.IsActive,
            EffectiveIsActive =
                x.Window.WaitingArea.Branch.IsActive &&
                x.Window.WaitingArea.IsActive &&
                x.Window.IsActive &&
                x.IsActive,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion),
            CreatedOnUtc = x.CreatedOnUtc,
            ModifiedOnUtc = x.ModifiedOnUtc
        });
    }
}
