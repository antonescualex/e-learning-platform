using System;
using System.Collections;
using System.Text;
using Auth;
using Clients.Interfaces;
using Dto.Error;
using Dto.Profile;
using UnityEngine;
using UnityEngine.Networking;

namespace Clients
{
    public sealed class ProfileClient : IProfileClient
    {
        private const int DefaultTimeoutSeconds = 20;

        private readonly string _baseUrl;
        private readonly AuthSession _authSession;
        private readonly int _timeoutSeconds;

        public ProfileClient(
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

        public IEnumerator DailyLogin(
            Action<ProfileAwardResponse> onSuccess,
            Action<string> onError)
        {
            yield return SendAuthorizedJsonWithAwardResponse(
                "/api/me/daily-login",
                UnityWebRequest.kHttpVerbPOST,
                new EmptyRequest(),
                onSuccess,
                onError);
        }
        
        public IEnumerator GetProfile(
            Action<ProfileDto> onSuccess,
            Action<string> onError)
        {
            yield return SendAuthorizedGet(
                "/api/me/profile",
                onSuccess,
                onError);
        }

        public IEnumerator UpdatePlayerName(
            string playerName,
            Action<ProfileDto> onSuccess,
            Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                onError?.Invoke("Player name is required.");
                yield break;
            }

            UpdateProfileRequest payload = new UpdateProfileRequest
            {
                PlayerName = playerName.Trim()
            };

            yield return SendAuthorizedJsonWithProfileResponse(
                "/api/me/profile",
                "PATCH",
                payload,
                onSuccess,
                onError);
        }

        public IEnumerator SelectAvatar(
            string avatarId,
            Action<ProfileDto> onSuccess,
            Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(avatarId))
            {
                onError?.Invoke("AvatarId is required.");
                yield break;
            }

            SelectAvatarRequest payload = new SelectAvatarRequest
            {
                AvatarId = avatarId.Trim()
            };

            yield return SendAuthorizedJsonWithProfileResponse(
                "/api/me/select-avatar",
                UnityWebRequest.kHttpVerbPOST,
                payload,
                onSuccess,
                onError);
        }

        public IEnumerator SelectBackground(
            string backgroundId,
            Action<ProfileDto> onSuccess,
            Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(backgroundId))
            {
                onError?.Invoke("BackgroundId is required.");
                yield break;
            }

            SelectBackgroundRequest payload = new SelectBackgroundRequest
            {
                BackgroundId = backgroundId.Trim()
            };

            yield return SendAuthorizedJsonWithProfileResponse(
                "/api/me/select-background",
                UnityWebRequest.kHttpVerbPOST,
                payload,
                onSuccess,
                onError);
        }

        private IEnumerator SendAuthorizedGet(
            string relativePath,
            Action<ProfileDto> onSuccess,
            Action<string> onError)
        {
            using UnityWebRequest request =
                new UnityWebRequest(BuildUrl(relativePath), UnityWebRequest.kHttpVerbGET);

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

            ProfileDto dto;
            try
            {
                dto = JsonUtility.FromJson<ProfileDto>(responseText);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Invalid JSON response from profile backend.\n" + ex.Message);
                yield break;
            }

            if (dto == null)
            {
                onError?.Invoke("Profile backend returned an empty response.");
                yield break;
            }

            onSuccess?.Invoke(dto);
        }

        private IEnumerator SendAuthorizedJsonWithProfileResponse(
            string relativePath,
            string method,
            object payload,
            Action<ProfileDto> onSuccess,
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

            ProfileDto dto;
            try
            {
                dto = JsonUtility.FromJson<ProfileDto>(responseText);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Invalid JSON response from profile backend.\n" + ex.Message);
                yield break;
            }

            if (dto == null)
            {
                onError?.Invoke("Profile backend returned an empty response.");
                yield break;
            }

            onSuccess?.Invoke(dto);
        }
        
        private IEnumerator SendAuthorizedJsonWithAwardResponse(
            string relativePath,
            string method,
            object payload,
            Action<ProfileAwardResponse> onSuccess,
            Action<string> onError)
        {
            string requestJson = JsonUtility.ToJson(payload);
            byte[] requestBody = Encoding.UTF8.GetBytes(requestJson);

            using UnityWebRequest request = new UnityWebRequest(BuildUrl(relativePath), method);

            request.uploadHandler = new UploadHandlerRaw(requestBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = _timeoutSeconds;
            request.SetRequestHeader("Content-Type", "application/json");

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

            ProfileAwardResponse response = JsonUtility.FromJson<ProfileAwardResponse>(responseText);

            if (response == null || response.Profile == null)
            {
                onError?.Invoke("Backend returned an invalid profile award response.");
                yield break;
            }

            onSuccess?.Invoke(response);
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
                return "Profile backend request failed: " + request.error + "\n" + responseText;
            }

            return "Profile backend request failed: " + request.error;
        }
    }
}