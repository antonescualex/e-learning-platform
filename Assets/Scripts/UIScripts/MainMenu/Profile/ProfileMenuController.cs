using Services;
using UnityEngine;

namespace UIScripts.MainMenu.Profile
{
    public class ProfileMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject profilePopup;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private Canvas canvas;

        private GameObject _currentPopup;
        private IProfileService _profileService;

        private void Start()
        {
            _profileService = App.Instance.ProfileService;
        }

        public void OpenProfile()
        {
            if (_currentPopup != null) return;
            
            mainMenu.SetActive(false);

            _currentPopup = Instantiate(profilePopup, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            var popup = _currentPopup.GetComponent<ProfilePopup>();
            popup.Init(this, _profileService);
            
            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
        }

        public void CloseProfile()
        {
            mainMenu.SetActive(true);

            if (_currentPopup != null)
            {
                _currentPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentPopup = null;
            }
        }
    }
}