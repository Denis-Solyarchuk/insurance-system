using System.Text.Json;
using System.Text.Json.Serialization;

namespace Insurance.Persistence;

public class JsonRepository<T> : IRepository<T> where T : class
{
    private readonly string _filePath;
    private readonly Func<T, Guid> _idSelector;
    private List<T> _items = new();

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonRepository(string filePath, Func<T, Guid> idSelector)
    {
        _filePath = filePath;
        _idSelector = idSelector;
        Load();
    }

    // Репозиторій віддає копії, щоб зміни поза сервісом не потрапляли у сховище без валідації
    private static T Clone(T item) =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(item, Options), Options)!;

    public IEnumerable<T> GetAll() => _items.Select(Clone).ToList();

    public T? GetById(Guid id)
    {
        var item = _items.FirstOrDefault(x => _idSelector(x) == id);
        return item == null ? null : Clone(item);
    }

    public void Add(T item)
    {
        _items.Add(Clone(item));
        Save();
    }

    public void Update(T item)
    {
        var index = _items.FindIndex(x => _idSelector(x) == _idSelector(item));
        if (index < 0)
            throw new KeyNotFoundException("Об'єкт не знайдено.");

        _items[index] = Clone(item);
        Save();
    }

    public bool Delete(Guid id)
    {
        var removed = _items.RemoveAll(x => _idSelector(x) == id) > 0;
        if (removed) Save();
        return removed;
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(_items, Options);
        File.WriteAllText(_filePath, json);
    }

    public void Load()
    {
        if (!File.Exists(_filePath))
        {
            _items = new List<T>();
            return;
        }

        var json = File.ReadAllText(_filePath);
        _items = JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
    }
}