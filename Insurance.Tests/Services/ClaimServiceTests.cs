using Insurance.Business;
using Insurance.Domain;
using Xunit;

namespace Insurance.Tests;

public class ClaimServiceTests
{
    private readonly InMemoryRepository<Claim> _claims = new(c => c.Id);
    private readonly InMemoryRepository<Policy> _policies = new(p => p.Id);
    private readonly ClaimService _service;

    public ClaimServiceTests()
    {
        _service = new ClaimService(_claims, _policies);
    }

    private Policy AddPolicy(PolicyStatus status = PolicyStatus.Active)
    {
        var policy = new Policy
        {
            Number = "AUTO-001",
            Type = PolicyType.Auto,
            ClientId = Guid.NewGuid(),
            CoverageAmount = 100_000m,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddYears(1),
            Status = status
        };
        _policies.Add(policy);
        return policy;
    }

    [Fact]
    public void Create_ValidClaim_IsSaved()
    {
        var policy = AddPolicy();

        _service.Create(new Claim
        {
            PolicyId = policy.Id,
            Description = "ДТП",
            PayoutAmount = 20_000m
        });

        Assert.Single(_service.GetByPolicy(policy.Id));
    }

    [Fact]
    public void Create_PolicyNotFound_ThrowsValidationException()
    {
        var claim = new Claim { PolicyId = Guid.NewGuid(), Description = "ДТП", PayoutAmount = 1m };

        Assert.Throws<ValidationException>(() => _service.Create(claim));
    }

    [Fact]
    public void Create_PayoutAboveCoverage_ThrowsValidationException()
    {
        var policy = AddPolicy();
        var claim = new Claim { PolicyId = policy.Id, Description = "ДТП", PayoutAmount = 500_000m };

        Assert.Throws<ValidationException>(() => _service.Create(claim));
    }

    [Fact]
    public void Create_SuspendedPolicy_ThrowsValidationException()
    {
        var policy = AddPolicy(PolicyStatus.Suspended);
        var claim = new Claim { PolicyId = policy.Id, Description = "ДТП", PayoutAmount = 1_000m };

        Assert.Throws<ValidationException>(() => _service.Create(claim));
    }
}