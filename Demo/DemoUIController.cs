using HarmonyAudio.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace HarmonyAudio.Demo
{
    public class DemoUIController : MonoBehaviour
    {
        [Header("Buttons")]
        public Button playMusicButton;
        public Button pauseMusicButton;
        public Button stopMusicButton;
        public Button playSfxButton;
        public Button fadeInMusicButton;
        public Button fadeOutMusicButton;

        [Header("Sliders")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        
        [Header("Save / Load")]
        public Button saveButton;
        public Button loadButton;

        [Header("Audio Clip Names")]
        public string musicClipName = "YourMusicClipName";
        public string sfxClipName = "YourSFXClipName";

        private void Start()
        {
            // Add listeners to buttons
            playMusicButton.onClick.AddListener(OnPlayMusic);
            pauseMusicButton.onClick.AddListener(OnPauseMusic);
            stopMusicButton.onClick.AddListener(OnStopMusic);
            playSfxButton.onClick.AddListener(OnPlaySFX);
            fadeInMusicButton.onClick.AddListener(OnFadeInMusic);
            fadeOutMusicButton.onClick.AddListener(OnFadeOutMusic);
            saveButton.onClick.AddListener(OnSaveButton);
            loadButton.onClick.AddListener(OnLoadButton);

            // Add listeners to sliders
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
            // Set slider values to AudioManager values
            SetSliders();
        }

        private void SetSliders()
        {
            musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
            sfxVolumeSlider.value = AudioManager.Instance.GetSfxVolume();
            masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
        }

        private void OnPlayMusic()
        {
            AudioManager.Instance.PlayMusic(musicClipName);
        }

        private void OnPauseMusic()
        {
            AudioManager.Instance.PauseMusic();
        }

        private void OnStopMusic()
        {
            AudioManager.Instance.StopMusic();
        }

        private void OnPlaySFX()
        {
            AudioManager.Instance.PlaySoundEffect(sfxClipName);
        }

        private void OnFadeInMusic()
        {
            AudioManager.Instance.FadeMusicVolume(musicVolumeSlider.value, 1);
        }

        private void OnFadeOutMusic()
        {
            AudioManager.Instance.FadeMusicVolume(0f, 1);
        }

        private void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }

        private void OnSaveButton()
        {
            AudioManager.Instance.SaveVolumeSettings();
        }
        
        private void OnLoadButton()
        {
            AudioManager.Instance.LoadVolumeSettings();
            SetSliders();
        }
    }
}
