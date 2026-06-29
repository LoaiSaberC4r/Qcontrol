using Microsoft.AspNetCore.Http;

namespace Qcontrol.Api.Contracts.Branches;

public sealed class UploadBranchLogoRequest
{
    public IFormFile? Logo { get; init; }

    public string? RowVersion { get; init; }
}
