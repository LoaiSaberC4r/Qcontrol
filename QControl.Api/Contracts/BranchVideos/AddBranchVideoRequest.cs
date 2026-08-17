using Microsoft.AspNetCore.Http;

namespace Qcontrol.Api.Contracts.BranchVideos;

public sealed class AddBranchVideoRequest
{
    public IFormFile? Video { get; init; }
    public int DisplayOrder { get; init; }
}
