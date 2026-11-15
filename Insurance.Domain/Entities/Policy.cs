namespace Insurance.Domain;

public class Policy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Number { get; set; } = "";
    public PolicyType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal Cost { get; set; }
    public PolicyStatus Status { get; set; } = PolicyStatus.Active;

    public Guid ClientId { get; set; }
    public Guid? AgentId { get; set; }

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Number))
            errors.Add("Номер поліса обов'язковий.");

        if (EndDate <= StartDate)
            errors.Add("Дата завершення має бути пізніше за дату початку.");

        if (CoverageAmount <= 0)
            errors.Add("Сума покриття має бути більшою за 0.");

        if (Cost < 0)
            errors.Add("Вартість не може бути від'ємною.");

        if (ClientId == Guid.Empty)
            errors.Add("Поліс має бути прив'язаний до клієнта.");

        return errors;
    }

    public override string ToString() =>
        $"{Number} | {Type} | {StartDate:d} – {EndDate:d} | покриття {CoverageAmount:N0} | вартість {Cost:N2} | {Status}";
}