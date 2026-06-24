using System;
using UnityEngine;
using TMPro;
using MathGame.Core.Interfaces;

namespace MathGame.UI
{
    public class MathInputPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text errorLabel;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private UnityEngine.UI.Button previewBtn;
        [SerializeField] private UnityEngine.UI.Button confirmBtn;
        [SerializeField] private UnityEngine.UI.Button cancelBtn;

        public event Action<string> OnConfirmed;
        public event Action<string> OnPreviewRequested;
        public event Action OnCancelled;

        private IMathParser mathParser;

        public void Initialize(IMathParser parser)
        {
            mathParser = parser;
        }

        private void OnEnable()
        {
            if (inputField != null)
            {
                inputField.text = "y=";
                inputField.caretPosition = 2;
                inputField.Select();
                inputField.onValueChanged.AddListener(OnInputChanged);
            }

            if (errorLabel) errorLabel.gameObject.SetActive(false);
            if (hintText) hintText.text = "支持: sin, cos, tan, sqrt, log, ln, abs, ^, +, -, *, /, pi, e";
        }

        private void OnDisable()
        {
            if (inputField) inputField.onValueChanged.RemoveListener(OnInputChanged);
        }

        private void OnInputChanged(string text)
        {
            if (!text.TrimStart().StartsWith("y="))
            {
                inputField.SetTextWithoutNotify("y=");
                inputField.caretPosition = 2;
            }
        }

        private void Awake()
        {
            if (previewBtn) previewBtn.onClick.AddListener(OnPreview);
            if (confirmBtn) confirmBtn.onClick.AddListener(OnConfirm);
            if (cancelBtn) cancelBtn.onClick.AddListener(() =>
            {
                OnCancelled?.Invoke();
                Hide();
            });
        }

        private void OnPreview()
        {
            OnPreviewRequested?.Invoke(inputField.text);
        }

        private void OnConfirm()
        {
            string expr = inputField.text;
            if (mathParser != null && mathParser.ValidateExpression(expr, out string error))
            {
                OnConfirmed?.Invoke(expr);
                Hide();
            }
            else
            {
                ShowError(error);
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
        public void ShowError(string msg) { if (errorLabel) { errorLabel.text = msg; errorLabel.gameObject.SetActive(true); } }
        public void ClearError() { if (errorLabel) errorLabel.gameObject.SetActive(false); }
    }
}
