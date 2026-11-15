using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class ClientService
{
    private readonly IRepository<Client> _clients;

    public ClientService(IRepository<Client> clients)
    {
        _clients = clients;
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

    public bool Delete(Guid id) => _clients.Delete(id);
}