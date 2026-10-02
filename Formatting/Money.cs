using System.Globalization;

namespace coworking.Formatting;

public static class Money
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

    public static string Format(decimal value) => $"{value.ToString("N2", Ru)} ₽";
}
