using HarmonyAudio.Scripts;
using HarmonyAudio.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace HarmonyAudio.Samples.RandomSounds
{
    public class RandomSoundSample : MonoBehaviour
    {
        [Header("Buttons")]
        public Button playMusicButton;
        public Button stopMusicButton;

        public Button playSoundButton;
        
        [Header("Sliders")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider soundVolumeSlider;

        private void Start()
        {
            // Add listeners to buttons
            playMusicButton.onClick.AddListener(OnPlayMusic);
            stopMusicButton.onClick.AddListener(OnStopMusic);
            
            playSoundButton.onClick.AddListener(OnPlaySound);
            
            // Add listeners to sliders
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
            // Set slider values to AudioManager values
            SetSliders();
        }

        private void SetSliders()
        {
            musicVolumeSlider.value = AudioManager.GetMusicVolume();
            soundVolumeSlider.value = AudioManager.GetSoundVolume();
            masterVolumeSlider.value = AudioManager.GetMasterVolume();
        }

        private void OnPlayMusic()
        {
            AudioManager.PlayMusic(MusicClips.Jinsei);
        }

        private void OnStopMusic()
        {
            AudioManager.StopMusic();
        }
        
        private void OnPlaySound()
        {
            AudioManager.PlayRandomSound(SoundClips.Footsteps);
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            AudioManager.SetMusicVolume(value);
        }

        private void OnSoundVolumeChanged(float value)
        {
            AudioManager.SetSoundVolume(value);
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            AudioManager.SetMasterVolume(value);
        }
    }
}
