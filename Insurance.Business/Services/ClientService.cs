using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class ClientService
{
    private readonly IRepository<Client> _clients;
    private readonly IRepository<Policy> _policies;

    public ClientService(IRepository<Client> clients, IRepository<Policy> policies)
    {
        _clients = clients;
        _policies = policies;
    }

    public Client Create(Client client)
    {
        var errors = client.Validate();
        if (errors.Any())
            throw new ValidationException(errors);

        _clients.Add(client);
        return client;
    }

    public IEnumerable<Client> GetAll() => _clients.GetAll();

    public Client? Get(Guid id) => _clients.GetById(id);

    public void Update(Client client)
    {
        var errors = client.Validate();
        if (errors.Any())
            throw new ValidationException(errors);

        _clients.Update(client);
    }

    public bool Delete(Guid id)
    {
        if (_policies.GetAll().Any(p => p.ClientId == id))
            throw new ValidationException(new List<string>
            {
                "Неможливо видалити клієнта, у якого є поліси."
            });

        return _clients.Delete(id);
    }
}