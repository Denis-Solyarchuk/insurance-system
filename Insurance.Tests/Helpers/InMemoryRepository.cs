using Insurance.Persistence;

namespace Insurance.Tests;

public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _items = new();
    private readonly Func<T, Guid> _idSelector;

    public InMemoryRepository(Func<T, Guid> idSelector)
    {
        _idSelector = idSelector;
    }

    public IEnumerable<T> GetAll() => _items.ToList();

    public T? GetById(Guid id) => _items.FirstOrDefault(x => _idSelector(x) == id);

    public void Add(T item) => _items.Add(item);

    public void Update(T item)
    {
        var index = _items.FindIndex(x => _idSelector(x) == _idSelector(item));
        if (index < 0) throw new KeyNotFoundException();
        _items[index] = item;
    }

    public bool Delete(Guid id) => _items.RemoveAll(x => _idSelector(x) == id) > 0;

    public void Save() { }
    public void Load() { }
}