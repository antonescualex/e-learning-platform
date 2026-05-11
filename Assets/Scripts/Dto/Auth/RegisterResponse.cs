using System;

namespace Dto.Auth
{
    [Serializable]
    public sealed class RegisterResponse
    {
        public string AccessToken;
        public string RefreshToken;
    }
}