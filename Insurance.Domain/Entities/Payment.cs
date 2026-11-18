namespace Insurance.Domain;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public Guid PolicyId { get; set; }

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (Amount <= 0)
            errors.Add("Сума платежу має бути більшою за 0.");

        if (PolicyId == Guid.Empty)
            errors.Add("Платіж має бути прив'язаний до поліса.");

        return errors;
    }

    public override string ToString() => $"{Date:d} | {Type} | {Amount:N2}";
}