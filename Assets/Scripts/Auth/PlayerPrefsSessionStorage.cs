using UnityEngine;

namespace Auth
{
    public sealed class PlayerPrefsSessionStorage : ISessionStorage
    {
        private readonly string _refreshTokenKey;
        
        public PlayerPrefsSessionStorage(string refreshTokenKey = "auth.refresh_token")
        {
            _refreshTokenKey = string.IsNullOrWhiteSpace(refreshTokenKey)
                ? "auth.refresh_token"
                : refreshTokenKey.Trim();
        }
        
        public bool TryLoadRefreshToken(out string refreshToken)
        {
            refreshToken = PlayerPrefs.GetString(_refreshTokenKey, string.Empty);
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                refreshToken = null;
                return false;
            }

            refreshToken = refreshToken.Trim();
            return true;
        }

        public void SaveRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                ClearRefreshToken();
                return;
            }

            PlayerPrefs.SetString(_refreshTokenKey, refreshToken.Trim());
            PlayerPrefs.Save();
        }

        public void ClearRefreshToken()
        {
            PlayerPrefs.DeleteKey(_refreshTokenKey);
            PlayerPrefs.Save();
        }
    }
}