using System.Collections;
using App;
using Auth;
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
        private IProfileClient _profileClient;
        private bool _ignore;
        private bool _isSelectingAvatar;

        private void Awake()
        {
            ServiceContainer.TryResolve<IProfileClient>(out _profileClient);
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

            StartCoroutine(SelectAvatar(avatarId));
        }
        
        private IEnumerator SelectAvatar(string avatarId)
        {
            if (_isSelectingAvatar) yield break;
            if (_profileClient == null || _profileService == null) yield break;

            _isSelectingAvatar = true;

            ProfileDto profileDto = null;
            string error = null;

            yield return _profileClient.SelectAvatar(
                avatarId,
                dto => profileDto = dto,
                message => error = message);

            _isSelectingAvatar = false;

            if (!string.IsNullOrWhiteSpace(error) || profileDto == null)
            {
                Debug.LogWarning("Select avatar failed:\n" + error);
                Refresh();
                yield break;
            }

            _profileService.SetLoadedProfile(ProfileMapper.ToProfileData(profileDto));
        }
    }
}
