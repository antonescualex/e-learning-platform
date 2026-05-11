namespace Auth
{
    public interface ISessionStorage
    {
        bool TryLoadRefreshToken(out string refreshToken);
        void SaveRefreshToken(string refreshToken);
        void ClearRefreshToken();
    }
}