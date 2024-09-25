using HarmonyAudio.Scripts;
using HarmonyAudio.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace HarmonyAudio.Samples.Simple_Volume
{
    public class SimpleVolumeSample : MonoBehaviour
    {
        [Header("Buttons")]
        public Button playMusicButton;
        public Button pauseMusicButton;
        public Button stopMusicButton;
        public Button playSoundButton;
        public Button fadeInMusicButton;
        public Button fadeOutMusicButton;

        [Header("Sliders")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider soundsVolumeSlider;
        
        [Header("Save / Load")]
        public Button saveButton;
        public Button loadButton;

        private void Start()
        {
            // Add listeners to buttons
            playMusicButton.onClick.AddListener(OnPlayMusic);
            pauseMusicButton.onClick.AddListener(OnPauseMusic);
            stopMusicButton.onClick.AddListener(OnStopMusic);
            playSoundButton.onClick.AddListener(OnPlaySound);
            fadeInMusicButton.onClick.AddListener(OnFadeInMusic);
            fadeOutMusicButton.onClick.AddListener(OnFadeOutMusic);
            saveButton.onClick.AddListener(OnSaveButton);
            loadButton.onClick.AddListener(OnLoadButton);

            // Add listeners to sliders
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            soundsVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
            // Set slider values to AudioManager values
            SetSliders();
        }

        private void SetSliders()
        {
            musicVolumeSlider.value = AudioManager.GetMusicVolume();
            soundsVolumeSlider.value = AudioManager.GetSoundVolume();
            masterVolumeSlider.value = AudioManager.GetMasterVolume();
        }

        private void OnPlayMusic()
        {
            AudioManager.PlayMusic(MusicClips.JinseibyLofium);
        }

        private void OnPauseMusic()
        {
            AudioManager.PauseMusic();
        }

        private void OnStopMusic()
        {
            AudioManager.StopMusic();
        }

        private void OnPlaySound()
        {
            AudioManager.PlaySoundEffect(SoundClips.UISound);
        }

        private void OnFadeInMusic()
        {
            AudioManager.FadeMusicVolume(musicVolumeSlider.value, 1);
        }

        private void OnFadeOutMusic()
        {
            AudioManager.FadeMusicVolume(0f, 1);
        }

        private void OnMusicVolumeChanged(float value)
        {
            AudioManager.SetMusicVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            AudioManager.SetSfxVolume(value);
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            AudioManager.SetMasterVolume(value);
        }

        private void OnSaveButton()
        {
            AudioManager.SaveVolumeSettings();
        }
        
        private void OnLoadButton()
        {
            AudioManager.LoadVolumeSettings();
            SetSliders();
        }
    }
}
