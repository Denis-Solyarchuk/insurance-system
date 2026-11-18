using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class RequestService
{
    private readonly IRepository<Request> _requests;
    private readonly IRepository<Client> _clients;
    private readonly IRepository<Policy> _policies;
    private readonly PolicyService _policyService;

    public RequestService(
        IRepository<Request> requests,
        IRepository<Client> clients,
        IRepository<Policy> policies,
        PolicyService policyService)
    {
        _requests = requests;
        _clients = clients;
        _policies = policies;
        _policyService = policyService;
    }

    public Request Create(Request request)
    {
        var errors = request.Validate();
        if (_clients.GetById(request.ClientId) == null)
            errors.Add("Клієнта не знайдено.");

        if (errors.Any())
            throw new ValidationException(errors);

        _requests.Add(request);
        return request;
    }

    public IEnumerable<Request> GetAll() => _requests.GetAll();

    public IEnumerable<Request> GetByClient(Guid clientId) =>
        _requests.GetAll().Where(r => r.ClientId == clientId).ToList();

    // Підбір: активні поліси того ж типу, з покриттям і строком не меншими за бажані
    public RequestMatchResult FindMatches(Guid requestId)
    {
        var request = _requests.GetById(requestId)
            ?? throw new ValidationException(new List<string> { "Запит не знайдено." });

        var today = DateTime.Today;
        var requestedEnd = today.AddMonths(request.DurationMonths);
        var requiredDays = (requestedEnd - today).TotalDays;

        var matches = _policies.GetAll()
            .Where(p => p.Status == PolicyStatus.Active
                     && p.Type == request.Type
                     && p.CoverageAmount >= request.DesiredCoverage
                     && (p.EndDate - p.StartDate).TotalDays >= requiredDays)
            .OrderBy(p => p.Cost)
            .ToList();

        // Орієнтовна вартість нового поліса за заявкою
        var estimate = _policyService.CalculateCost(
            request.Type, request.DesiredCoverage, today, requestedEnd);

        request.Status = RequestStatus.Processed;
        _requests.Update(request);

        return new RequestMatchResult(request, matches, estimate);
    }
}