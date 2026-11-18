using Insurance.Business;
using Insurance.Domain;

namespace Insurance.UI;

public class ClaimMenu
{
    private readonly ClaimService _claims;
    private readonly PolicyService _policies;

    public ClaimMenu(ClaimService claims, PolicyService policies)
    {
        _claims = claims;
        _policies = policies;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n--- Страхові події ---");
            Console.WriteLine("1. Зареєструвати подію");
            Console.WriteLine("2. Події за полісом");
            Console.WriteLine("3. Змінити статус події");
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
            case "1": Register(); break;
            case "2": ShowByPolicy(); break;
            case "3": ChangeStatus(); break;
            default: Console.WriteLine("Невідомий пункт меню."); break;
        }
    }

    private void Register()
    {
        var policy = ConsoleIO.Pick(_policies.GetAll().ToList(), "поліс");
        if (policy == null) return;

        var claim = new Claim
        {
            PolicyId = policy.Id,
            Description = ConsoleIO.ReadString("Опис події"),
            PayoutAmount = ConsoleIO.ReadDecimal("Сума виплати")
        };

        _claims.Create(claim);
        Console.WriteLine("Подію зареєстровано.");
    }

    private void ShowByPolicy()
    {
        var policy = ConsoleIO.Pick(_policies.GetAll().ToList(), "поліс");
        if (policy == null) return;

        ConsoleIO.ShowAll(_claims.GetByPolicy(policy.Id));
    }

    private void ChangeStatus()
    {
        var policy = ConsoleIO.Pick(_policies.GetAll().ToList(), "поліс");
        if (policy == null) return;

        var claim = ConsoleIO.Pick(_claims.GetByPolicy(policy.Id).ToList(), "подію");
        if (claim == null) return;

        var status = ConsoleIO.ReadEnum<ClaimStatus>("Новий статус");
        _claims.ChangeStatus(claim.Id, status);
        Console.WriteLine("Статус змінено.");
    }
}