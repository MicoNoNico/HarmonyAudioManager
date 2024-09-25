using HarmonyAudio.Scripts;
using HarmonyAudio.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace HarmonyAudio.Samples.Voice_Over
{
    public class VoiceExtensionSample : MonoBehaviour
    {
        [Header("Buttons")]
        public Button playMusicButton;
        public Button stopMusicButton;

        public Button playVoiceButton;
        public Button pauseVoiceButton;
        public Button resumeVoiceButton;
        public Button stopVoiceButton;
        public Button fadeInVoiceButton;
        public Button fadeOutVoiceButton;

        [Header("Sliders")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider voiceVolumeSlider;
        
        private void Start()
        {
            // Add listeners to buttons
            playMusicButton.onClick.AddListener(OnPlayMusic);
            stopMusicButton.onClick.AddListener(OnStopMusic);
            
            fadeInVoiceButton.onClick.AddListener(OnFadeInVoice);
            fadeOutVoiceButton.onClick.AddListener(OnFadeOutVoice);
            playVoiceButton.onClick.AddListener(OnPlayVoice);
            pauseVoiceButton.onClick.AddListener(OnPauseVoice);
            resumeVoiceButton.onClick.AddListener(OnResumeVoice);
            stopVoiceButton.onClick.AddListener(OnStopVoice);

            // Add listeners to sliders
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
            // Set slider values to AudioManager values
            SetSliders();
        }
        
        private void SetSliders()
        {
            musicVolumeSlider.value = AudioManager.GetMusicVolume();
            voiceVolumeSlider.value = AudioManager.GetVoiceVolume();
            masterVolumeSlider.value = AudioManager.GetMasterVolume();
        }

        private void OnPlayMusic()
        {
            AudioManager.PlayMusic(MusicClips.JinseibyLofium);
        }

        private void OnStopMusic()
        {
            AudioManager.StopMusic();
        }

        private void OnMusicVolumeChanged(float value)
        {
            AudioManager.SetMusicVolume(value);
        }

        private void OnVoiceVolumeChanged(float value)
        {
            AudioManager.SetVoiceVolume(value);
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            AudioManager.SetMasterVolume(value);
        }

        private void OnPlayVoice()
        {
            AudioManager.PlayVoice(VoiceClips.VoiceoverSample);
        }

        private void OnPauseVoice()
        {
            AudioManager.PauseVoice();
        }
        
        private void OnResumeVoice()
        {
            AudioManager.ResumeVoice();
        }
        
        private void OnStopVoice()
        {
            AudioManager.StopVoice();
        }
        
        private void OnFadeInVoice()
        {
            AudioManager.FadeVoiceVolume(voiceVolumeSlider.value, 1);
        }
        
        private void OnFadeOutVoice()
        {
            AudioManager.FadeVoiceVolume(0f, 1);
        }
    }
}
