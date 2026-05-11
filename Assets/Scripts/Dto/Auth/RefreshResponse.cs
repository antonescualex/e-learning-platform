using System;

namespace Dto.Auth
{
    [Serializable]
    public sealed class RefreshResponse
    {
        public string AccessToken;
        public string RefreshToken;
    }
}