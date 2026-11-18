using System.Text.RegularExpressions;

namespace Insurance.Domain;

public class Client
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public ClientType Type { get; set; }

    // Оновлюються автоматично сервісами
    public int PolicyCount { get; set; }
    public decimal TotalPayouts { get; set; }

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(FullName) || FullName.Trim().Length < 3)
            errors.Add("ПІБ має містити щонайменше 3 символи.");

        if (!Regex.IsMatch(Phone ?? "", @"^\+?\d{10,13}$"))
            errors.Add("Телефон має містити 10–13 цифр (можливий префікс +).");

        if (!Regex.IsMatch(Email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errors.Add("Некоректний email.");

        return errors;
    }

    public override string ToString() =>
        $"{Id.ToString()[..8]} | {FullName} | {Type} | {Phone} | {Email} | полісів: {PolicyCount} | виплат: {TotalPayouts:N2}";
}