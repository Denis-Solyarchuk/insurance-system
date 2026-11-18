using Insurance.Business;
using Insurance.Domain;

namespace Insurance.UI;

public class AgentMenu
{
    private readonly AgentService _agents;
    private readonly PolicyService _policies;

    public AgentMenu(AgentService agents, PolicyService policies)
    {
        _agents = agents;
        _policies = policies;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n--- Агенти ---");
            Console.WriteLine("1. Додати агента");
            Console.WriteLine("2. Список агентів");
            Console.WriteLine("3. Змінити комісію");
            Console.WriteLine("4. Призначити агента на поліс");
            Console.WriteLine("5. Поліси агента");
            Console.WriteLine("6. Відкріпити агента від поліса");
            Console.WriteLine("7. Видалити агента");
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
            case "2": ConsoleIO.ShowAll(_agents.GetAll()); break;
            case "3": ChangeCommission(); break;
            case "4": Assign(); break;
            case "5": ShowPolicies(); break;
            case "6": Unassign(); break;
            case "7": Delete(); break;
            default: Console.WriteLine("Невідомий пункт меню."); break;
        }
    }

    private void Add()
    {
        var agent = new Agent
        {
            Name = ConsoleIO.ReadString("Ім'я агента"),
            CommissionPercent = ConsoleIO.ReadDecimal("Комісія, %")
        };

        _agents.Create(agent);
        Console.WriteLine("Агента додано.");
    }

    private void ChangeCommission()
    {
        var agent = ConsoleIO.Pick(_agents.GetAll().ToList(), "агента");
        if (agent == null) return;

        agent.CommissionPercent = ConsoleIO.ReadDecimal($"Нова комісія, % [{agent.CommissionPercent}]");
        _agents.Update(agent);
        Console.WriteLine("Комісію оновлено.");
    }

    private void Assign()
    {
        var agent = ConsoleIO.Pick(_agents.GetAll().ToList(), "агента");
        if (agent == null) return;

        var policy = ConsoleIO.Pick(_policies.GetAll().ToList(), "поліс");
        if (policy == null) return;

        _agents.AssignToPolicy(agent.Id, policy.Id);
        Console.WriteLine("Агента призначено.");
    }

    private void ShowPolicies()
    {
        var agent = ConsoleIO.Pick(_agents.GetAll().ToList(), "агента");
        if (agent == null) return;

        Console.WriteLine($"Поліси агента {agent.Name}:");
        ConsoleIO.ShowAll(_agents.GetPolicies(agent.Id));
    }

    private void Unassign()
    {
        var agent = ConsoleIO.Pick(_agents.GetAll().ToList(), "агента");
        if (agent == null) return;

        var policy = ConsoleIO.Pick(_agents.GetPolicies(agent.Id).ToList(), "поліс для відкріплення");
        if (policy == null) return;

        _agents.UnassignFromPolicy(policy.Id);
        Console.WriteLine("Агента відкріплено від поліса.");
    }

    private void Delete()
    {
        var agent = ConsoleIO.Pick(_agents.GetAll().ToList(), "агента");
        if (agent == null) return;

        var assigned = _agents.GetPolicies(agent.Id).ToList();

        if (assigned.Count > 0)
        {
            Console.WriteLine($"За агентом {agent.Name} закріплено полісів: {assigned.Count}");
            ConsoleIO.ShowAll(assigned);

            if (!ConsoleIO.Confirm("Відкріпити агента від цих полісів і видалити його?"))
            {
                Console.WriteLine("Скасовано.");
                return;
            }

            _agents.Delete(agent.Id, unassignPolicies: true);
            Console.WriteLine("Агента відкріплено від полісів і видалено.");
            return;
        }

        if (ConsoleIO.Confirm($"Видалити агента {agent.Name}?"))
        {
            _agents.Delete(agent.Id);
            Console.WriteLine("Агента видалено.");
        }
    }
}