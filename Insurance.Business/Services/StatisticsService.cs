using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class StatisticsService
{
    private readonly IRepository<Policy> _policies;
    private readonly IRepository<Claim> _claims;
    private readonly IRepository<Agent> _agents;

    public StatisticsService(
        IRepository<Policy> policies,
        IRepository<Claim> claims,
        IRepository<Agent> agents)
    {
        _policies = policies;
        _claims = claims;
        _agents = agents;
    }

    public int CountActivePolicies() =>
        _policies.GetAll().Count(p => p.Status == PolicyStatus.Active);

    // Межі періоду включно
    public int CountClaimsInPeriod(DateTime from, DateTime to)
    {
        if (to < from)
            throw new ValidationException(new List<string> { "Кінець періоду раніше за початок." });

        var end = to.Date.AddDays(1);
        return _claims.GetAll().Count(c => c.Date >= from.Date && c.Date < end);
    }

    // Виплаченими вважаються події зі статусом Paid
    public decimal TotalPayouts() =>
        _claims.GetAll().Where(c => c.Status == ClaimStatus.Paid).Sum(c => c.PayoutAmount);

    // Внески = вартість усіх полісів
    public decimal TotalPremiums() => _policies.GetAll().Sum(p => p.Cost);

    // Дохід = внески - виплати
    public decimal CompanyIncome() => TotalPremiums() - TotalPayouts();

    public IReadOnlyList<AgentPerformance> GetAgentPerformance()
    {
        var policies = _policies.GetAll().ToList();

        return _agents.GetAll()
            .Select(agent =>
            {
                var sold = policies.Where(p => p.AgentId == agent.Id).ToList();
                var commission = Math.Round(sold.Sum(p => p.Cost) * agent.CommissionPercent / 100m, 2);
                return new AgentPerformance(agent, sold.Count, commission);
            })
            .OrderByDescending(x => x.PoliciesSold)
            .ToList();
    }

    public Dictionary<PolicyType, int> CountByType()
    {
        var policies = _policies.GetAll().ToList();
        return Enum.GetValues<PolicyType>()
            .ToDictionary(t => t, t => policies.Count(p => p.Type == t));
    }

    public IReadOnlyList<PolicyTypeReport> GetTypeReport()
    {
        var policies = _policies.GetAll().ToList();

        return Enum.GetValues<PolicyType>()
            .Select(type =>
            {
                var ofType = policies.Where(p => p.Type == type).ToList();
                return new PolicyTypeReport(
                    type,
                    ofType.Count(p => p.Status == PolicyStatus.Active),
                    ofType.Count(p => p.Status == PolicyStatus.Finished),
                    ofType.Count == 0 ? 0m : Math.Round(ofType.Average(p => p.Cost), 2));
            })
            .ToList();
    }
}