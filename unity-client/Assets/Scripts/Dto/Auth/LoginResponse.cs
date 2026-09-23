using System;

namespace Dto.Auth
{
    [Serializable]
    public sealed class LoginResponse
    {
        public string AccessToken;
        public string RefreshToken;
    }
}