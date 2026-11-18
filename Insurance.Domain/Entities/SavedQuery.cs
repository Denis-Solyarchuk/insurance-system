namespace Insurance.Domain;

public class SavedQuery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClientId { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Text))
            errors.Add("Текст запиту обов'язковий.");
        else if (Text.Length > 200)
            errors.Add("Запит має бути не довшим за 200 символів.");

        if (ClientId == Guid.Empty)
            errors.Add("Запит має бути прив'язаний до клієнта.");

        return errors;
    }

    public override string ToString() => $"{CreatedAt:g} | {Text}";
}