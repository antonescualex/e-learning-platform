using System;
using System.Collections;
using System.Text;
using Clients;
using Clients.Interfaces;
using Dto;
using Dto.Auth;
using Dto.Error;
using UnityEngine;
using UnityEngine.Networking;

namespace Auth
{
    public sealed class AuthClient : IAuthClient
    {
        private const int DefaultTimeoutSeconds = 20;

        private readonly string _baseUrl;
        private readonly AuthSession _authSession;
        private readonly int _timeoutSeconds;

        public AuthClient(
            string baseUrl,
            AuthSession authSession,
            int timeoutSeconds = DefaultTimeoutSeconds)
        {
            _baseUrl = string.IsNullOrWhiteSpace(baseUrl)
                ? Services.LessonContentService.DefaultBaseUrl
                : baseUrl.Trim().TrimEnd('/');

            _authSession = authSession ?? throw new ArgumentNullException(nameof(authSession));
            _timeoutSeconds = Mathf.Max(1, timeoutSeconds);
        }

        public IEnumerator Register(
            string username,
            string password,
            string playerName,
            Action<AuthTokensResponse> onSuccess,
            Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                onError?.Invoke("Username is required.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                onError?.Invoke("Password is required.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(playerName))
            {
                onError?.Invoke("Player name is required.");
                yield break;
            }

            RegisterRequest payload = new RegisterRequest
            {
                Username = username.Trim(),
                Password = password,
                PlayerName = playerName.Trim()
            };

            yield return SendJsonForTokens(
                "/api/auth/register",
                UnityWebRequest.kHttpVerbPOST,
                payload,
                onSuccess,
                onError);
        }

        public IEnumerator Login(
            string username,
            string password,
            Action<AuthTokensResponse> onSuccess,
            Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                onError?.Invoke("Username is required.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                onError?.Invoke("Password is required.");
                yield break;
            }

            LoginRequest payload = new LoginRequest
            {
                Username = username.Trim(),
                Password = password
            };

            yield return SendJsonForTokens(
                "/api/auth/login",
                UnityWebRequest.kHttpVerbPOST,
                payload,
                onSuccess,
                onError);
        }

        public IEnumerator RefreshSession(
            string refreshToken,
            Action<AuthTokensResponse> onSuccess,
            Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                onError?.Invoke("Refresh token is missing.");
                yield break;
            }

            RefreshRequest payload = new RefreshRequest
            {
                RefreshToken = refreshToken.Trim()
            };

            yield return SendJsonForTokens(
                "/api/auth/refresh",
                UnityWebRequest.kHttpVerbPOST,
                payload,
                onSuccess,
                onError);
        }

        public IEnumerator LogoutAll(
            Action onSuccess,
            Action<string> onError)
        {
            using UnityWebRequest request =
                new UnityWebRequest(BuildUrl("/api/auth/logout-all"), UnityWebRequest.kHttpVerbPOST);

            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = _timeoutSeconds;

            if (!TryApplyAuthorizationHeader(request, out string authError))
            {
                onError?.Invoke(authError);
                yield break;
            }

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null
                ? request.downloadHandler.text
                : string.Empty;

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(FormatRequestError(request, responseText));
                yield break;
            }

            onSuccess?.Invoke();
        }

        private IEnumerator SendJsonForTokens(
            string relativePath,
            string method,
            object payload,
            Action<AuthTokensResponse> onSuccess,
            Action<string> onError)
        {
            string requestJson = JsonUtility.ToJson(payload);
            byte[] requestBody = Encoding.UTF8.GetBytes(requestJson);

            using UnityWebRequest request =
                new UnityWebRequest(BuildUrl(relativePath), method);

            request.uploadHandler = new UploadHandlerRaw(requestBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = _timeoutSeconds;
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null
                ? request.downloadHandler.text
                : string.Empty;

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(FormatRequestError(request, responseText));
                yield break;
            }

            AuthTokensResponse tokens;
            try
            {
                tokens = JsonUtility.FromJson<AuthTokensResponse>(responseText);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Invalid JSON response from auth backend.\n" + ex.Message);
                yield break;
            }

            if (tokens == null
                || string.IsNullOrWhiteSpace(tokens.AccessToken)
                || string.IsNullOrWhiteSpace(tokens.RefreshToken))
            {
                onError?.Invoke("Auth response does not contain both tokens.");
                yield break;
            }

            onSuccess?.Invoke(tokens);
        }

        private bool TryApplyAuthorizationHeader(
            UnityWebRequest request,
            out string error)
        {
            if (!_authSession.HasAccessToken)
            {
                error = "Access token is missing from the current auth session.";
                return false;
            }

            request.SetRequestHeader("Authorization", "Bearer " + _authSession.AccessToken);
            error = null;
            return true;
        }

        private string BuildUrl(string relativePath)
        {
            return _baseUrl + relativePath;
        }

        private static string FormatRequestError(
            UnityWebRequest request,
            string responseText)
        {
            ApiErrorResponse error = null;

            if (!string.IsNullOrWhiteSpace(responseText))
            {
                try
                {
                    error = JsonUtility.FromJson<ApiErrorResponse>(responseText);
                }
                catch
                {
                    error = null;
                }
            }

            if (error != null && !string.IsNullOrWhiteSpace(error.Message))
            {
                StringBuilder builder = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(error.Code))
                {
                    builder.Append(error.Code).Append(": ");
                }

                builder.Append(error.Message);

                if (error.Details != null && error.Details.Length > 0)
                {
                    builder.Append("\n").Append(string.Join("\n", error.Details));
                }

                if (!string.IsNullOrWhiteSpace(error.CorrelationId))
                {
                    builder.Append("\nCorrelationId: ").Append(error.CorrelationId);
                }

                return builder.ToString();
            }

            if (!string.IsNullOrWhiteSpace(responseText))
            {
                return "Auth backend request failed: " + request.error + "\n" + responseText;
            }

            return "Auth backend request failed: " + request.error;
        }
    }
}