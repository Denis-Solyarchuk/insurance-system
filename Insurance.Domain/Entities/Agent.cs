namespace Insurance.Domain;

public class Agent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public decimal CommissionPercent { get; set; }

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Ім'я агента обов'язкове.");

        if (CommissionPercent < 0 || CommissionPercent > 100)
            errors.Add("Комісія має бути в межах 0–100%.");

        return errors;
    }

    public override string ToString() =>
        $"{Id.ToString()[..8]} | {Name} | комісія {CommissionPercent}%";
}