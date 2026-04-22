using System;
using App;
using Data;
using Data.StaticData.Avatar;
using Data.StaticData.Background;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Controllers
{
    public class BackgroundController : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private BackgroundCatalog backgroundCatalog;
        
        private IProfileService _profileService;

        private void OnEnable()
        {
            ServiceContainer.TryResolve<IProfileService>(out _profileService);
            if (_profileService == null) return;

            _profileService.ProfileChanged += OnProfileChanged;

            if (_profileService.ProfileData != null)
            {
                OnProfileChanged(_profileService.ProfileData);
            }
        }

        private void OnDisable()
        {
            if (_profileService != null)
            {
                _profileService.ProfileChanged -= OnProfileChanged;
            }
        }
        
        private void OnProfileChanged(ProfileData profileData)
        {
            if (backgroundImage == null) return;
            if (backgroundCatalog == null) return;
            if (profileData == null) return;
            if (string.IsNullOrEmpty(profileData.BackgroundId)) return;

            Sprite sprite = backgroundCatalog.GetBackgroundDefinition(profileData.BackgroundId).sprite;
            if (sprite == null) return;

            backgroundImage.sprite = sprite;
        }
    }
}