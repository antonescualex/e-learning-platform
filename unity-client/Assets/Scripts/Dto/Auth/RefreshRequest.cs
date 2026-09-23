using System;

namespace Dto.Auth
{
    [Serializable]
    public sealed class RefreshRequest
    {
        public string RefreshToken;
    }
}