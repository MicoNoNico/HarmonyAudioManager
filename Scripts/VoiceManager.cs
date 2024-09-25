using UnityEngine;

namespace HarmonyAudio.Scripts
{
    public class VoiceManager : MonoBehaviour
    {
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private float voiceVolume = 1f;

        private void Awake()
        {
            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
            }
        }

        public void PlayVoice(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("Voice clip is null");
                return;
            }

            voiceSource.clip = clip;
            voiceSource.volume = voiceVolume;
            voiceSource.Play();
        }

        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            voiceSource.volume = voiceVolume;
        }

        public float GetVoiceVolume() => voiceVolume;

        public void StopVoice()
        {
            voiceSource.Stop();
        }

        public void PauseVoice()
        {
            voiceSource.Pause();
        }

        public void ResumeVoice()
        {
            voiceSource.UnPause();
        }
    }
}