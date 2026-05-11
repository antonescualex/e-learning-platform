using System;

namespace Dto.Auth
{
    [Serializable]
    public sealed class RegisterRequest
    {
        public string Username;
        public string Password;
        public string PlayerName;
    }
}