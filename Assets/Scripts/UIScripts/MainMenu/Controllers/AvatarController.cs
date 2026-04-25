using App;
using Data;
using Data.StaticData.Avatar;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Controllers
{
    public class AvatarController : MonoBehaviour
    {
        [SerializeField] private AvatarCatalog avatarCatalog;
        [SerializeField] private Image avatarPreviewImage;

        private Toggle[] _toggles;
        private IProfileService _profileService;
        private bool _ignore;

        private void Awake()
        {
            ServiceContainer.TryResolve<IProfileService>(out _profileService);
            _toggles = GetComponentsInChildren<Toggle>(true);
        }

        private void OnEnable()
        {
            if (_profileService != null)
            {
                _profileService.ProfileChanged += OnProfileChanged;
            }
            
            foreach (var toggle in _toggles)
            {
                toggle.onValueChanged.AddListener(isOn => OnToggleChanged(toggle, isOn));
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (_profileService != null)
            {
                _profileService.ProfileChanged -= OnProfileChanged;
            }
            foreach (var toggle in _toggles)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }
        }

        private void Refresh()
        {
            if (_profileService == null || !_profileService.HasProfile) return;

            ProfileData profileData = _profileService.ProfileData;
            string selectedAvatarId = profileData.AvatarId;

            _ignore = true;

            foreach (var toggle in _toggles)
            {
                string avatarId = toggle.gameObject.name;
                bool isOwned = _profileService.HasAvatar(avatarId);

                toggle.gameObject.SetActive(isOwned);
                toggle.SetIsOnWithoutNotify(isOwned && avatarId == selectedAvatarId);
            }

            _ignore = false;

            RefreshPreview(selectedAvatarId);
        }
        
        private void RefreshPreview(string avatarId)
        {
            if (avatarPreviewImage == null || avatarCatalog == null) return;

            Sprite sprite = avatarCatalog.GetSprite(avatarId);
            if (sprite != null)
            {
                avatarPreviewImage.sprite = sprite;
            }
        }

        private void OnProfileChanged(ProfileData profileData)
        {
            Refresh();
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
