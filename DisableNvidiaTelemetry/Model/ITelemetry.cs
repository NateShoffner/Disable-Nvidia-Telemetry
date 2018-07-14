namespace DisableNvidiaTelemetry.Model
{
    public interface ITelemetry
    {
        bool RestartRequired { get; set; }
        bool IsActive();
    }
}