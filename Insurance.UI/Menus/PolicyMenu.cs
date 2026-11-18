using Insurance.Business;
using Insurance.Domain;

namespace Insurance.UI;

public class PolicyMenu
{
    private readonly PolicyService _policies;
    private readonly ClientService _clients;
    private readonly AgentService _agents;

    public PolicyMenu(PolicyService policies, ClientService clients, AgentService agents)
    {
        _policies = policies;
        _clients = clients;
        _agents = agents;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n--- Поліси ---");
            Console.WriteLine("1. Створити поліс");
            Console.WriteLine("2. Список полісів");
            Console.WriteLine("3. Змінити суму покриття");
            Console.WriteLine("4. Змінити статус");
            Console.WriteLine("5. Пошук полісів");
            Console.WriteLine("6. Видалити поліс");
            Console.WriteLine("7. Список полісів з агентами");
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
            case "1": Add(); break;
            case "2": ConsoleIO.ShowAll(_policies.GetAll()); break;
            case "3": ChangeCoverage(); break;
            case "4": ChangeStatus(); break;
            case "5": Search(); break;
            case "6": Delete(); break;
            case "7": ShowWithAgents(); break;
            default: Console.WriteLine("Невідомий пункт меню."); break;
        }
    }

    private void Add()
    {
        var client = ConsoleIO.Pick(_clients.GetAll().ToList(), "клієнта");
        if (client == null) return;

        var start = DateTime.Today;
        var policy = new Policy
        {
            ClientId = client.Id,
            Number = ConsoleIO.ReadString("Номер поліса"),
            Type = ConsoleIO.ReadEnum<PolicyType>("Тип страхування"),
            CoverageAmount = ConsoleIO.ReadDecimal("Сума покриття"),
            StartDate = start,
            EndDate = start.AddMonths(ConsoleIO.ReadInt("Тривалість (місяців)"))
        };

        var created = _policies.Create(policy);
        Console.WriteLine($"Поліс створено. Вартість: {created.Cost:N2}");
    }

    private void ChangeCoverage()
    {
        var policy = ConsoleIO.Pick(_policies.GetAll().ToList(), "поліс");
        if (policy == null) return;

        policy.CoverageAmount = ConsoleIO.ReadDecimal($"Нова сума покриття [{policy.CoverageAmount:N0}]");
        _policies.Update(policy);
        Console.WriteLine($"Поліс оновлено. Нова вартість: {policy.Cost:N2}");
    }

    private void ChangeStatus()
    {
        var policy = ConsoleIO.Pick(_policies.GetAll().ToList(), "поліс");
        if (policy == null) return;

        var status = ConsoleIO.ReadEnum<PolicyStatus>("Новий статус");
        _policies.ChangeStatus(policy.Id, status);
        Console.WriteLine("Статус змінено.");
    }

    private void Search()
    {
        var criteria = new PolicySearchCriteria
        {
            Type = ConsoleIO.ReadOptionalEnum<PolicyType>("Тип"),
            Status = ConsoleIO.ReadOptionalEnum<PolicyStatus>("Статус"),
            MinCost = ConsoleIO.ReadOptionalDecimal("Мінімальна вартість"),
            MaxCost = ConsoleIO.ReadOptionalDecimal("Максимальна вартість")
        };

        if (ConsoleIO.Confirm("Фільтрувати за клієнтом?"))
            criteria.ClientId = ConsoleIO.Pick(_clients.GetAll().ToList(), "клієнта")?.Id;

        ConsoleIO.ShowAll(_policies.Search(criteria));
    }

    private void Delete()
    {
        var policy = ConsoleIO.Pick(_policies.GetAll().ToList(), "поліс");
        if (policy == null) return;

        if (ConsoleIO.Confirm($"Видалити поліс {policy.Number}?"))
        {
            _policies.Delete(policy.Id);
            Console.WriteLine("Поліс видалено.");
        }
    }

    private void ShowWithAgents()
    {
        var policies = _policies.GetAll().ToList();
        if (policies.Count == 0)
        {
            Console.WriteLine("Список порожній.");
            return;
        }

        var agents = _agents.GetAll().ToDictionary(a => a.Id, a => a.Name);

        for (var i = 0; i < policies.Count; i++)
        {
            var policy = policies[i];
            var agentName = policy.AgentId.HasValue && agents.TryGetValue(policy.AgentId.Value, out var name)
                ? name
                : "не призначено";

            Console.WriteLine($"{i + 1}. {policy} | агент: {agentName}");
        }
    }
}