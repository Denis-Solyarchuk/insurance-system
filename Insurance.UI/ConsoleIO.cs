using System.Globalization;
using Insurance.Business;

namespace Insurance.UI;

public static class ConsoleIO
{
    public static string ReadString(string prompt)
    {
        Console.Write($"{prompt}: ");
        return Console.ReadLine()?.Trim() ?? "";
    }

    public static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            var text = ReadString(prompt).Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                return value;
            Console.WriteLine("Введіть число.");
        }
    }

    public static decimal? ReadOptionalDecimal(string prompt)
    {
        while (true)
        {
            var text = ReadString($"{prompt} (Enter - пропустити)").Replace(',', '.');
            if (text == "") return null;
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                return value;
            Console.WriteLine("Введіть число.");
        }
    }

    public static int ReadInt(string prompt)
    {
        while (true)
        {
            if (int.TryParse(ReadString(prompt), out var value))
                return value;
            Console.WriteLine("Введіть ціле число.");
        }
    }

    public static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            var text = ReadString($"{prompt} (дд.мм.рррр)");
            if (DateTime.TryParseExact(text, "dd.MM.yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
                return date;
            Console.WriteLine("Невірний формат дати.");
        }
    }

    public static bool Confirm(string question)
    {
        Console.Write($"{question} (y/n): ");
        return Console.ReadLine()?.Trim().ToLower() == "y";
    }

    public static TEnum ReadEnum<TEnum>(string prompt) where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();
        while (true)
        {
            Console.WriteLine($"{prompt}:");
            for (var i = 0; i < values.Length; i++)
                Console.WriteLine($"  {i + 1} - {values[i]}");

            if (int.TryParse(ReadString("Номер"), out var n) && n >= 1 && n <= values.Length)
                return values[n - 1];
            Console.WriteLine("Некоректний вибір.");
        }
    }

    public static TEnum? ReadOptionalEnum<TEnum>(string prompt) where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();
        while (true)
        {
            Console.WriteLine($"{prompt} (Enter - пропустити):");
            for (var i = 0; i < values.Length; i++)
                Console.WriteLine($"  {i + 1} - {values[i]}");

            var text = ReadString("Номер");
            if (text == "") return null;
            if (int.TryParse(text, out var n) && n >= 1 && n <= values.Length)
                return values[n - 1];
            Console.WriteLine("Некоректний вибір.");
        }
    }

    public static void ShowAll<T>(IEnumerable<T> items)
    {
        var list = items.ToList();
        if (list.Count == 0)
        {
            Console.WriteLine("Список порожній.");
            return;
        }

        for (var i = 0; i < list.Count; i++)
            Console.WriteLine($"{i + 1}. {list[i]}");
    }

    public static T? Pick<T>(IReadOnlyList<T> list, string what) where T : class
    {
        if (list.Count == 0)
        {
            Console.WriteLine("Список порожній.");
            return null;
        }

        ShowAll(list);
        Console.Write($"Оберіть {what} (номер): ");

        if (int.TryParse(Console.ReadLine(), out var n) && n >= 1 && n <= list.Count)
            return list[n - 1];

        Console.WriteLine("Некоректний номер.");
        return null;
    }

    // Єдине місце обробки помилок у UI
    public static void Safe(Action action)
    {
        try
        {
            action();
        }
        catch (ValidationException ex)
        {
            Console.WriteLine("Помилка:");
            foreach (var error in ex.Errors)
                Console.WriteLine($" - {error}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
        }
    }
}