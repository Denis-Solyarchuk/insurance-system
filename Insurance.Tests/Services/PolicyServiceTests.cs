using Insurance.Business;
using Insurance.Domain;
using Xunit;

namespace Insurance.Tests;

public class PolicyServiceTests
{
    private readonly InMemoryRepository<Policy> _policies = new(p => p.Id);
    private readonly InMemoryRepository<Client> _clients = new(c => c.Id);
    private readonly PolicyService _service;

    public PolicyServiceTests()
    {
        _service = new PolicyService(_policies, _clients);
    }

    private Client AddClient()
    {
        var client = new Client
        {
            FullName = "Іваненко Іван",
            Phone = "+380501234567",
            Email = "ivan@mail.com"
        };
        _clients.Add(client);
        return client;
    }

    private static Policy NewPolicy(Guid clientId, string number = "AUTO-001") => new()
    {
        Number = number,
        Type = PolicyType.Auto,
        ClientId = clientId,
        CoverageAmount = 100_000m,
        StartDate = new DateTime(2026, 1, 1),
        EndDate = new DateTime(2027, 1, 1)
    };

    [Fact]
    public void CalculateCost_AutoForOneYear_IsFivePercentOfCoverage()
    {
        var cost = _service.CalculateCost(
            PolicyType.Auto, 100_000m, new DateTime(2026, 1, 1), new DateTime(2027, 1, 1));

        Assert.Equal(5_000m, cost);
    }

    [Fact]
    public void CalculateCost_PropertyFor73Days_IsProportionalToPeriod()
    {
        var start = new DateTime(2026, 1, 1);

        var cost = _service.CalculateCost(PolicyType.Property, 100_000m, start, start.AddDays(73));

        // 100000 * 2% * 73/365 = 400
        Assert.Equal(400m, cost);
    }

    [Fact]
    public void Create_ValidPolicy_SetsCostAndSaves()
    {
        var client = AddClient();

        var created = _service.Create(NewPolicy(client.Id));

        Assert.Equal(5_000m, created.Cost);
        Assert.Single(_service.GetAll());
    }

    [Fact]
    public void Create_UnknownClient_ThrowsValidationException()
    {
        var policy = NewPolicy(Guid.NewGuid());

        Assert.Throws<ValidationException>(() => _service.Create(policy));
    }

    [Fact]
    public void Create_DuplicateNumber_ThrowsValidationException()
    {
        var client = AddClient();
        _service.Create(NewPolicy(client.Id, "AUTO-001"));

        Assert.Throws<ValidationException>(() => _service.Create(NewPolicy(client.Id, "AUTO-001")));
    }

    [Fact]
    public void Create_EndDateBeforeStart_ThrowsValidationException()
    {
        var client = AddClient();
        var policy = NewPolicy(client.Id);
        policy.EndDate = policy.StartDate.AddDays(-1);

        Assert.Throws<ValidationException>(() => _service.Create(policy));
    }

    [Fact]
    public void ChangeStatus_ActiveToSuspended_Succeeds()
    {
        var client = AddClient();
        var policy = _service.Create(NewPolicy(client.Id));

        _service.ChangeStatus(policy.Id, PolicyStatus.Suspended);

        Assert.Equal(PolicyStatus.Suspended, _service.Get(policy.Id)!.Status);
    }

    [Fact]
    public void ChangeStatus_FromFinished_ThrowsValidationException()
    {
        var client = AddClient();
        var policy = _service.Create(NewPolicy(client.Id));
        _service.ChangeStatus(policy.Id, PolicyStatus.Finished);

        Assert.Throws<ValidationException>(() =>
            _service.ChangeStatus(policy.Id, PolicyStatus.Active));
    }

    [Fact]
    public void Create_And_Delete_UpdateClientPolicyCount()
    {
        var client = AddClient();
        var policy = _service.Create(NewPolicy(client.Id));
        Assert.Equal(1, _clients.GetById(client.Id)!.PolicyCount);

        _service.Delete(policy.Id);
        Assert.Equal(0, _clients.GetById(client.Id)!.PolicyCount);
    }

    [Fact]
    public void Search_ByMaxCost_ReturnsOnlyCheaperPolicies()
    {
        var client = AddClient();
        _service.Create(NewPolicy(client.Id, "A-1"));              // авто: 5000

        var medical = NewPolicy(client.Id, "M-1");
        medical.Type = PolicyType.Medical;                         // медичне: 3000
        _service.Create(medical);

        var result = _service.Search(new PolicySearchCriteria { MaxCost = 4_000m }).ToList();

        Assert.Single(result);
        Assert.Equal("M-1", result[0].Number);
    }
}