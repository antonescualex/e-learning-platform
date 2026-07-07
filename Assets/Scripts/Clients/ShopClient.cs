using System;
using System.Collections;
using Clients.Interfaces;
using Dto.Profile;
using UnityEngine.Networking;

namespace Clients
{
    public class ShopClient : AbstractClient, IShopClient
    {
        public ShopClient(string baseUrl, AuthSession authSession, int timeoutSeconds = 20) : base(baseUrl, authSession, timeoutSeconds)
        {
        }

        public IEnumerator PurchaseBooster(string boosterId, Action<ProfileAwardResponse> onSuccess, Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(boosterId))
            {
                onError?.Invoke("BoosterId is required.");
                yield break;
            }

            var payload = new PurchaseBoosterRequest
            {
                BoosterId = boosterId.Trim()
            };

            yield return SendAuthorizedJsonWithAwardResponse("/api/shop/purchase-booster", UnityWebRequest.kHttpVerbPOST, payload, onSuccess, onError);
        }

        public IEnumerator PurchaseBackground(string backgroundId, Action<ProfileAwardResponse> onSuccess, Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(backgroundId))
            {
                onError?.Invoke("BackgroundId is required.");
                yield break;
            }

            var payload = new PurchaseBackgroundRequest
            {
                BackgroundId = backgroundId.Trim()
            };

            yield return SendAuthorizedJsonWithAwardResponse(
                "/api/shop/purchase-background",
                UnityWebRequest.kHttpVerbPOST,
                payload,
                onSuccess,
                onError);
        }

        public IEnumerator PurchaseAvatar(string avatarId, Action<ProfileAwardResponse> onSuccess, Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(avatarId))
            {
                onError?.Invoke("AvatarId is required.");
                yield break;
            }

            var payload = new PurchaseAvatarRequest
            {
                AvatarId = avatarId.Trim()
            };

            yield return SendAuthorizedJsonWithAwardResponse(
                "/api/shop/purchase-avatar",
                UnityWebRequest.kHttpVerbPOST,
                payload,
                onSuccess,
                onError);
        }
    }
}