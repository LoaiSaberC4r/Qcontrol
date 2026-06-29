using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;

namespace QControl.Application.Shared.Validation;

internal static class BranchImageFileValidator
{
    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/png",
            "image/jpeg",
            "image/webp"
        };

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".webp"
        };

    public static Error? Validate(
        IFormFile? imageFile,
        string requiredCode,
        string requiredMessage,
        string invalidTypeCode,
        string invalidTypeMessage)
    {
        if (imageFile is null || imageFile.Length == 0)
        {
            return new Error(
                requiredCode,
                requiredMessage,
                ErrorType.Validation);
        }

        var extension = Path.GetExtension(imageFile.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return InvalidType(invalidTypeCode, invalidTypeMessage);
        }

        if (string.IsNullOrWhiteSpace(imageFile.ContentType) ||
            !AllowedContentTypes.Contains(imageFile.ContentType))
        {
            return InvalidType(invalidTypeCode, invalidTypeMessage);
        }

        if (!HasExpectedSignature(imageFile, extension))
        {
            return InvalidType(invalidTypeCode, invalidTypeMessage);
        }

        return null;
    }

    private static Error InvalidType(string code, string message) =>
        new(code, message, ErrorType.Validation);

    private static bool HasExpectedSignature(
        IFormFile imageFile,
        string extension)
    {
        Span<byte> header = stackalloc byte[12];

        using var stream = imageFile.OpenReadStream();
        var bytesRead = stream.Read(header);

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => IsJpeg(header, bytesRead),
            ".png" => IsPng(header, bytesRead),
            ".webp" => IsWebp(header, bytesRead),
            _ => false
        };
    }

    private static bool IsJpeg(ReadOnlySpan<byte> header, int bytesRead) =>
        bytesRead >= 3 &&
        header[0] == 0xFF &&
        header[1] == 0xD8 &&
        header[2] == 0xFF;

    private static bool IsPng(ReadOnlySpan<byte> header, int bytesRead) =>
        bytesRead >= 8 &&
        header[0] == 0x89 &&
        header[1] == 0x50 &&
        header[2] == 0x4E &&
        header[3] == 0x47 &&
        header[4] == 0x0D &&
        header[5] == 0x0A &&
        header[6] == 0x1A &&
        header[7] == 0x0A;

    private static bool IsWebp(ReadOnlySpan<byte> header, int bytesRead) =>
        bytesRead >= 12 &&
        header[0] == 0x52 &&
        header[1] == 0x49 &&
        header[2] == 0x46 &&
        header[3] == 0x46 &&
        header[8] == 0x57 &&
        header[9] == 0x45 &&
        header[10] == 0x42 &&
        header[11] == 0x50;
}
