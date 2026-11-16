namespace Insurance.Persistence;

public interface IRepository<T>
{
    IEnumerable<T> GetAll();
    T? GetById(Guid id);
    void Add(T item);
    void Update(T item);
    bool Delete(Guid id);
    void Save();
    void Load();
}