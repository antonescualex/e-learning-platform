using System.Collections;
using System.Collections.Generic;
using App;
using Services;
using Services.Interfaces;
using UnityEngine;

namespace UIScripts.MainMenu.Profile
{
    public class ProfileMenuController : MonoBehaviour
    {
        [Header("Popup")]
        [SerializeField] private GameObject profilePopup;

        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject buttons;
        [SerializeField] private Canvas canvas;

        private GameObject _currentPopup;
        private IProfileService _profileService;
        private IBadgeService _badgeService;
        private IBoosterService _boosterService;

        private void Start()
        {
            ServiceContainer.TryResolve<IProfileService>(out _profileService);
            ServiceContainer.TryResolve<IBadgeService>(out _badgeService);
            ServiceContainer.TryResolve<IBoosterService>(out _boosterService);
        }

        public void OpenProfile()
        {
            if (_currentPopup != null) return;

            StartCoroutine(OpenProfileWithDelay());
        }

        public void CloseProfile()
        {
            mainMenu.SetActive(true);
            buttons.SetActive(true);

            if (_currentPopup != null)
            {
                _currentPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentPopup = null;
            }
        }

        private IEnumerator OpenProfileWithDelay()
        {
            yield return new WaitForSeconds(0.2f);

            mainMenu.SetActive(false);
            buttons.SetActive(false);

            _currentPopup = Instantiate(profilePopup, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            var popup = _currentPopup.GetComponent<ProfileView>();
            popup.Init(this, _profileService, _badgeService, _boosterService);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
        }
    }
}
