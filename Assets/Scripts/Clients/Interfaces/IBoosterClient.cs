using System;
using System.Collections;
using Auth;

namespace Clients.Interfaces
{
    public interface IBoosterClient
    {
        public IEnumerator ActivateBooster(
            string boosterItemId,
            Action<ProfileDto> onSuccess,
            Action<string> onError);
    }
}