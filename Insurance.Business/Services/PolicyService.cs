using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class PolicyService
{
    private readonly IRepository<Policy> _policies;
    private readonly IRepository<Client> _clients;

    private static readonly Dictionary<PolicyType, decimal> BaseRates = new()
    {
        [PolicyType.Auto] = 0.05m,
        [PolicyType.Medical] = 0.03m,
        [PolicyType.Property] = 0.02m
    };

    // Допустимі переходи між статусами полісів
    private static readonly Dictionary<PolicyStatus, PolicyStatus[]> AllowedTransitions = new()
    {
        [PolicyStatus.Active] = new[] { PolicyStatus.Suspended, PolicyStatus.Finished },
        [PolicyStatus.Suspended] = new[] { PolicyStatus.Active, PolicyStatus.Finished },
        [PolicyStatus.Finished] = Array.Empty<PolicyStatus>()
    };

    public PolicyService(IRepository<Policy> policies, IRepository<Client> clients)
    {
        _policies = policies;
        _clients = clients;
    }

    // Формула: покриття * тариф * (кількість днів / 365)
    public decimal CalculateCost(PolicyType type, decimal coverage, DateTime start, DateTime end)
    {
        var days = (decimal)(end - start).TotalDays;
        return Math.Round(coverage * BaseRates[type] * days / 365m, 2);
    }

    public Policy Create(Policy policy)
    {
        var errors = new List<string>();

        if (_clients.GetById(policy.ClientId) == null)
            errors.Add("Клієнта з таким Id не існує.");

        if (_policies.GetAll().Any(p => p.Number == policy.Number))
            errors.Add("Поліс з таким номером уже існує.");

        policy.Cost = CalculateCost(policy.Type, policy.CoverageAmount, policy.StartDate, policy.EndDate);
        errors.AddRange(policy.Validate());

        if (errors.Any())
            throw new ValidationException(errors);

        _policies.Add(policy);
        RefreshClientStats(policy.ClientId);
        return policy;
    }

    public IEnumerable<Policy> GetAll() => _policies.GetAll();

    public Policy? Get(Guid id) => _policies.GetById(id);

    public void Update(Policy policy)
    {
        var stored = _policies.GetById(policy.Id)
            ?? throw new ValidationException(new List<string> { "Поліс не знайдено." });

        if (stored.Status == PolicyStatus.Finished)
            throw new ValidationException(new List<string> { "Завершений поліс не можна змінювати." });

        policy.Cost = CalculateCost(policy.Type, policy.CoverageAmount, policy.StartDate, policy.EndDate);

        var errors = policy.Validate();
        if (errors.Any())
            throw new ValidationException(errors);

        _policies.Update(policy);
    }

    public void ChangeStatus(Guid policyId, PolicyStatus newStatus)
    {
        var policy = _policies.GetById(policyId)
            ?? throw new ValidationException(new List<string> { "Поліс не знайдено." });

        if (!AllowedTransitions[policy.Status].Contains(newStatus))
            throw new ValidationException(new List<string>
            {
                $"Перехід {policy.Status} → {newStatus} недопустимий."
            });

        policy.Status = newStatus;
        _policies.Update(policy);
    }

    public IEnumerable<Policy> Search(PolicySearchCriteria criteria)
    {
        var query = _policies.GetAll();

        if (criteria.Type.HasValue)
            query = query.Where(p => p.Type == criteria.Type.Value);
        if (criteria.ClientId.HasValue)
            query = query.Where(p => p.ClientId == criteria.ClientId.Value);
        if (criteria.Status.HasValue)
            query = query.Where(p => p.Status == criteria.Status.Value);
        if (criteria.MinCost.HasValue)
            query = query.Where(p => p.Cost >= criteria.MinCost.Value);
        if (criteria.MaxCost.HasValue)
            query = query.Where(p => p.Cost <= criteria.MaxCost.Value);

        return query.ToList();
    }

    public bool Delete(Guid id)
    {
        var policy = _policies.GetById(id);
        if (policy == null) return false;

        var removed = _policies.Delete(id);
        if (removed) RefreshClientStats(policy.ClientId);
        return removed;
    }

    // Автооновлення даних клієнта: кількість полісів
    private void RefreshClientStats(Guid clientId)
    {
        var client = _clients.GetById(clientId);
        if (client == null) return;

        client.PolicyCount = _policies.GetAll().Count(p => p.ClientId == clientId);
        _clients.Update(client);
    }
}