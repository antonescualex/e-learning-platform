using System;
using System.Collections;
using Dto.Auth;

namespace Clients.Interfaces
{
    public interface IAuthClient
    {
        IEnumerator Register(
            string username,
            string password,
            string playerName,
            Action<AuthTokensResponse> onSuccess,
            Action<string> onError);

        IEnumerator Login(
            string username,
            string password,
            Action<AuthTokensResponse> onSuccess,
            Action<string> onError);

        IEnumerator RefreshSession(
            string refreshToken,
            Action<AuthTokensResponse> onSuccess,
            Action<string> onError);

        IEnumerator LogoutAll(
            Action onSuccess,
            Action<string> onError);
    }
}