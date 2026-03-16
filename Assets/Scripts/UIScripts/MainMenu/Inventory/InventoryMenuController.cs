using System.Collections;
using Data.StaticData;
using Data.StaticData.Accessory;
using Services;
using UnityEngine;

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

        [Header("Accessory Catalog")]
        [SerializeField] private AccessoryCatalog accessoryCatalog;

        private GameObject _currentPopup;
        private IInventoryService _inventoryService;

        public void OpenInventory()
        {
            if (_currentPopup != null) return;

            if (_inventoryService == null)
            {
                _inventoryService = new InventoryService(accessoryCatalog, App.Instance.ProfileService);
            }

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

            var popup = _currentPopup.GetComponent<InventoryPopup>();
            popup.Init(this, _inventoryService);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
        }
    }
}