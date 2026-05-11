using System.Collections;
using App;
using Auth;
using Data.StaticData;
using Data.StaticData.Shop;
using Services;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIScripts.MainMenu.Inventory
{
    public class InventoryMenuController : MonoBehaviour
    {
        [Header("Popup")]
        [SerializeField] private GameObject inventoryPopup;

        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject buttons;
        [SerializeField] private Canvas canvas;

        [FormerlySerializedAs("accessoryCatalog")]
        [Header("Accessory Catalog")]
        [SerializeField] private ShopCatalog shopCatalog;

        private GameObject _currentPopup;
        private IInventoryService _inventoryService;
        private IProfileService _profileService;
        private IProfileClient _profileClient;

        public void OpenInventory()
        {
            if (_currentPopup != null) return;

            if (_inventoryService == null)
            {
                ServiceContainer.TryResolve<IInventoryService>(out _inventoryService);
            }
            if (_profileService == null)
            {
                ServiceContainer.TryResolve<IProfileService>(out _profileService);
            }
            if (_profileClient == null)
                ServiceContainer.TryResolve<IProfileClient>(out _profileClient);

            StartCoroutine(OpenInventoryWithDelay());
        }

        public void CloseInventory()
        {
            mainMenu.SetActive(true);
            buttons.SetActive(true);

            if (_currentPopup != null)
            {
                _currentPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentPopup = null;
            }
        }

        private IEnumerator OpenInventoryWithDelay()
        {
            yield return new WaitForSeconds(0.2f);

            mainMenu.SetActive(false);
            buttons.SetActive(false);

            _currentPopup = Instantiate(inventoryPopup, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            var popup = _currentPopup.GetComponent<InventoryView>();
            popup.Init(this, _inventoryService, _profileService, _profileClient);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
        }
    }
}
