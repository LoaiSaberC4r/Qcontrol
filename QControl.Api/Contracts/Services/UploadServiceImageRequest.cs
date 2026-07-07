using Microsoft.AspNetCore.Http;

namespace Qcontrol.Api.Contracts.Services;

public sealed class UploadServiceImageRequest
{
    public IFormFile? Image { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
