namespace PulseTrace.Config;

public class PulseTraceOptions
{
    public int ThresholdMilliseconds { get; set; } = 100;
    public List<string> IncludeControllers { get; set; } = new();
    public string? OutputJsonPath { get; set; } = "pulse-log.json";
}