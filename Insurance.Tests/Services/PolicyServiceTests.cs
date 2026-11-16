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
}