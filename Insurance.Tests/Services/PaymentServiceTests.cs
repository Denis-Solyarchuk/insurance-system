using Insurance.Business;
using Insurance.Domain;
using Xunit;

namespace Insurance.Tests;

public class PaymentServiceTests
{
    private readonly InMemoryRepository<Payment> _payments = new(p => p.Id);
    private readonly InMemoryRepository<Policy> _policies = new(p => p.Id);
    private readonly InMemoryRepository<Claim> _claims = new(c => c.Id);
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _service = new PaymentService(_payments, _policies, _claims);
    }

    private Policy AddPolicy(decimal cost = 5_000m)
    {
        var policy = new Policy
        {
            Number = Guid.NewGuid().ToString(),
            Type = PolicyType.Auto,
            Cost = cost,
            CoverageAmount = 100_000m,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
            ClientId = Guid.NewGuid()
        };
        _policies.Add(policy);
        return policy;
    }

    private void AddClaim(Policy policy, ClaimStatus status, decimal amount) =>
        _claims.Add(new Claim
        {
            PolicyId = policy.Id,
            Description = "ДТП",
            Status = status,
            PayoutAmount = amount
        });

    private static Payment NewPayment(Guid policyId, PaymentType type, decimal amount) =>
        new() { PolicyId = policyId, Type = type, Amount = amount };

    [Fact]
    public void Register_ContributionWithinCost_IsSaved()
    {
        var policy = AddPolicy();

        _service.Register(NewPayment(policy.Id, PaymentType.Contribution, 2_000m));

        Assert.Single(_service.GetByPolicy(policy.Id));
    }

    [Fact]
    public void Register_ContributionAboveCost_ThrowsValidationException()
    {
        var policy = AddPolicy(5_000m);
        _service.Register(NewPayment(policy.Id, PaymentType.Contribution, 4_000m));

        Assert.Throws<ValidationException>(() =>
            _service.Register(NewPayment(policy.Id, PaymentType.Contribution, 2_000m)));
    }

    [Fact]
    public void Register_PayoutWithoutApprovedClaim_ThrowsValidationException()
    {
        var policy = AddPolicy();
        AddClaim(policy, ClaimStatus.New, 2_000m);

        Assert.Throws<ValidationException>(() =>
            _service.Register(NewPayment(policy.Id, PaymentType.Payout, 2_000m)));
    }

    [Fact]
    public void Register_PayoutForApprovedClaim_IsSaved()
    {
        var policy = AddPolicy();
        AddClaim(policy, ClaimStatus.Approved, 2_000m);

        _service.Register(NewPayment(policy.Id, PaymentType.Payout, 2_000m));

        Assert.Single(_service.GetByPolicy(policy.Id));
    }

    [Fact]
    public void Register_UnknownPolicy_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            _service.Register(NewPayment(Guid.NewGuid(), PaymentType.Contribution, 100m)));
    }

    [Fact]
    public void GetBalance_IsContributionsMinusPayouts()
    {
        var policy = AddPolicy(5_000m);
        AddClaim(policy, ClaimStatus.Approved, 2_000m);

        _service.Register(NewPayment(policy.Id, PaymentType.Contribution, 5_000m));
        _service.Register(NewPayment(policy.Id, PaymentType.Payout, 2_000m));

        Assert.Equal(3_000m, _service.GetBalance());
    }
}