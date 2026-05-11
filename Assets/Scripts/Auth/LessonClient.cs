using System;
using System.Collections;
using Auth.Interfaces;
using Dto.Lesson;
using Dto.Profile;
using UnityEngine.Networking;

namespace Auth
{
    public class LessonClient : AbstractClient, ILessonClient
    {
        protected LessonClient(string baseUrl, AuthSession authSession, int timeoutSeconds = 20) : base(baseUrl, authSession, timeoutSeconds)
        {
        }

        public IEnumerator CompleteLesson(
            CompleteLessonRequest requestModel,
            Action<ProfileAwardResponse> onSuccess,
            Action<string> onError)
        {
            if (requestModel == null)
            {
                onError?.Invoke("CompleteLesson request is missing.");
                yield break;
            }

            yield return SendAuthorizedJsonWithAwardResponse(
                "/api/lessons/complete",
                UnityWebRequest.kHttpVerbPOST,
                requestModel,
                onSuccess,
                onError);
        }
    }
}