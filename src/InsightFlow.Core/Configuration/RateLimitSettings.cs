namespace InsightFlow.Core.Configuration;

public sealed class RateLimitSettings
{
    public int RequestsPerMinute { get; init; } = 120;
    public int UploadRequestsPerMinute { get; init; } = 20;
}
