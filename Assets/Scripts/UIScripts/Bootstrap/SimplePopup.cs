using UnityEngine;

namespace UIScripts.Bootstrap
{
    public class SimplePopup : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Open()
        {
            gameObject.SetActive(true);

            if (_animator != null)
            {
                _animator.Play("Open");
            }
        }

        public void Close()
        {
            if (_animator != null)
            {
                _animator.Play("Close");
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}