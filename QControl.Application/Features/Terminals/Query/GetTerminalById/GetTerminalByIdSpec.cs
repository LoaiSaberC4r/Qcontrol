using BuildingBlock.Domain.Specification;
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
            BranchId = x.Window.WaitingArea.BranchId,
            BranchArabicName = x.Window.WaitingArea.Branch.ArabicName,
            BranchEnglishName = x.Window.WaitingArea.Branch.EnglishName,
            Number = x.Number,
            IPAddress = x.IPAddress,
            SerialNo = x.SerialNo,
            Type = x.Type,
            CreatedOnUtc = x.CreatedOnUtc,
            ModifiedOnUtc = x.ModifiedOnUtc
        });
    }
}
