using Insurance.Business;

namespace Insurance.UI;

public class QueryMenu
{
    private readonly SavedQueryService _queries;
    private readonly ClientService _clients;

    public QueryMenu(SavedQueryService queries, ClientService clients)
    {
        _queries = queries;
        _clients = clients;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n--- Запити клієнтів ---");
            Console.WriteLine("1. Зберегти запит клієнта");
            Console.WriteLine("2. Переглянути запити клієнта");
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
            case "1": Save(); break;
            case "2": Show(); break;
            default: Console.WriteLine("Невідомий пункт меню."); break;
        }
    }

    private void Save()
    {
        var client = ConsoleIO.Pick(_clients.GetAll().ToList(), "клієнта");
        if (client == null) return;

        var text = ConsoleIO.ReadString("Запит (наприклад: страхування авто до 10000 грн на 1 рік)");
        _queries.Save(client.Id, text);
        Console.WriteLine("Запит збережено.");
    }

    private void Show()
    {
        var client = ConsoleIO.Pick(_clients.GetAll().ToList(), "клієнта");
        if (client == null) return;

        ConsoleIO.ShowAll(_queries.GetByClient(client.Id));
    }
}