using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class SavedQueryService
{
    private readonly IRepository<SavedQuery> _queries;
    private readonly IRepository<Client> _clients;

    public SavedQueryService(IRepository<SavedQuery> queries, IRepository<Client> clients)
    {
        _queries = queries;
        _clients = clients;
    }

    public SavedQuery Save(Guid clientId, string text)
    {
        var query = new SavedQuery { ClientId = clientId, Text = text.Trim() };

        var errors = query.Validate();
        if (_clients.GetById(clientId) == null)
            errors.Add("Клієнта не знайдено.");

        if (errors.Any())
            throw new ValidationException(errors);

        _queries.Add(query);
        return query;
    }

    public IEnumerable<SavedQuery> GetByClient(Guid clientId) =>
        _queries.GetAll()
            .Where(q => q.ClientId == clientId)
            .OrderByDescending(q => q.CreatedAt)
            .ToList();
}