using Dto;
using Dto.Auth;

namespace Auth
{
    public sealed class AuthSession
    {
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }

        public bool HasAccessToken => !string.IsNullOrWhiteSpace(AccessToken);
        public bool HasRefreshToken => !string.IsNullOrWhiteSpace(RefreshToken);

        public void SetAccessToken(string accessToken)
        {
            AccessToken = string.IsNullOrWhiteSpace(accessToken) ? null : accessToken.Trim();
        }

        public void SetRefreshToken(string refreshToken)
        {
            RefreshToken = string.IsNullOrWhiteSpace(refreshToken) ? null : refreshToken.Trim();
        }

        public void ApplyTokens(AuthTokensResponse tokensResponse)
        {
            if (tokensResponse == null) return;

            SetAccessToken(tokensResponse.AccessToken);
            SetRefreshToken(tokensResponse.RefreshToken);
        }

        public void Clear()
        {
            AccessToken = null;
            RefreshToken = null;
        }
    }
}