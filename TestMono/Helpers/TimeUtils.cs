using System;

namespace TestMono.Helpers;

public static class TimeUtils
{
    public static long GetCurrentTimestamp()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
    }

    public static DateTime GetDateTimeFromTimestamp(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(timestamp);
    }
}
