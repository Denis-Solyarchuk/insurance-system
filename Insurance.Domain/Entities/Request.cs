namespace Insurance.Domain;

public class Request
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClientId { get; set; }
    public PolicyType Type { get; set; }
    public decimal DesiredCoverage { get; set; }
    public int DurationMonths { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (ClientId == Guid.Empty)
            errors.Add("Запит має бути прив'язаний до клієнта.");

        if (DesiredCoverage <= 0)
            errors.Add("Бажана сума покриття має бути більшою за 0.");

        if (DurationMonths < 1 || DurationMonths > 120)
            errors.Add("Строк має бути від 1 до 120 місяців.");

        return errors;
    }

    public override string ToString() =>
        $"{CreatedAt:d} | {Type} | покриття {DesiredCoverage:N0} | {DurationMonths} міс. | {Status}";
}