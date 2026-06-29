using Microsoft.AspNetCore.Http;

namespace Qcontrol.Api.Contracts.Branches;

public sealed class AddBranchAdvertisementsRequest
{
    public List<IFormFile> Images { get; init; } = new();
}
