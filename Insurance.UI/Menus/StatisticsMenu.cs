using Insurance.Business;

namespace Insurance.UI;

public class StatisticsMenu
{
    private readonly StatisticsService _stats;

    public StatisticsMenu(StatisticsService stats) => _stats = stats;

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n--- Статистика ---");
            Console.WriteLine("1. Кількість активних полісів");
            Console.WriteLine("2. Кількість подій за період");
            Console.WriteLine("3. Загальна сума виплат");
            Console.WriteLine("4. Дохід компанії");
            Console.WriteLine("5. Ефективність агентів");
            Console.WriteLine("6. Статистика за типами страхування");
            Console.WriteLine("0. Назад");
            Console.Write("> ");

            var choice = Console.ReadLine()?.Trim();
            if (choice == "0") return;
            ConsoleIO.Safe(() => Handle(choice));
        }
    }

    private void Handle(string? choice)
    {
        switch (choice)
        {
            case "1":
                Console.WriteLine($"Активних полісів: {_stats.CountActivePolicies()}");
                break;
            case "2":
                CountInPeriod();
                break;
            case "3":
                Console.WriteLine($"Загальна сума виплат: {_stats.TotalPayouts():N2}");
                break;
            case "4":
                Console.WriteLine($"Внески: {_stats.TotalPremiums():N2}");
                Console.WriteLine($"Виплати: {_stats.TotalPayouts():N2}");
                Console.WriteLine($"Дохід: {_stats.CompanyIncome():N2}");
                break;
            case "5":
                ShowAgents();
                break;
            case "6":
                ShowByType();
                break;
            default:
                Console.WriteLine("Невідомий пункт меню.");
                break;
        }
    }

    private void CountInPeriod()
    {
        var from = ConsoleIO.ReadDate("Початок періоду");
        var to = ConsoleIO.ReadDate("Кінець періоду");
        Console.WriteLine($"Подій за період: {_stats.CountClaimsInPeriod(from, to)}");
    }

    private void ShowAgents()
    {
        var report = _stats.GetAgentPerformance();
        if (report.Count == 0)
        {
            Console.WriteLine("Агентів немає.");
            return;
        }

        foreach (var r in report)
            Console.WriteLine($"{r.Agent.Name} | продано полісів: {r.PoliciesSold} | комісія: {r.Commission:N2}");
    }

    private void ShowByType()
    {
        foreach (var r in _stats.GetTypeReport())
            Console.WriteLine(
                $"{r.Type}: активних {r.Active}, завершених {r.Finished}, середня вартість {r.AverageCost:N2}");
    }
}