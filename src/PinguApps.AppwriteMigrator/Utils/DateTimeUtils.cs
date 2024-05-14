namespace PinguApps.AppwriteMigrator.Utils;
public static class DateTimeUtils
{
    public static string ConvertLocalTimeToOffset(string currentValue)
    {
        var dt = DateTime.Parse(currentValue);

        return dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz");
    }
}