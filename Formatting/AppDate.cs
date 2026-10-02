namespace coworking.Formatting;

public static class AppDate
{
    public static string Format(DateTime value) => value.ToString("dd.MM.yyyy HH:mm");

    public static string Hours(decimal hours) => hours.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + " ч";
}
