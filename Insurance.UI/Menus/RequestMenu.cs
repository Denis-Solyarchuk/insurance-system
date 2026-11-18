using Insurance.Business;
using Insurance.Domain;

namespace Insurance.UI;

public class RequestMenu
{
    private readonly RequestService _requests;
    private readonly ClientService _clients;

    public RequestMenu(RequestService requests, ClientService clients)
    {
        _requests = requests;
        _clients = clients;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n--- Заявки на страхування ---");
            Console.WriteLine("1. Створити заявку");
            Console.WriteLine("2. Усі заявки");
            Console.WriteLine("3. Заявки клієнта");
            Console.WriteLine("4. Підібрати поліси під заявку");
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
            case "1": Create(); break;
            case "2": ConsoleIO.ShowAll(_requests.GetAll()); break;
            case "3": ShowByClient(); break;
            case "4": Match(); break;
            default: Console.WriteLine("Невідомий пункт меню."); break;
        }
    }

    private void Create()
    {
        var client = ConsoleIO.Pick(_clients.GetAll().ToList(), "клієнта");
        if (client == null) return;

        var request = new Request
        {
            ClientId = client.Id,
            Type = ConsoleIO.ReadEnum<PolicyType>("Тип страхування"),
            DesiredCoverage = ConsoleIO.ReadDecimal("Бажана сума покриття"),
            DurationMonths = ConsoleIO.ReadInt("Строк (місяців)")
        };

        _requests.Create(request);
        Console.WriteLine("Заявку створено.");
    }

    private void ShowByClient()
    {
        var client = ConsoleIO.Pick(_clients.GetAll().ToList(), "клієнта");
        if (client == null) return;

        ConsoleIO.ShowAll(_requests.GetByClient(client.Id));
    }

    private void Match()
    {
        var request = ConsoleIO.Pick(_requests.GetAll().ToList(), "заявку");
        if (request == null) return;

        var result = _requests.FindMatches(request.Id);

        Console.WriteLine($"Орієнтовна вартість нового поліса за заявкою: {result.EstimatedCost:N2}");
        Console.WriteLine("Підходящі поліси:");
        ConsoleIO.ShowAll(result.Matches);
        Console.WriteLine("Заявку позначено як оброблену.");
    }
}