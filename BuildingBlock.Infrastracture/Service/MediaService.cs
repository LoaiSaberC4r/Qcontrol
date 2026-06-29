using BuildingBlock.Application.Abstraction.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlock.Infrastracture.Service
{
    public class MediaService : IMediaService
    {
        private readonly ILogger<MediaService> _logger;
        private readonly string _mediaRoot;

        public MediaService(ILogger<MediaService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger is not configured.");
            _mediaRoot = Path.GetFullPath(Path.Combine(".", "wwwroot", "Media"));
        }

        public async Task<string> SaveAsync(IFormFile mediaFile, string folderName)
        {
            if (mediaFile == null)
                throw new ArgumentNullException(nameof(mediaFile), "Media file cannot be null.");

            _logger.LogInformation("Saving media file: {FileName} to folder: {FolderName}", mediaFile.FileName, folderName);

            var relativeFolder = NormalizeFolder(folderName);
            var extension = NormalizeExtension(Path.GetExtension(mediaFile.FileName));
            var fileName = $"{Guid.NewGuid()}{extension}";
            var relativePath = CombineRelative(relativeFolder, fileName);
            var filePath = ResolvePhysicalPath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Create);
                await mediaFile.CopyToAsync(stream);
                _logger.LogInformation("File saved successfully: {RelativePath}", relativePath);

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {FileName}", mediaFile.FileName);
                throw;
            }
        }

        public async Task<Stream> GetStream(IFormFile formFile)
        {
            if (formFile == null)
                throw new ArgumentNullException(nameof(formFile), "Form file cannot be null.");

            _logger.LogInformation("Converting form file to stream: {FileName}", formFile.FileName);

            var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task<string> SaveVideoAsync(IFormFile videoFile, string folderName)
        {
            if (videoFile == null)
                throw new ArgumentNullException(nameof(videoFile), "Video file cannot be null.");

            _logger.LogInformation("Saving video file: {FileName} to folder: {FolderName}", videoFile.FileName, folderName);

            var relativeFolder = NormalizeFolder(folderName);
            var extension = NormalizeExtension(Path.GetExtension(videoFile.FileName));
            var fileName = $"{Guid.NewGuid()}{extension}";
            var relativePath = CombineRelative(relativeFolder, fileName);
            var filePath = ResolvePhysicalPath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Create);
                await videoFile.CopyToAsync(stream);
                _logger.LogInformation("Video saved successfully: {RelativePath}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving video file: {FileName}", videoFile.FileName);
                throw;
            }
        }

        public async Task<List<string>> SaveAsync(List<IFormFile> formFiles, string folderName)
        {
            ArgumentNullException.ThrowIfNull(formFiles);

            var filePaths = new List<string>();

            try
            {
                foreach (var file in formFiles)
                {
                    filePaths.Add(await SaveAsync(file, folderName));
                }
            }
            catch
            {
                RemoveRange(filePaths);
                throw;
            }

            return filePaths;
        }

        public void Remove(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            var physicalPath = ResolveStoredPath(filePath);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        public void RemoveRange(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                Remove(filePath);
            }
        }

        private string NormalizeFolder(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                throw new ArgumentException("Folder name is required.", nameof(folderName));
            }

            var normalized = folderName.Trim().Replace('\\', '/').Trim('/');

            if (Path.IsPathRooted(folderName) ||
                normalized.Contains(':', StringComparison.Ordinal) ||
                normalized.Split('/').Any(segment =>
                    string.IsNullOrWhiteSpace(segment) ||
                    segment is "." or ".." ||
                    segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            {
                throw new ArgumentException("Folder name contains an unsafe path.", nameof(folderName));
            }

            return normalized;
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException("File extension is required.", nameof(extension));
            }

            return extension.Trim().ToLowerInvariant();
        }

        private string ResolveStoredPath(string storedPath)
        {
            var normalized = storedPath.Trim().Replace('\\', '/');

            if (normalized.StartsWith("/Media/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized["/Media/".Length..];
            }
            else if (normalized.StartsWith("Media/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized["Media/".Length..];
            }
            else if (normalized.StartsWith("wwwroot/Media/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized["wwwroot/Media/".Length..];
            }

            if (Path.IsPathRooted(normalized))
            {
                var rooted = Path.GetFullPath(normalized);
                EnsureUnderMediaRoot(rooted);
                return rooted;
            }

            return ResolvePhysicalPath(normalized);
        }

        private string ResolvePhysicalPath(string relativePath)
        {
            var normalized = relativePath.Trim().Replace('\\', '/').Trim('/');

            if (normalized.Contains(':', StringComparison.Ordinal) ||
                normalized.Split('/').Any(segment =>
                    string.IsNullOrWhiteSpace(segment) ||
                    segment is "." or ".."))
            {
                throw new ArgumentException("File path contains an unsafe path.", nameof(relativePath));
            }

            var physicalPath = Path.GetFullPath(Path.Combine(
                _mediaRoot,
                normalized.Replace('/', Path.DirectorySeparatorChar)));

            EnsureUnderMediaRoot(physicalPath);

            return physicalPath;
        }

        private void EnsureUnderMediaRoot(string physicalPath)
        {
            var root = _mediaRoot.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!physicalPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Media path resolves outside the media root.");
            }
        }

        private static string CombineRelative(string folder, string fileName) =>
            $"{folder.Trim('/')}/{fileName}".Replace('\\', '/');
    }
}
