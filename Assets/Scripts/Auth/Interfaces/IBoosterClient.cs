using System;
using System.Collections;
using Dto.Profile;
using UnityEngine.Networking;

namespace Auth.Interfaces
{
    public interface IBoosterClient
    {
        public IEnumerator ActivateBooster(
            string boosterItemId,
            Action<ProfileDto> onSuccess,
            Action<string> onError);
    }
}