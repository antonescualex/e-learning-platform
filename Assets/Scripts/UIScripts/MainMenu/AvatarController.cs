using System;
using Data.StaticData;
using Data.StaticData.Avatar;
using Services;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu
{
    public class AvatarController : MonoBehaviour
    {
        [SerializeField] private AvatarCatalog avatarCatalog;

        private Toggle[] _toggles;
        private IProfileService _profileService;
        private bool _ignore;

        private void Awake()
        {
            _profileService = App.Instance.ProfileService;
            _toggles = GetComponentsInChildren<Toggle>();
        }

        private void OnEnable()
        {
            foreach (var toggle in _toggles)
            {
                toggle.onValueChanged.AddListener(isOn => OnToggleChanged(toggle, isOn));
            }

            ApplySelectionFromProfile();
        }

        private void OnDisable()
        {
            foreach (var toggle in _toggles)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }
        }

        private void ApplySelectionFromProfile()
        {
            string savedId = _profileService.ProfileData.AvatarId;
            _ignore = true;
            foreach (var toggle in _toggles)
            {
                bool shouldBeOn = toggle.gameObject.name == savedId;
                toggle.SetIsOnWithoutNotify(shouldBeOn);
            }
            _ignore = false;
        }

        private void OnToggleChanged(Toggle toggle, bool isOn)
        {
            if (_ignore) return;
            if (!isOn) return;

            string avatarId = toggle.gameObject.name;
            if (avatarCatalog != null && !avatarCatalog.ContainsId(avatarId)) return;

            _profileService.SetAvatar(avatarId);
        }
    }
}