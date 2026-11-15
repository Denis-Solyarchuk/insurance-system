namespace Insurance.Domain;

public class Claim
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.Now;
    public string Description { get; set; } = "";
    public decimal PayoutAmount { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.New;
    public Guid PolicyId { get; set; }

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Description))
            errors.Add("Опис події обов'язковий.");

        if (PayoutAmount < 0)
            errors.Add("Сума виплати не може бути від'ємною.");

        if (PolicyId == Guid.Empty)
            errors.Add("Подія має бути прив'язана до поліса.");

        return errors;
    }

    public override string ToString() =>
        $"{Date:d} | {Description} | виплата {PayoutAmount:N2} | {Status}";
}