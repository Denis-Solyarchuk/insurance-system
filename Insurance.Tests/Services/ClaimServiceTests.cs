using Insurance.Business;
using Insurance.Domain;
using Xunit;

namespace Insurance.Tests;

public class ClaimServiceTests
{
    private readonly InMemoryRepository<Claim> _claims = new(c => c.Id);
    private readonly InMemoryRepository<Policy> _policies = new(p => p.Id);
    private readonly InMemoryRepository<Client> _clients = new(c => c.Id);
    private readonly ClaimService _service;

    public ClaimServiceTests()
    {
        _service = new ClaimService(_claims, _policies, _clients);
    }

    private Policy AddPolicy(PolicyStatus status = PolicyStatus.Active)
    {
        var client = new Client
        {
            FullName = "Іваненко Іван",
            Phone = "+380501234567",
            Email = "ivan@mail.com"
        };
        _clients.Add(client);

        var policy = new Policy
        {
            Number = "AUTO-001",
            Type = PolicyType.Auto,
            ClientId = client.Id,
            CoverageAmount = 100_000m,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddYears(1),
            Status = status
        };
        _policies.Add(policy);
        return policy;
    }

    private Claim AddClaim(Policy policy, decimal amount = 20_000m) =>
        _service.Create(new Claim { PolicyId = policy.Id, Description = "ДТП", PayoutAmount = amount });

    private void Advance(Guid claimId, params ClaimStatus[] statuses)
    {
        foreach (var status in statuses)
            _service.ChangeStatus(claimId, status);
    }

    [Fact]
    public void Create_ValidClaim_IsSaved()
    {
        var policy = AddPolicy();
        AddClaim(policy);

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

        Assert.Throws<ValidationException>(() => AddClaim(policy, 500_000m));
    }

    [Fact]
    public void Create_SuspendedPolicy_ThrowsValidationException()
    {
        var policy = AddPolicy(PolicyStatus.Suspended);

        Assert.Throws<ValidationException>(() => AddClaim(policy, 1_000m));
    }

    [Fact]
    public void ChangeStatus_FollowingChain_ReachesPaid()
    {
        var claim = AddClaim(AddPolicy());

        Advance(claim.Id, ClaimStatus.UnderReview, ClaimStatus.Approved, ClaimStatus.Paid);

        Assert.Equal(ClaimStatus.Paid, _claims.GetById(claim.Id)!.Status);
    }

    [Fact]
    public void ChangeStatus_SkippingStep_ThrowsValidationException()
    {
        var claim = AddClaim(AddPolicy());

        Assert.Throws<ValidationException>(() => _service.ChangeStatus(claim.Id, ClaimStatus.Approved));
    }

    [Fact]
    public void ChangeStatus_ToPaid_UpdatesClientTotalPayouts()
    {
        var policy = AddPolicy();
        var claim = AddClaim(policy, 20_000m);

        Advance(claim.Id, ClaimStatus.UnderReview, ClaimStatus.Approved, ClaimStatus.Paid);

        Assert.Equal(20_000m, _clients.GetById(policy.ClientId)!.TotalPayouts);
    }
}