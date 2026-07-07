using System;
using System.Collections;
using Dto.Lesson;
using Dto.Profile;

namespace Clients.Interfaces
{
    public interface ILessonClient
    {
        public IEnumerator CompleteLesson(
            CompleteLessonRequest requestModel,
            Action<ProfileAwardResponse> onSuccess,
            Action<string> onError);
    }
}