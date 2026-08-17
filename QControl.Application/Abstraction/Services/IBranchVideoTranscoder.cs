namespace QControl.Application.Abstraction.Services;

public interface IBranchVideoTranscoder
{
    Task<string> TranscodeToHlsAsync(
        string originalPath,
        CancellationToken cancellationToken);

    void RemoveHlsOutput(
        string originalPath,
        string? hlsManifestPath);
}
