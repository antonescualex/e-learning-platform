using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Profile
{
    public class ProfileItemPopup : MonoBehaviour
    {
        [SerializeField] private Transform popupCanvas;
        [SerializeField] private Button closeButton;
        [SerializeField] private float closeDelay = 0.5f;

        private readonly List<CanvasGroupState> _blockedCanvasGroups = new List<CanvasGroupState>();
        private bool _interactionsBlocked;
        private bool _isClosing;

        public void SetPopupCanvas(Transform popupCanvasTransform)
        {
            popupCanvas = popupCanvasTransform;
        }

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
            }

            RestoreBlockedInteractions();
        }

        public void Open()
        {
            RectTransform popupCanvasRect = ResolvePopupCanvas();
            if (popupCanvasRect == null)
            {
                Debug.LogWarning("ProfileItemPopup requires a popup canvas reference.", this);
                return;
            }

            transform.SetParent(popupCanvasRect, false);
            transform.SetAsLastSibling();
            BlockBehindInteractions();
        }

        public void Close()
        {
            if (_isClosing) return;
            _isClosing = true;

            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Close");
            }

            StartCoroutine(RunPopupDestroy());
        }

        private IEnumerator RunPopupDestroy()
        {
            yield return new WaitForSeconds(closeDelay);
            RestoreBlockedInteractions();
            Destroy(gameObject);
        }

        private void BlockBehindInteractions()
        {
            if (_interactionsBlocked) return;

            Transform interactionRoot = popupCanvas != null ? popupCanvas.parent : null;
            if (interactionRoot == null) return;

            _blockedCanvasGroups.Clear();

            for (int i = 0; i < interactionRoot.childCount; i++)
            {
                Transform sibling = interactionRoot.GetChild(i);
                if (sibling == popupCanvas) continue;

                CanvasGroup canvasGroup = sibling.GetComponent<CanvasGroup>();
                bool addedCanvasGroup = false;

                if (canvasGroup == null)
                {
                    canvasGroup = sibling.gameObject.AddComponent<CanvasGroup>();
                    addedCanvasGroup = true;
                }

                _blockedCanvasGroups.Add(new CanvasGroupState(
                    canvasGroup,
                    addedCanvasGroup,
                    canvasGroup.interactable,
                    canvasGroup.blocksRaycasts));

                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            _interactionsBlocked = true;
        }

        private void RestoreBlockedInteractions()
        {
            if (!_interactionsBlocked) return;

            for (int i = 0; i < _blockedCanvasGroups.Count; i++)
            {
                CanvasGroupState state = _blockedCanvasGroups[i];
                if (state.CanvasGroup == null) continue;

                state.CanvasGroup.interactable = state.Interactable;
                state.CanvasGroup.blocksRaycasts = state.BlocksRaycasts;

                if (state.AddedAtRuntime)
                {
                    Destroy(state.CanvasGroup);
                }
            }

            _blockedCanvasGroups.Clear();
            _interactionsBlocked = false;
        }

        private RectTransform ResolvePopupCanvas()
        {
            if (popupCanvas is RectTransform popupCanvasRect)
            {
                return popupCanvasRect;
            }

            return transform.parent as RectTransform;
        }

        private readonly struct CanvasGroupState
        {
            public CanvasGroupState(
                CanvasGroup canvasGroup,
                bool addedAtRuntime,
                bool interactable,
                bool blocksRaycasts)
            {
                CanvasGroup = canvasGroup;
                AddedAtRuntime = addedAtRuntime;
                Interactable = interactable;
                BlocksRaycasts = blocksRaycasts;
            }

            public CanvasGroup CanvasGroup { get; }
            public bool AddedAtRuntime { get; }
            public bool Interactable { get; }
            public bool BlocksRaycasts { get; }
        }
    }
}
