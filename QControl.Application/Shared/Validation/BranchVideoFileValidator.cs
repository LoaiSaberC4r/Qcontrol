using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;

namespace QControl.Application.Shared.Validation;

internal static class BranchVideoFileValidator
{
    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "video/mp4",
            "video/webm",
            "video/quicktime",
            "video/x-msvideo",
            "video/x-matroska",
            "video/mpeg",
            "video/3gpp"
        };

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".m4v", ".mov", ".webm", ".avi", ".mkv", ".mpeg", ".mpg", ".3gp"
        };

    public static Error? Validate(
        IFormFile? video,
        string requiredMessage,
        string invalidTypeMessage)
    {
        if (video is null || video.Length == 0)
        {
            return new Error(
                "BranchVideos.VideoRequired",
                requiredMessage,
                ErrorType.Validation);
        }

        var extension = Path.GetExtension(video.FileName);
        if (!AllowedExtensions.Contains(extension) ||
            string.IsNullOrWhiteSpace(video.ContentType) ||
            !AllowedContentTypes.Contains(video.ContentType))
        {
            return new Error(
                "BranchVideos.InvalidVideoType",
                invalidTypeMessage,
                ErrorType.Validation);
        }

        return null;
    }
}
