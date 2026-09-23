using System;

namespace Dto.Auth
{
    [Serializable]
    public sealed class AuthTokensResponse
    {
        public string AccessToken;
        public string RefreshToken;
    }
}