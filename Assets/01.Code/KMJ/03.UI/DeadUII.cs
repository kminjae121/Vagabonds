
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _01.Member.KMJ._02.Scripts._03.UI
{
    public class DeadUII : MonoBehaviour
    {
        private bool _isSetDead = false;
        [SerializeField] private Image _deadUIImage;
        [SerializeField] private Image _bloodImage;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _retryButton;

        [SerializeField] private float fadeSpeed = 1f;
        public UnityEvent FadeEndEvent;

        private void Awake()
        {
        }

        private void OnDestroy()
        {
        }

        public void SetDead()
        {
            _isSetDead = true;
        }

        private void Update()
        {
            if (_isSetDead)
            {
                Color color = _deadUIImage.color;
                
                color.a = Mathf.Lerp(color.a, 1f, Time.deltaTime * fadeSpeed);
                
                _deadUIImage.color = color;
                
                if (color.a >= 0.99f)
                {
                    color.a = 1f;
                    _deadUIImage.color = color;
                    _isSetDead = false;
                    FadeEndEvent?.Invoke();
                }
            }
        }

        public void BloodImageSetActive()
        {
            _bloodImage.gameObject.SetActive(true);
        }

        public void SetActiveButton()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
            _retryButton.transform.gameObject.SetActive(true);
            _exitButton.transform.gameObject.SetActive(true);
        }
    }
}