using System;
using System.Collections;
using Auth;
using Clients.Interfaces;
using Dto.Profile;
using UnityEngine.Networking;

namespace Clients
{
    public class BoosterClient : AbstractClient, IBoosterClient
    {
        public BoosterClient(string baseUrl, AuthSession authSession, int timeoutSeconds = 20) : base(baseUrl, authSession, timeoutSeconds)
        {
        }

        public IEnumerator ActivateBooster(
            string boosterItemId,
            Action<ProfileDto> onSuccess,
            Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(boosterItemId))
            {
                onError?.Invoke("BoosterItemId is required.");
                yield break;
            }

            var payload = new ActivateBoosterRequest
            {
                BoosterItemId = boosterItemId.Trim()
            };

            yield return SendAuthorizedJsonWithProfileResponse(
                "/api/me/activate-booster",
                UnityWebRequest.kHttpVerbPOST,
                payload,
                onSuccess,
                onError);
        }
    }
}