using System;
using System.Collections;
using Auth;
using Dto.Profile;

namespace Clients.Interfaces
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
    }
}