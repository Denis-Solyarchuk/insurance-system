using Insurance.Business;
using Insurance.Domain;
using Xunit;

namespace Insurance.Tests;

public class RequestServiceTests
{
    private readonly InMemoryRepository<Request> _requests = new(r => r.Id);
    private readonly InMemoryRepository<Client> _clients = new(c => c.Id);
    private readonly InMemoryRepository<Policy> _policies = new(p => p.Id);
    private readonly RequestService _service;

    public RequestServiceTests()
    {
        _service = new RequestService(
            _requests, _clients, _policies, new PolicyService(_policies, _clients));
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

    private void AddPolicy(
        PolicyType type, decimal coverage, int months, decimal cost,
        PolicyStatus status = PolicyStatus.Active)
    {
        var start = DateTime.Today;
        _policies.Add(new Policy
        {
            Number = Guid.NewGuid().ToString(),
            Type = type,
            CoverageAmount = coverage,
            StartDate = start,
            EndDate = start.AddMonths(months),
            Cost = cost,
            Status = status,
            ClientId = Guid.NewGuid()
        });
    }

    private Request NewRequest(Guid clientId) => _service.Create(new Request
    {
        ClientId = clientId,
        Type = PolicyType.Auto,
        DesiredCoverage = 100_000m,
        DurationMonths = 12
    });

    [Fact]
    public void FindMatches_ReturnsOnlySuitablePoliciesSortedByCost()
    {
        var client = AddClient();
        AddPolicy(PolicyType.Auto, 100_000m, 12, 5_000m);                          // підходить
        AddPolicy(PolicyType.Auto, 150_000m, 12, 4_000m);                          // підходить, дешевший
        AddPolicy(PolicyType.Auto, 50_000m, 12, 1_000m);                           // замале покриття
        AddPolicy(PolicyType.Medical, 100_000m, 12, 3_000m);                       // інший тип
        AddPolicy(PolicyType.Auto, 100_000m, 6, 2_500m);                           // замалий строк
        AddPolicy(PolicyType.Auto, 100_000m, 12, 5_000m, PolicyStatus.Suspended);  // неактивний

        var result = _service.FindMatches(NewRequest(client.Id).Id);

        Assert.Equal(2, result.Matches.Count);
        Assert.Equal(4_000m, result.Matches[0].Cost);
        Assert.Equal(5_000m, result.Matches[1].Cost);
    }

    [Fact]
    public void FindMatches_MarksRequestAsProcessed()
    {
        var client = AddClient();
        var request = NewRequest(client.Id);

        _service.FindMatches(request.Id);

        Assert.Equal(RequestStatus.Processed, _requests.GetById(request.Id)!.Status);
    }

    [Fact]
    public void FindMatches_ReturnsEstimatedCostForNewPolicy()
    {
        var client = AddClient();

        var result = _service.FindMatches(NewRequest(client.Id).Id);

        Assert.True(result.EstimatedCost > 0);
    }

    [Fact]
    public void Create_UnknownClient_ThrowsValidationException()
    {
        var request = new Request
        {
            ClientId = Guid.NewGuid(),
            DesiredCoverage = 1_000m,
            DurationMonths = 6
        };

        Assert.Throws<ValidationException>(() => _service.Create(request));
    }
}