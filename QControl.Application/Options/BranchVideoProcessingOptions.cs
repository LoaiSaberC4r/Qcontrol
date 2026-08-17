namespace QControl.Application.Options;

public sealed class BranchVideoProcessingOptions
{
    public const string SectionName = "BranchVideoProcessing";

    public string FfmpegPath { get; set; } = "ffmpeg";

    public string TargetResolution { get; set; } = "1280x720";

    public int MaximumFrameRate { get; set; } = 30;

    public string VideoBitrate { get; set; } = "1500k";

    public string AudioBitrate { get; set; } = "128k";

    public int SegmentDurationSeconds { get; set; } = 6;
}
