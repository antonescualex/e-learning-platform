using System;

namespace Dto.Auth
{
    [Serializable]
    public sealed class LoginRequest
    {
        public string Username;
        public string Password;
    }
}