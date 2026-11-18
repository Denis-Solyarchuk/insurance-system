using Insurance.Business;
using Insurance.Domain;

namespace Insurance.UI;

public class PaymentMenu
{
    private readonly PaymentService _payments;
    private readonly PolicyService _policies;

    public PaymentMenu(PaymentService payments, PolicyService policies)
    {
        _payments = payments;
        _policies = policies;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n--- Платежі ---");
            Console.WriteLine("1. Зареєструвати платіж");
            Console.WriteLine("2. Платежі за полісом");
            Console.WriteLine("3. Баланс (внески - виплати)");
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
            case "3": ShowBalance(); break;
            default: Console.WriteLine("Невідомий пункт меню."); break;
        }
    }

    private void Register()
    {
        var policy = ConsoleIO.Pick(_policies.GetAll().ToList(), "поліс");
        if (policy == null) return;

        var payment = new Payment
        {
            PolicyId = policy.Id,
            Type = ConsoleIO.ReadEnum<PaymentType>("Тип платежу"),
            Amount = ConsoleIO.ReadDecimal("Сума")
        };

        _payments.Register(payment);
        Console.WriteLine("Платіж зареєстровано.");
    }

    private void ShowByPolicy()
    {
        var policy = ConsoleIO.Pick(_policies.GetAll().ToList(), "поліс");
        if (policy == null) return;

        ConsoleIO.ShowAll(_payments.GetByPolicy(policy.Id));
    }

    private void ShowBalance()
    {
        Console.WriteLine($"Внески: {_payments.GetTotal(PaymentType.Contribution):N2}");
        Console.WriteLine($"Виплати: {_payments.GetTotal(PaymentType.Payout):N2}");
        Console.WriteLine($"Баланс: {_payments.GetBalance():N2}");
    }
}