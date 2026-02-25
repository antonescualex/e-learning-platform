using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CreateProfile
{
    public class CreateProfileButton : MonoBehaviour
    {
        private static readonly float DELAY = 0.3f;
        
        [SerializeField] private Animator _animator;
        
        public void Load()
        {
            StartCoroutine(LoadRoutine());
        }

        private IEnumerator LoadRoutine()
        {
            if (_animator != null)
            {
                _animator.Play("Close");
            }

            yield return new WaitForSeconds(DELAY);
            SceneManager.LoadScene("MainMenu");
        }
    }
}