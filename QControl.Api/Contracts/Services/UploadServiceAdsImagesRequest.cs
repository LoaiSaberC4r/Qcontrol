using Microsoft.AspNetCore.Http;

namespace Qcontrol.Api.Contracts.Services;

public sealed class UploadServiceAdsImagesRequest
{
    public List<IFormFile> Images { get; init; } = new();

    public string RowVersion { get; init; } = string.Empty;
}
