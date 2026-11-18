using Insurance.Business;
using Insurance.Domain;
using Xunit;

namespace Insurance.Tests;

public class StatisticsServiceTests
{
    private readonly InMemoryRepository<Policy> _policies = new(p => p.Id);
    private readonly InMemoryRepository<Claim> _claims = new(c => c.Id);
    private readonly InMemoryRepository<Agent> _agents = new(a => a.Id);
    private readonly StatisticsService _service;

    public StatisticsServiceTests()
    {
        _service = new StatisticsService(_policies, _claims, _agents);
    }

    private Policy AddPolicy(PolicyStatus status, decimal cost, Guid? agentId = null)
    {
        var policy = new Policy
        {
            Number = Guid.NewGuid().ToString(),
            Type = PolicyType.Auto,
            Status = status,
            Cost = cost,
            CoverageAmount = 100_000m,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
            ClientId = Guid.NewGuid(),
            AgentId = agentId
        };
        _policies.Add(policy);
        return policy;
    }

    private void AddClaim(ClaimStatus status, decimal amount, DateTime date) =>
        _claims.Add(new Claim
        {
            PolicyId = Guid.NewGuid(),
            Description = "Подія",
            Status = status,
            PayoutAmount = amount,
            Date = date
        });

    [Fact]
    public void CountActivePolicies_IgnoresOtherStatuses()
    {
        AddPolicy(PolicyStatus.Active, 1_000m);
        AddPolicy(PolicyStatus.Active, 1_000m);
        AddPolicy(PolicyStatus.Suspended, 1_000m);
        AddPolicy(PolicyStatus.Finished, 1_000m);

        Assert.Equal(2, _service.CountActivePolicies());
    }

    [Fact]
    public void TotalPayouts_CountsOnlyPaidClaims()
    {
        AddClaim(ClaimStatus.Paid, 2_000m, DateTime.Today);
        AddClaim(ClaimStatus.Approved, 9_000m, DateTime.Today);
        AddClaim(ClaimStatus.New, 7_000m, DateTime.Today);

        Assert.Equal(2_000m, _service.TotalPayouts());
    }

    [Fact]
    public void CompanyIncome_IsPremiumsMinusPayouts()
    {
        AddPolicy(PolicyStatus.Active, 5_000m);
        AddPolicy(PolicyStatus.Active, 3_000m);
        AddClaim(ClaimStatus.Paid, 2_000m, DateTime.Today);

        Assert.Equal(6_000m, _service.CompanyIncome());
    }

    [Fact]
    public void CountClaimsInPeriod_CountsOnlyClaimsInsideRange()
    {
        AddClaim(ClaimStatus.New, 1m, new DateTime(2026, 1, 10));
        AddClaim(ClaimStatus.New, 1m, new DateTime(2026, 1, 31, 18, 0, 0));
        AddClaim(ClaimStatus.New, 1m, new DateTime(2026, 2, 10));

        var count = _service.CountClaimsInPeriod(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

        Assert.Equal(2, count);
    }

    [Fact]
    public void CountClaimsInPeriod_EndBeforeStart_Throws()
    {
        Assert.Throws<ValidationException>(() =>
            _service.CountClaimsInPeriod(new DateTime(2026, 2, 1), new DateTime(2026, 1, 1)));
    }

    [Fact]
    public void GetAgentPerformance_CalculatesSoldPoliciesAndCommission()
    {
        var agent = new Agent { Name = "Петренко", CommissionPercent = 10m };
        _agents.Add(agent);
        AddPolicy(PolicyStatus.Active, 5_000m, agent.Id);
        AddPolicy(PolicyStatus.Active, 3_000m, agent.Id);
        AddPolicy(PolicyStatus.Active, 9_000m);

        var report = _service.GetAgentPerformance().Single();

        Assert.Equal(2, report.PoliciesSold);
        Assert.Equal(800m, report.Commission);
    }
}