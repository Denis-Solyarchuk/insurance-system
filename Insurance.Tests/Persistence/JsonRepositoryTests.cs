using Insurance.Domain;
using Insurance.Persistence;
using Xunit;

namespace Insurance.Tests;

public class JsonRepositoryTests : IDisposable
{
    private readonly string _path =
        Path.Combine(Path.GetTempPath(), $"clients-{Guid.NewGuid()}.json");

    [Fact]
    public void Add_ThenReloadFromFile_ReturnsSavedItem()
    {
        var client = new Client { FullName = "Іваненко Іван", Type = ClientType.LegalEntity };

        var first = new JsonRepository<Client>(_path, c => c.Id);
        first.Add(client);

        // Новий екземпляр імітує перезапуск програми
        var second = new JsonRepository<Client>(_path, c => c.Id);
        var loaded = second.GetById(client.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Іваненко Іван", loaded!.FullName);
        Assert.Equal(ClientType.LegalEntity, loaded.Type);
    }

    public void Dispose()
    {
        if (File.Exists(_path)) File.Delete(_path);
    }
}