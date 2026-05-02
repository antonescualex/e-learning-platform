using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Services.OpenAI
{
    public class LessonsClient
    {
        public const string DefaultEndpoint = "https://api.openai.com/v1/responses";
        public const string DefaultPinnedModel = "gpt-5.4-mini";

        private const string DefaultInstructions =
            "Return only valid JSON. " +
            "Do not include markdown, code fences, comments, or explanations.";

        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly int _timeoutSeconds;

        public LessonsClient(
            string apiKey,
            string model = DefaultPinnedModel,
            string endpoint = DefaultEndpoint,
            int timeoutSeconds = 60)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _model = string.IsNullOrWhiteSpace(model) ? DefaultPinnedModel : model.Trim();
            _endpoint = string.IsNullOrWhiteSpace(endpoint) ? DefaultEndpoint : endpoint.Trim();
            _timeoutSeconds = Mathf.Max(1, timeoutSeconds);
        }

        public IEnumerator CreateJsonResponse(
            string prompt,
            Action<string> onSuccess,
            Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                onError?.Invoke("OpenAI prompt is empty.");
                yield break;
            }

            ResponsesRequest payload = new ResponsesRequest
            {
                model = _model,
                instructions = DefaultInstructions,
                input = prompt,
                store = false,
                reasoning = new ResponsesReasoning { effort = "low" },
                text = new ResponsesText { verbosity = "low" }
            };

            string requestJson = JsonUtility.ToJson(payload);
            byte[] requestBody = Encoding.UTF8.GetBytes(requestJson);

            using UnityWebRequest request = new UnityWebRequest(_endpoint, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(requestBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = _timeoutSeconds;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + _apiKey);

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(FormatHttpError(request, responseText));
                yield break;
            }

            ResponsesApiEnvelope envelope;
            try
            {
                envelope = JsonUtility.FromJson<ResponsesApiEnvelope>(responseText);
            }
            catch (Exception ex)
            {
                onError?.Invoke("OpenAI returned invalid Responses API JSON.\n" + ex.Message);
                yield break;
            }

            if (envelope != null && envelope.error != null && !string.IsNullOrWhiteSpace(envelope.error.message))
            {
                onError?.Invoke("OpenAI error: " + envelope.error.message);
                yield break;
            }

            string outputText = ExtractOutputText(envelope);
            if (string.IsNullOrWhiteSpace(outputText))
            {
                onError?.Invoke("OpenAI returned no output_text content.");
                yield break;
            }

            onSuccess?.Invoke(outputText);
        }

        private static string ExtractOutputText(ResponsesApiEnvelope envelope)
        {
            if (envelope == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(envelope.output_text))
            {
                return envelope.output_text;
            }

            if (envelope.output == null)
            {
                return null;
            }

            for (int i = 0; i < envelope.output.Length; i++)
            {
                ResponsesApiOutputItem item = envelope.output[i];
                if (item == null || item.content == null)
                {
                    continue;
                }

                for (int j = 0; j < item.content.Length; j++)
                {
                    ResponsesApiContentItem content = item.content[j];
                    if (content == null)
                    {
                        continue;
                    }

                    if (string.Equals(content.type, "output_text", StringComparison.Ordinal) &&
                        !string.IsNullOrWhiteSpace(content.text))
                    {
                        return content.text;
                    }
                }
            }

            return null;
        }

        private static string FormatHttpError(UnityWebRequest request, string responseText)
        {
            if (!string.IsNullOrWhiteSpace(responseText))
            {
                try
                {
                    ErrorEnvelope error = JsonUtility.FromJson<ErrorEnvelope>(responseText);
                    if (error != null && error.error != null && !string.IsNullOrWhiteSpace(error.error.message))
                    {
                        return "OpenAI request failed: " + error.error.message;
                    }
                }
                catch
                {
                }

                return "OpenAI request failed: " + request.error + "\n" + responseText;
            }

            return "OpenAI request failed: " + request.error;
        }

        [Serializable]
        private sealed class ResponsesRequest
        {
            public string model;
            public string instructions;
            public string input;
            public bool store;
            public ResponsesReasoning reasoning;
            public ResponsesText text;
        }

        [Serializable]
        private sealed class ResponsesReasoning
        {
            public string effort;
        }

        [Serializable]
        private sealed class ResponsesText
        {
            public string verbosity;
        }

        [Serializable]
        private sealed class ResponsesApiEnvelope
        {
            public string status;
            public string output_text;
            public ResponsesApiOutputItem[] output;
            public OpenAiError error;
        }

        [Serializable]
        private sealed class ResponsesApiOutputItem
        {
            public string type;
            public string role;
            public ResponsesApiContentItem[] content;
        }

        [Serializable]
        private sealed class ResponsesApiContentItem
        {
            public string type;
            public string text;
        }

        [Serializable]
        private sealed class ErrorEnvelope
        {
            public OpenAiError error;
        }

        [Serializable]
        private sealed class OpenAiError
        {
            public string message;
        }
    }
}