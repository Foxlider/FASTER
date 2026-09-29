namespace FASTER.Services;

public interface IAnalyticsService
{
    void TrackEvent(string name, IDictionary<string, string>? properties = null);
    void TrackError(Exception exception, IDictionary<string, string>? properties = null);
}
