using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Services
{
    public sealed class LessonContentGenerator
    {
        private const string LessonContentPath = "/api/lesson-content";

        private readonly string _baseUrl;
        private readonly int _timeoutSeconds;

        public LessonContentGenerator(string baseUrl, int timeoutSeconds)
        {
            _baseUrl = string.IsNullOrWhiteSpace(baseUrl)
                ? LessonContentService.DefaultBaseUrl
                : baseUrl.Trim().TrimEnd('/');
            _timeoutSeconds = Mathf.Max(1, timeoutSeconds);
        }

        public IEnumerator Generate<TRequest, TResponse, TQuestion>(
            string endpointPath,
            TRequest payload,
            Func<string, TResponse> deserialize,
            Func<TResponse, List<TQuestion>> validate,
            Action<List<TQuestion>> onSuccess,
            Action<string> onError)
        {
            string requestJson = JsonUtility.ToJson(payload);
            byte[] requestBody = Encoding.UTF8.GetBytes(requestJson);

            using UnityWebRequest request = new UnityWebRequest(BuildUrl(endpointPath), UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(requestBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = _timeoutSeconds;
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(FormatRequestError(request, responseText));
                yield break;
            }

            TResponse response;
            try
            {
                response = deserialize(responseText);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Invalid JSON response from lesson backend.\n" + ex.Message);
                yield break;
            }

            List<TQuestion> questions;
            try
            {
                questions = validate(response);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Lesson backend returned invalid content.\n" + ex.Message);
                yield break;
            }

            onSuccess?.Invoke(questions);
        }

        private string BuildUrl(string endpointPath)
        {
            return _baseUrl + LessonContentPath + endpointPath;
        }

        private static string FormatRequestError(UnityWebRequest request, string responseText)
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
                builder.Append(error.Code).Append(": ").Append(error.Message);

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
                return "Lesson backend request failed: " + request.error + "\n" + responseText;
            }

            return "Lesson backend request failed: " + request.error;
        }

        [Serializable]
        private sealed class ApiErrorResponse
        {
            public string Code;
            public string Message;
            public string[] Details;
            public string CorrelationId;
        }
    }
}
