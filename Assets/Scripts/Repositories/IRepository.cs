namespace Bootstrap
{
    public interface IRepository<T>
    {
        bool TryLoad(out T data);
        void Save(T data);
    }
}