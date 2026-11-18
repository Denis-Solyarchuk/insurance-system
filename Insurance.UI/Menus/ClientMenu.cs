using Insurance.Business;
using Insurance.Domain;

namespace Insurance.UI;

public class ClientMenu
{
    private readonly ClientService _clients;

    public ClientMenu(ClientService clients) => _clients = clients;

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n--- Клієнти ---");
            Console.WriteLine("1. Додати клієнта");
            Console.WriteLine("2. Список клієнтів");
            Console.WriteLine("3. Редагувати клієнта");
            Console.WriteLine("4. Видалити клієнта");
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
            case "2": ConsoleIO.ShowAll(_clients.GetAll()); break;
            case "3": Edit(); break;
            case "4": Delete(); break;
            default: Console.WriteLine("Невідомий пункт меню."); break;
        }
    }

    private void Add()
    {
        var client = new Client
        {
            FullName = ConsoleIO.ReadString("ПІБ"),
            Phone = ConsoleIO.ReadString("Телефон"),
            Email = ConsoleIO.ReadString("Email"),
            Type = ConsoleIO.ReadEnum<ClientType>("Тип клієнта")
        };

        _clients.Create(client);
        Console.WriteLine("Клієнта додано.");
    }

    private void Edit()
    {
        var client = ConsoleIO.Pick(_clients.GetAll().ToList(), "клієнта");
        if (client == null) return;

        Console.WriteLine("Enter - залишити без змін.");
        var name = ConsoleIO.ReadString($"ПІБ [{client.FullName}]");
        var phone = ConsoleIO.ReadString($"Телефон [{client.Phone}]");
        var email = ConsoleIO.ReadString($"Email [{client.Email}]");

        if (name != "") client.FullName = name;
        if (phone != "") client.Phone = phone;
        if (email != "") client.Email = email;

        _clients.Update(client);
        Console.WriteLine("Клієнта оновлено.");
    }

    private void Delete()
    {
        var client = ConsoleIO.Pick(_clients.GetAll().ToList(), "клієнта");
        if (client == null) return;

        if (ConsoleIO.Confirm($"Видалити клієнта {client.FullName}?"))
        {
            _clients.Delete(client.Id);
            Console.WriteLine("Клієнта видалено.");
        }
    }
}