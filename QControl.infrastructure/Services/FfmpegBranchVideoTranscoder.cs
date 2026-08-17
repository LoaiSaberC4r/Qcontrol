using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QControl.Application.Abstraction.Services;
using QControl.Application.Options;

namespace QControl.infrastructure.Services;

internal sealed class FfmpegBranchVideoTranscoder
    : IBranchVideoTranscoder
{
    private const int DiagnosticCharacterLimit = 32_768;

    private readonly BranchVideoProcessingOptions _options;
    private readonly ILogger<FfmpegBranchVideoTranscoder> _logger;
    private readonly string _mediaRoot;

    public FfmpegBranchVideoTranscoder(
        IOptions<BranchVideoProcessingOptions> options,
        ILogger<FfmpegBranchVideoTranscoder> logger)
    {
        _options = options.Value;
        _logger = logger;
        _mediaRoot = Path.GetFullPath(Path.Combine(".", "wwwroot", "Media"));
    }

    public async Task<string> TranscodeToHlsAsync(
        string originalPath,
        CancellationToken cancellationToken)
    {
        var sourcePath = ResolveStoredPath(originalPath);
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException(
                "The original branch video could not be found.",
                sourcePath);
        }

        var hlsDirectory = ResolveHlsDirectoryFromOriginal(sourcePath);
        var manifestPath = Path.Combine(hlsDirectory, "master.m3u8");
        if (File.Exists(manifestPath))
        {
            return ToStoredPath(manifestPath);
        }

        if (Directory.Exists(hlsDirectory))
        {
            Directory.Delete(hlsDirectory, recursive: true);
        }

        var videoRoot = Path.GetDirectoryName(hlsDirectory)!;
        var temporaryDirectory = Path.Combine(
            videoRoot,
            $"hls.tmp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);

        var temporaryManifest = Path.Combine(temporaryDirectory, "master.m3u8");
        var temporarySegments = Path.Combine(temporaryDirectory, "segment-%05d.ts");
        using var process = CreateProcess(
            sourcePath,
            temporaryManifest,
            temporarySegments);

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException("FFmpeg could not be started.");
            }

            var standardOutput = ReadBoundedAsync(
                process.StandardOutput,
                DiagnosticCharacterLimit,
                CancellationToken.None);
            var standardError = ReadBoundedAsync(
                process.StandardError,
                DiagnosticCharacterLimit,
                CancellationToken.None);

            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                await Task.WhenAll(standardOutput, standardError);
                throw;
            }

            var output = await standardOutput;
            var error = await standardError;
            if (process.ExitCode != 0)
            {
                _logger.LogError(
                    "FFmpeg failed with exit code {ExitCode}. Output: {Output}. Error: {Error}",
                    process.ExitCode,
                    output,
                    error);
                throw new InvalidOperationException(
                    $"FFmpeg exited with code {process.ExitCode}.");
            }

            if (!File.Exists(temporaryManifest))
            {
                throw new InvalidOperationException(
                    "FFmpeg completed without producing master.m3u8.");
            }

            Directory.Move(temporaryDirectory, hlsDirectory);
            return ToStoredPath(manifestPath);
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    public void RemoveHlsOutput(
        string originalPath,
        string? hlsManifestPath)
    {
        var hlsDirectory = string.IsNullOrWhiteSpace(hlsManifestPath)
            ? ResolveHlsDirectoryFromOriginal(ResolveStoredPath(originalPath))
            : Path.GetDirectoryName(ResolveStoredPath(hlsManifestPath))!;

        EnsureUnderMediaRoot(hlsDirectory);
        if (Directory.Exists(hlsDirectory))
        {
            Directory.Delete(hlsDirectory, recursive: true);
        }

        var videoRoot = Path.GetDirectoryName(hlsDirectory)!;
        EnsureUnderMediaRoot(videoRoot);
        if (!Directory.Exists(videoRoot))
        {
            return;
        }

        foreach (var temporaryDirectory in Directory.GetDirectories(
                     videoRoot,
                     "hls.tmp-*",
                     SearchOption.TopDirectoryOnly))
        {
            EnsureUnderMediaRoot(temporaryDirectory);
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private Process CreateProcess(
        string sourcePath,
        string manifestPath,
        string segmentPattern)
    {
        var dimensions = _options.TargetResolution.Split('x', 'X');
        var scaleFilter =
            $"scale={dimensions[0]}:{dimensions[1]}:" +
            "force_original_aspect_ratio=decrease:force_divisible_by=2";
        var keyFrameExpression =
            $"expr:gte(t,n_forced*{_options.SegmentDurationSeconds})";

        var startInfo = new ProcessStartInfo
        {
            FileName = _options.FfmpegPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        AddArguments(startInfo,
            "-hide_banner", "-nostdin", "-y",
            "-i", sourcePath,
            "-map", "0:v:0",
            "-map", "0:a?",
            "-c:v", "libx264",
            "-profile:v", "main",
            "-pix_fmt", "yuv420p",
            "-vf", scaleFilter,
            "-fpsmax", _options.MaximumFrameRate.ToString(),
            "-force_key_frames", keyFrameExpression,
            "-b:v", _options.VideoBitrate,
            "-preset", "veryfast",
            "-c:a", "aac",
            "-b:a", _options.AudioBitrate,
            "-f", "hls",
            "-hls_time", _options.SegmentDurationSeconds.ToString(),
            "-hls_playlist_type", "vod",
            "-hls_flags", "independent_segments",
            "-hls_segment_filename", segmentPattern,
            manifestPath);
        return new Process { StartInfo = startInfo };
    }

    private static void AddArguments(
        ProcessStartInfo startInfo,
        params string[] arguments)
    {
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
    }

    private async Task<string> ReadBoundedAsync(
        StreamReader reader,
        int limit,
        CancellationToken cancellationToken)
    {
        var result = new StringBuilder(Math.Min(limit, 4096));
        var buffer = new char[2048];
        while (true)
        {
            var read = await reader.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                break;
            }

            var remaining = limit - result.Length;
            if (remaining > 0)
            {
                result.Append(buffer, 0, Math.Min(read, remaining));
            }
        }

        return result.ToString();
    }

    private string ResolveStoredPath(string storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            throw new ArgumentException("A stored media path is required.", nameof(storedPath));
        }

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

        if (Path.IsPathRooted(normalized) ||
            normalized.Contains(':', StringComparison.Ordinal) ||
            normalized.Split('/').Any(x => x is "" or "." or ".."))
        {
            throw new ArgumentException("The stored media path is unsafe.", nameof(storedPath));
        }

        var physicalPath = Path.GetFullPath(Path.Combine(
            _mediaRoot,
            normalized.Replace('/', Path.DirectorySeparatorChar)));
        EnsureUnderMediaRoot(physicalPath);
        return physicalPath;
    }

    private string ResolveHlsDirectoryFromOriginal(string sourcePath)
    {
        EnsureUnderMediaRoot(sourcePath);
        var originalDirectory = Path.GetDirectoryName(sourcePath)
            ?? throw new InvalidOperationException("The source path has no directory.");
        if (!string.Equals(
                Path.GetFileName(originalDirectory),
                "original",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The original video is not in the expected media layout.");
        }

        var videoRoot = Path.GetDirectoryName(originalDirectory)!;
        var hlsDirectory = Path.GetFullPath(Path.Combine(videoRoot, "hls"));
        EnsureUnderMediaRoot(hlsDirectory);
        return hlsDirectory;
    }

    private string ToStoredPath(string physicalPath)
    {
        EnsureUnderMediaRoot(physicalPath);
        return Path.GetRelativePath(_mediaRoot, physicalPath)
            .Replace('\\', '/');
    }

    private void EnsureUnderMediaRoot(string physicalPath)
    {
        var root = _mediaRoot.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(physicalPath);
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The media path resolves outside the configured media root.");
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }
}
