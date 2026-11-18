using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class ClaimService
{
    private readonly IRepository<Claim> _claims;
    private readonly IRepository<Policy> _policies;
    private readonly IRepository<Client> _clients;

    // Статуси події змінюються лише послідовно
    private static readonly Dictionary<ClaimStatus, ClaimStatus> NextStatus = new()
    {
        [ClaimStatus.New] = ClaimStatus.UnderReview,
        [ClaimStatus.UnderReview] = ClaimStatus.Approved,
        [ClaimStatus.Approved] = ClaimStatus.Paid
    };

    public ClaimService(
        IRepository<Claim> claims,
        IRepository<Policy> policies,
        IRepository<Client> clients)
    {
        _claims = claims;
        _policies = policies;
        _clients = clients;
    }

    public Claim Create(Claim claim)
    {
        var policy = _policies.GetById(claim.PolicyId)
            ?? throw new ValidationException(new List<string> { "Поліса не знайдено." });

        var errors = claim.Validate();

        if (policy.Status != PolicyStatus.Active)
            errors.Add("Страхову подію можна створити лише для активного поліса.");

        if (claim.Date < policy.StartDate || claim.Date > policy.EndDate)
            errors.Add("Дата події виходить за межі дії поліса.");

        if (claim.PayoutAmount > policy.CoverageAmount)
            errors.Add("Сума виплати перевищує суму покриття.");

        if (errors.Any())
            throw new ValidationException(errors);

        _claims.Add(claim);
        return claim;
    }

    public IEnumerable<Claim> GetAll() => _claims.GetAll();

    public IEnumerable<Claim> GetByPolicy(Guid policyId) =>
        _claims.GetAll().Where(c => c.PolicyId == policyId);

    public void ChangeStatus(Guid claimId, ClaimStatus newStatus)
    {
        var claim = _claims.GetById(claimId)
            ?? throw new ValidationException(new List<string> { "Подію не знайдено." });

        if (!NextStatus.TryGetValue(claim.Status, out var allowed) || allowed != newStatus)
            throw new ValidationException(new List<string>
            {
                $"Перехід {claim.Status} → {newStatus} недопустимий."
            });

        claim.Status = newStatus;
        _claims.Update(claim);

        if (newStatus == ClaimStatus.Paid)
            RefreshClientPayouts(claim.PolicyId);
    }

    // Автооновлення даних клієнта: загальна сума виплат
    private void RefreshClientPayouts(Guid policyId)
    {
        var policy = _policies.GetById(policyId);
        if (policy == null) return;

        var client = _clients.GetById(policy.ClientId);
        if (client == null) return;

        var policyIds = _policies.GetAll()
            .Where(p => p.ClientId == client.Id)
            .Select(p => p.Id)
            .ToHashSet();

        client.TotalPayouts = _claims.GetAll()
            .Where(c => c.Status == ClaimStatus.Paid && policyIds.Contains(c.PolicyId))
            .Sum(c => c.PayoutAmount);

        _clients.Update(client);
    }
}