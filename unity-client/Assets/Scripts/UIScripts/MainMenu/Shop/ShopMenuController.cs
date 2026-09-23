using System;
using System.Collections;
using App;
using Data.StaticData;
using Data.StaticData.Shop;
using Services;
using Services.Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIScripts.MainMenu.Shop
{
    public class ShopMenuController : MonoBehaviour
    {
        [Header("Popup")]
        [SerializeField] private GameObject shopPopupPrefab;

        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject buttons;
        [SerializeField] private Canvas canvas;

        [Header("Accessory Catalog")]
        [SerializeField] private ShopCatalog catalog;

        private GameObject _currentPopup;
        private IShopService _shopService;

        public void OpenShop()
        {
            if (_currentPopup != null) return;
            if (_shopService == null)
            {
                ServiceContainer.TryResolve<IShopService>(out _shopService);
            }
            StartCoroutine(OpenShopWithDelay());
        }

        public void CloseShop()
        {
            mainMenu.SetActive(true);
            buttons.SetActive(true);

            if (_currentPopup != null)
            {
                _currentPopup.GetComponent<Ricimi.Popup>()?.Close();
                _currentPopup = null;
            }
        }

        private IEnumerator OpenShopWithDelay()
        {
            yield return new WaitForSeconds(0.2f);
            mainMenu.SetActive(false);
            buttons.SetActive(false);

            _currentPopup = Instantiate(shopPopupPrefab, canvas.transform);
            _currentPopup.transform.SetAsLastSibling();

            var popup = _currentPopup.GetComponent<ShopView>();
            popup.Init(_shopService, this);

            _currentPopup.GetComponent<Ricimi.Popup>()?.Open();
        }
    }
}
