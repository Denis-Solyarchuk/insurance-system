using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class PolicyService
{
    private readonly IRepository<Policy> _policies;
    private readonly IRepository<Client> _clients;

    // Базові тарифи: частка від суми покриття за рік
    private static readonly Dictionary<PolicyType, decimal> BaseRates = new()
    {
        [PolicyType.Auto] = 0.05m,
        [PolicyType.Medical] = 0.03m,
        [PolicyType.Property] = 0.02m
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
        return policy;
    }

    public IEnumerable<Policy> GetAll() => _policies.GetAll();

    public Policy? Get(Guid id) => _policies.GetById(id);

    public void Update(Policy policy)
    {
        policy.Cost = CalculateCost(policy.Type, policy.CoverageAmount, policy.StartDate, policy.EndDate);

        var errors = policy.Validate();
        if (errors.Any())
            throw new ValidationException(errors);

        _policies.Update(policy);
    }

    public bool Delete(Guid id) => _policies.Delete(id);
}