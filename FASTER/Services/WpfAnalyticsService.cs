using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using System;
using System.Collections.Generic;

namespace FASTER.Services
{
    internal sealed class WpfAnalyticsService : IAnalyticsService
    {
        public void TrackEvent(string name, IDictionary<string, string>? properties = null)
            => Analytics.TrackEvent(name, properties == null ? null : new Dictionary<string, string>(properties));

        public void TrackError(Exception exception, IDictionary<string, string>? properties = null)
            => Crashes.TrackError(exception, properties == null ? null : new Dictionary<string, string>(properties));
    }
}
