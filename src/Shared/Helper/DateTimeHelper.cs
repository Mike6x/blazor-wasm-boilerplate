namespace FSH.WebApi.Shared.Helper;
public class DateTimeHelper
{
    public static DateTime Local2UTC(DateTime localDate, TimeSpan localTimeOfDay)
    {
        TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
        DateTime localDateTime = localDate.Date + localTimeOfDay;
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, localTimeZone);
    }
}
