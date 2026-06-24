using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MathGame.UI
{
    /// <summary>
    /// 设置面板 — 音量、难度选择等
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("音量设置")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;

        [Header("显示设置")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [Header("控制")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button closeButton;

        public event System.Action OnClosed;

        private void Awake()
        {
            applyButton?.onClick.AddListener(SaveSettings);
            resetButton?.onClick.AddListener(ResetToDefaults);
            closeButton?.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                OnClosed?.Invoke();
            });
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void LoadSettings()
        {
            if (masterVolumeSlider) masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            if (sfxVolumeSlider) sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            if (bgmVolumeSlider) bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            if (fullscreenToggle) fullscreenToggle.isOn = Screen.fullScreen;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider?.value ?? 1f);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider?.value ?? 1f);
            PlayerPrefs.SetFloat("BGMVolume", bgmVolumeSlider?.value ?? 0.8f);

            if (fullscreenToggle != null)
                Screen.fullScreen = fullscreenToggle.isOn;

            PlayerPrefs.Save();
            Debug.Log("[SettingsPanel] 设置已保存");
        }

        private void ResetToDefaults()
        {
            if (masterVolumeSlider) masterVolumeSlider.value = 1f;
            if (sfxVolumeSlider) sfxVolumeSlider.value = 1f;
            if (bgmVolumeSlider) bgmVolumeSlider.value = 0.8f;
            if (fullscreenToggle) fullscreenToggle.isOn = true;
        }
    }
}
