using System;
using System.Collections;
using Dto;
using Dto.Lesson;
using Dto.Profile;

namespace Auth
{
    public interface IProfileClient
    {
        IEnumerator DailyLogin(
            Action<ProfileAwardResponse> onSuccess,
            Action<string> onError);
        
        IEnumerator GetProfile(
            Action<ProfileDto> onSuccess,
            Action<string> onError);

        IEnumerator UpdatePlayerName(
            string playerName,
            Action<ProfileDto> onSuccess,
            Action<string> onError);

        IEnumerator SelectAvatar(
            string avatarId,
            Action<ProfileDto> onSuccess,
            Action<string> onError);

        IEnumerator SelectBackground(
            string backgroundId,
            Action<ProfileDto> onSuccess,
            Action<string> onError);
        
        IEnumerator PurchaseBooster(
            string boosterId,
            Action<ProfileAwardResponse> onSuccess,
            Action<string> onError);

        IEnumerator PurchaseBackground(
            string backgroundId,
            Action<ProfileAwardResponse> onSuccess,
            Action<string> onError);

        IEnumerator PurchaseAvatar(
            string avatarId,
            Action<ProfileAwardResponse> onSuccess,
            Action<string> onError);
        
        IEnumerator ActivateBooster(string boosterItemId,
            Action<ProfileDto> onSuccess,
            Action<string> onError);

        IEnumerator CompleteLesson(
            CompleteLessonRequest request,
            Action<ProfileAwardResponse> onSuccess,
            Action<string> onError);
    }
}