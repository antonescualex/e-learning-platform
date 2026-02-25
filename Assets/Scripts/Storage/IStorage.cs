namespace Bootstrap
{
    public interface IStorage
    {
        bool Exists(string relativePath);
        void Save<T>(string relativePath, T data);
        bool TryLoad<T>(string relativePath, out T data);
    }
}