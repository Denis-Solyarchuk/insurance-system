using System.Globalization;
using System.Text;
using Insurance.Business;
using Insurance.Domain;
using Insurance.Persistence;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// ---------- Збирання залежностей ----------
var dataDir = Path.Combine(AppContext.BaseDirectory, "data");

var clientRepo = new JsonRepository<Client>(Path.Combine(dataDir, "clients.json"), c => c.Id);
var policyRepo = new JsonRepository<Policy>(Path.Combine(dataDir, "policies.json"), p => p.Id);
var claimRepo  = new JsonRepository<Claim>(Path.Combine(dataDir, "claims.json"), c => c.Id);

var clients  = new ClientService(clientRepo);
var policies = new PolicyService(policyRepo, clientRepo);
var claims   = new ClaimService(claimRepo, policyRepo);

// ---------- Головний цикл ----------
while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== Страхова компанія ===");
    Console.WriteLine("1.  Додати клієнта");
    Console.WriteLine("2.  Список клієнтів");
    Console.WriteLine("3.  Редагувати клієнта");
    Console.WriteLine("4.  Видалити клієнта");
    Console.WriteLine("5.  Створити поліс");
    Console.WriteLine("6.  Список полісів");
    Console.WriteLine("7.  Змінити суму покриття поліса");
    Console.WriteLine("8.  Видалити поліс");
    Console.WriteLine("9.  Зареєструвати страхову подію");
    Console.WriteLine("10. Список страхових подій");
    Console.WriteLine("0.  Вихід");
    Console.Write("> ");

    try
    {
        switch (Console.ReadLine()?.Trim())
        {
            case "1":  AddClient(); break;
            case "2":  ShowAll(clients.GetAll()); break;
            case "3":  EditClient(); break;
            case "4":  DeleteClient(); break;
            case "5":  AddPolicy(); break;
            case "6":  ShowAll(policies.GetAll()); break;
            case "7":  EditPolicyCoverage(); break;
            case "8":  DeletePolicy(); break;
            case "9":  AddClaim(); break;
            case "10": ShowClaims(); break;
            case "0":  return;
            default:   Console.WriteLine("Невідомий пункт меню."); break;
        }
    }
    catch (ValidationException ex)
    {
        Console.WriteLine("Помилка валідації:");
        foreach (var error in ex.Errors)
            Console.WriteLine($" - {error}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Помилка: {ex.Message}");
    }
}

// ---------- Дії меню ----------
void AddClient()
{
    var client = new Client
    {
        FullName = ReadString("ПІБ"),
        Phone = ReadString("Телефон"),
        Email = ReadString("Email"),
        Type = ReadString("Тип (1 - фіз. особа, 2 - юр. особа)") == "2"
            ? ClientType.LegalEntity
            : ClientType.Individual
    };

    clients.Create(client);
    Console.WriteLine("Клієнта додано.");
}

void EditClient()
{
    var client = Pick(clients.GetAll().ToList(), "клієнта");
    if (client == null) return;

    Console.WriteLine("Залиште поле порожнім (Enter), щоб не змінювати.");
    var name = ReadString($"ПІБ [{client.FullName}]");
    var phone = ReadString($"Телефон [{client.Phone}]");
    var email = ReadString($"Email [{client.Email}]");

    if (name != "") client.FullName = name;
    if (phone != "") client.Phone = phone;
    if (email != "") client.Email = email;

    clients.Update(client);
    Console.WriteLine("Клієнта оновлено.");
}

void DeleteClient()
{
    var client = Pick(clients.GetAll().ToList(), "клієнта");
    if (client == null) return;

    if (Confirm($"Видалити клієнта {client.FullName}?"))
    {
        clients.Delete(client.Id);
        Console.WriteLine("Клієнта видалено.");
    }
}

void AddPolicy()
{
    var client = Pick(clients.GetAll().ToList(), "клієнта");
    if (client == null) return;

    var policy = new Policy
    {
        ClientId = client.Id,
        Number = ReadString("Номер поліса"),
        Type = ReadString("Тип (1 - авто, 2 - медичне, 3 - майно)") switch
        {
            "2" => PolicyType.Medical,
            "3" => PolicyType.Property,
            _ => PolicyType.Auto
        },
        CoverageAmount = ReadDecimal("Сума покриття"),
        StartDate = DateTime.Today
    };
    policy.EndDate = policy.StartDate.AddMonths(ReadInt("Тривалість (місяців)"));

    var created = policies.Create(policy);
    Console.WriteLine($"Поліс створено. Вартість: {created.Cost:N2}");
}

void EditPolicyCoverage()
{
    var policy = Pick(policies.GetAll().ToList(), "поліс");
    if (policy == null) return;

    policy.CoverageAmount = ReadDecimal($"Нова сума покриття [{policy.CoverageAmount:N0}]");
    policies.Update(policy);
    Console.WriteLine($"Поліс оновлено. Нова вартість: {policy.Cost:N2}");
}

void DeletePolicy()
{
    var policy = Pick(policies.GetAll().ToList(), "поліс");
    if (policy == null) return;

    if (Confirm($"Видалити поліс {policy.Number}?"))
    {
        policies.Delete(policy.Id);
        Console.WriteLine("Поліс видалено.");
    }
}

void AddClaim()
{
    var policy = Pick(policies.GetAll().ToList(), "поліс");
    if (policy == null) return;

    var claim = new Claim
    {
        PolicyId = policy.Id,
        Description = ReadString("Опис події"),
        PayoutAmount = ReadDecimal("Сума виплати")
    };

    claims.Create(claim);
    Console.WriteLine("Подію зареєстровано.");
}

void ShowClaims()
{
    var policy = Pick(policies.GetAll().ToList(), "поліс");
    if (policy == null) return;

    ShowAll(claims.GetByPolicy(policy.Id));
}

// ---------- Допоміжні методи вводу/виводу ----------
void ShowAll<T>(IEnumerable<T> items)
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

T? Pick<T>(List<T> list, string what) where T : class
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

string ReadString(string prompt)
{
    Console.Write($"{prompt}: ");
    return Console.ReadLine()?.Trim() ?? "";
}

decimal ReadDecimal(string prompt)
{
    while (true)
    {
        var text = ReadString(prompt).Replace(',', '.');
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return value;
        Console.WriteLine("Введіть число.");
    }
}

int ReadInt(string prompt)
{
    while (true)
    {
        if (int.TryParse(ReadString(prompt), out var value))
            return value;
        Console.WriteLine("Введіть ціле число.");
    }
}

bool Confirm(string question)
{
    Console.Write($"{question} (y/n): ");
    return Console.ReadLine()?.Trim().ToLower() == "y";
}