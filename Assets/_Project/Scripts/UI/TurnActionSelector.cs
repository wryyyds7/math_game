using System;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    public class TurnActionSelector : MonoBehaviour
    {
        [SerializeField] private Button shootButton;
        [SerializeField] private Button blinkButton;
        [SerializeField] private Button rotateButton;

        public event Action OnShootClicked;
        public event Action OnBlinkClicked;
        public event Action OnRotateClicked;

        private void Awake()
        {
            if (shootButton) shootButton.onClick.AddListener(() => OnShootClicked?.Invoke());
            if (blinkButton) blinkButton.onClick.AddListener(() => OnBlinkClicked?.Invoke());
            if (rotateButton) rotateButton.onClick.AddListener(() => OnRotateClicked?.Invoke());
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetBlinkAvailable(bool available)
        {
            if (blinkButton) blinkButton.interactable = available;
        }
    }
}
