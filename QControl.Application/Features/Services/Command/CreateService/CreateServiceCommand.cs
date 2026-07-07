using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Services.Command.CreateService;

public sealed record CreateServiceCommand
    : ICommand<ServiceResponse>,
      ICacheInvalidator
{
    public int? ParentServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string? ArabicUserMessage { get; init; }

    public string? EnglishUserMessage { get; init; }

    public bool? IsTicketIssuable { get; init; }

    public bool IsClientInputRequired { get; init; }

    public bool HasReservation { get; init; }

    public int OrderNo { get; init; }

    public int Priority { get; init; }

    public string? RangePrefix { get; init; }

    public int? RangeStartNumber { get; init; }

    public int? RangeEndNumber { get; init; }

    public int? WaitingDuration { get; init; }

    public int? NoOfTicketCopies { get; init; }

    public IEnumerable<string> Tags =>
        ParentServiceId.HasValue
            ? new[]
            {
                OperationalCacheTags.Services,
                OperationalCacheTags.Service(ParentServiceId.Value)
            }
            : new[]
            {
                OperationalCacheTags.Services
            };
}
