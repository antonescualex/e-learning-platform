using System;
using System.Collections;
using Dto.Profile;

namespace Clients.Interfaces
{
    public interface IShopClient
    {
        public IEnumerator PurchaseBooster(string boosterId, Action<ProfileAwardResponse> onSuccess,
            Action<string> onError);

        public IEnumerator PurchaseBackground(string backgroundId, Action<ProfileAwardResponse> onSuccess,
            Action<string> onError);

        public IEnumerator PurchaseAvatar(string avatarId, Action<ProfileAwardResponse> onSuccess,
            Action<string> onError);
    }
}