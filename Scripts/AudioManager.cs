/*
 * Harmony - Simple Audio Manager
 * Author: Niccolò Chiodo
 * Copyright © 2024 Niccolò Chiodo
 * 
 * View License in HarmonyAudio/LICENSE.md
 * 
 * Description:
 * Manages audio playback for music and sound effects, including volume control,
 * fading, and cross-fading between tracks.
 */

using System.Collections;
using System.Collections.Generic;
using HarmonyAudio.Scripts.Enums;
using UnityEngine;

namespace HarmonyAudio.Scripts
{
    public class AudioManager : MonoBehaviour
    {
        #region Singleton

        private static AudioManager _instance;

        protected void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject); // Make the AudioManager persist across scenes
                Initialize();
            }
            else
            {
                Destroy(gameObject); // Destroy this instance if another instance already exists
            }
        }

        #endregion

        #region Fields and Properties

        [Header("Audio Library")]
        [SerializeField] private AudioLibrary audioLibrary;
        
        [Header("References")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField, HideInInspector] private int voiceSourcePoolSize = 3;
        private AudioSource _soundSource;
        [SerializeField] private int soundSourcePoolSize = 5;

        [SerializeField, HideInInspector] private float masterVolume = 1f;
        [SerializeField, HideInInspector] private float musicVolume = 1f;
        [SerializeField, HideInInspector] private float soundsVolume = 1f;
        [SerializeField, HideInInspector] private float voiceVolume = 1f;
        
        [SerializeField, HideInInspector] public bool enableVoice;
        
        private List<AudioSource> _soundSources;
        private List<AudioSource> _voiceSources;
        
        private Dictionary<string, AudioClip> _musicClipsDict;
        private Dictionary<string, AudioClip> _sfxClipsDict;

        #endregion

        #region Initialization

        private void Initialize()
        {
            LoadVolumeSettings();

            // Initialize the SFX AudioSource pool
            _soundSources = new List<AudioSource>();
            for (int i = 0; i < soundSourcePoolSize; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = soundsVolume * masterVolume;
                _soundSources.Add(source);
            }
            
            if (enableVoice)
            {
                _voiceSources = new List<AudioSource>();
                for (int i = 0; i < voiceSourcePoolSize; i++)
                {
                    AudioSource source = gameObject.AddComponent<AudioSource>();
                    source.playOnAwake = false;
                    source.volume = voiceVolume * masterVolume;
                    _voiceSources.Add(source);
                }
            }
        }

        #endregion

        #region Music Playback Methods

        /// <summary>
        /// Plays a music clip by name.
        /// </summary>
        /// <param name="musicClip">The name of the music clip to play.</param>
        /// <param name="loop">Whether the music should loop.</param>
        public static void PlayMusic(MusicClips musicClip, bool loop = true)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found. Ensure an AudioManager exists in the scene.");
                return;
            }

            AudioClip clip = _instance.audioLibrary.GetMusicClip(musicClip);
            if (clip != null)
            {
                _instance.musicSource.clip = clip;
                _instance.musicSource.loop = loop;
                _instance.musicSource.volume = _instance.musicVolume * _instance.masterVolume;
                _instance.musicSource.Play();
            }
            else
            {
                Debug.LogWarning($"Music clip '{musicClip}' not found.");
            }
        }


        /// <summary>
        /// Stops the currently playing music.
        /// </summary>
        public static void StopMusic()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            _instance.musicSource.Stop();
            // Reset the volume (for fade coroutine)
            _instance.musicSource.volume = _instance.musicVolume;
        }

        /// <summary>
        /// Pauses the currently playing music.
        /// </summary>
        public static void PauseMusic()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            _instance.musicSource.Pause();
        }
        
        /// <summary>
        /// Resumes the paused music.
        /// </summary>
        public static void ResumeMusic()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            _instance.musicSource.UnPause();
        }

        /// <summary>
        /// Fades the music volume to a target volume over a duration.
        /// </summary>
        /// <param name="targetVolume">The target volume (0.0 to 1.0).</param>
        /// <param name="duration">The duration over which to fade.</param>
        public static void FadeMusicVolume(float targetVolume, float duration)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            _instance.StartCoroutine(_instance.FadeMusicVolumeCoroutine(targetVolume, duration));
        }
        
        /// <summary>
        /// Gets the current music volume.
        /// </summary>
        /// <returns>The music volume (0.0 to 1.0).</returns>
        public static float GetMusicVolume()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return 0f;
            }

            return _instance.musicVolume;
        }

        #endregion

        #region Sound Effect Playback Methods

        /// <summary>
        /// Plays a sound effect by name.
        /// </summary>
        /// <param name="soundClip">The name of the sound effect clip to play.</param>
        public static void PlaySoundEffect(SoundClips soundClip)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            AudioClip clip = _instance.audioLibrary.GetSoundClip(soundClip);
            if (clip != null)
            {
                AudioSource availableSource = _instance._soundSources.Find(s => !s.isPlaying);
                if (availableSource != null)
                {
                    availableSource.volume = _instance.soundsVolume * _instance.masterVolume;
                    availableSource.PlayOneShot(clip);
                }
                else
                {
                    Debug.LogWarning("All SFX AudioSources are busy.");
                }
            }
            else
            {
                Debug.LogWarning($"SFX clip '{soundClip}' not found.");
            }
        }

        
        /// <summary>
        /// Gets the current sound effects volume.
        /// </summary>
        /// <returns>The SFX volume (0.0 to 1.0).</returns>
        public static float GetSoundVolume()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return 0f;
            }

            return _instance.soundsVolume;
        }
        
        #endregion

        #region Volume Control Methods

        /// <summary>
        /// Sets the music volume.
        /// </summary>
        /// <param name="volume">The new volume level (0.0 to 1.0).</param>
        public static void SetMusicVolume(float volume)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            _instance.musicVolume = Mathf.Clamp01(volume);
            _instance.musicSource.volume = _instance.musicVolume * _instance.masterVolume;
        }

        /// <summary>
        /// Sets the sound effects volume.
        /// </summary>
        /// <param name="volume">The new volume level (0.0 to 1.0).</param>
        public static void SetSfxVolume(float volume)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            _instance.soundsVolume = Mathf.Clamp01(volume);
            // Update all SFX sources volumes
            if (_instance._soundSources != null)
            {
                foreach (var source in _instance._soundSources)
                {
                    source.volume = _instance.soundsVolume * _instance.masterVolume;
                }
            }
        }
        
        /// <summary>
        /// Sets the master volume.
        /// </summary>
        /// <param name="volume">The new master volume level (0.0 to 1.0).</param>
        public static void SetMasterVolume(float volume)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            _instance.masterVolume = Mathf.Clamp01(volume);
            _instance.UpdateAudioSourceVolumes();
        }

        /// <summary>
        /// Gets the current master volume.
        /// </summary>
        /// <returns>The master volume (0.0 to 1.0).</returns>
        public static float GetMasterVolume()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return 0f;
            }

            return _instance.masterVolume;
        }
        
        /// <summary>
        /// Updates the volume of all AudioSources based on individual and master volumes.
        /// </summary>
        private void UpdateAudioSourceVolumes()
        {
            // Update music source volume
            musicSource.volume = musicVolume * masterVolume;

            // Update all SFX sources volumes
            if (_soundSources != null)
            {
                foreach (var source in _soundSources)
                {
                    source.volume = soundsVolume * masterVolume;
                }
            }
            
            // Update voice sources volume if enabled
            if (enableVoice && _voiceSources != null)
            {
                foreach (var source in _voiceSources)
                {
                    source.volume = voiceVolume * masterVolume;
                }
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        /// Coroutine to fade the music volume to a target volume over a duration.
        /// </summary>
        /// <param name="targetVolume">The target volume.</param>
        /// <param name="duration">The duration over which to fade.</param>
        private IEnumerator FadeMusicVolumeCoroutine(float targetVolume, float duration)
        {
            float startVolume = musicVolume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                musicVolume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
                musicSource.volume = musicVolume * masterVolume;
                yield return null;
            }

            musicVolume = targetVolume;
            musicSource.volume = musicVolume * masterVolume;
        }
        
        /// <summary>
        /// Coroutine to fade the volume of all voice audio sources to a target volume over a duration.
        /// </summary>
        /// <param name="targetVolume">The target volume.</param>
        /// <param name="duration">The duration over which to fade.</param>
        private IEnumerator FadeVoiceVolumeCoroutine(float targetVolume, float duration)
        {
            float startVolume = voiceVolume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                voiceVolume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
        
                // Update volume for all active voice sources
                foreach (var source in _voiceSources)
                {
                    if (source.isPlaying)
                    {
                        source.volume = voiceVolume * masterVolume;
                    }
                }

                yield return null;
            }

            // Set the final volume
            voiceVolume = targetVolume;
            foreach (var source in _voiceSources)
            {
                if (source.isPlaying)
                {
                    source.volume = voiceVolume * masterVolume;
                }
            }
        }

        #endregion

        #region Save and Load (PlayerPrefs)

        /// <summary>
        /// Saves the current volume settings to PlayerPrefs.
        /// </summary>
        public static void SaveVolumeSettings()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            PlayerPrefs.SetFloat("MasterVolume", _instance.masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", _instance.musicVolume);
            PlayerPrefs.SetFloat("SoundsVolume", _instance.soundsVolume);
            if (_instance.enableVoice) PlayerPrefs.SetFloat("VoiceVolume", _instance.voiceVolume);
            PlayerPrefs.Save();
        }

        public static void LoadVolumeSettings()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            _instance.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            _instance.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            _instance.soundsVolume = PlayerPrefs.GetFloat("SoundsVolume", 1f);
            if (_instance.enableVoice) _instance.voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);

            _instance.UpdateAudioSourceVolumes();
        }

        #endregion
        
        #region Voice Playback Extension

        /// <summary>
        /// Plays a voice clip by name.
        /// </summary>
        /// <param name="voiceClip">The name of the voice clip to play.</param>
        /// <param name="loop">Whether the voice clip should loop.</param>
        public static void PlayVoice(VoiceClips voiceClip, bool loop = false)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found. Ensure an AudioManager exists in the scene.");
                return;
            }
            
            if (!_instance.enableVoice)
            {
                Debug.LogWarning("Voice Extension is not enabled.");
                return;
            }

            AudioClip clip = _instance.audioLibrary.GetVoiceClip(voiceClip);
            if (clip != null)
            {
                AudioSource availableSource = _instance._voiceSources.Find(s => !s.isPlaying);
                if (availableSource != null)
                {
                    availableSource.clip = clip;
                    availableSource.loop = loop;
                    availableSource.volume = _instance.voiceVolume * _instance.masterVolume;
                    availableSource.Play();
                }
                else
                {
                    Debug.LogWarning("All Voice AudioSources are busy.");
                    // TODO: expand the pool or replace the oldest playing source
                    // AudioSource newSource = _instance.gameObject.AddComponent<AudioSource>();
                    // newSource.playOnAwake = false;
                    // newSource.volume = _instance.voiceVolume * _instance.masterVolume;
                    // _instance._voiceSources.Add(newSource);
// 
                    // newSource.clip = clip;
                    // newSource.loop = loop;
                    // newSource.Play();
                }
            }
            else
            {
                Debug.LogWarning($"Voice clip '{voiceClip}' not found.");
            }
        }

        /// <summary>
        /// Stops all currently playing voice clips.
        /// </summary>
        public static void StopVoice()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }
    
            if (!_instance.enableVoice)
            {
                Debug.LogWarning("Voice Extension is not enabled.");
                return;
            }

            // Stop all playing voice audio sources
            if (_instance._voiceSources != null)
            {
                foreach (var source in _instance._voiceSources)
                {
                    if (source.isPlaying)
                    {
                        source.Stop();
                    }
                }
            }
        }

        /// <summary>
        /// Pauses all currently playing voice clips.
        /// </summary>
        public static void PauseVoice()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }
    
            if (!_instance.enableVoice)
            {
                Debug.LogWarning("Voice Extension is not enabled.");
                return;
            }

            // Pause all playing voice audio sources
            if (_instance._voiceSources != null)
            {
                foreach (var source in _instance._voiceSources)
                {
                    if (source.isPlaying)
                    {
                        source.Pause();
                    }
                }
            }
        }

        /// <summary>
        /// Resumes all paused voice clips.
        /// </summary>
        public static void ResumeVoice()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }
    
            if (!_instance.enableVoice)
            {
                Debug.LogWarning("Voice Extension is not enabled.");
                return;
            }

            // Resume all paused voice audio sources
            if (_instance._voiceSources != null)
            {
                foreach (var source in _instance._voiceSources)
                {
                    // There's no direct way to check if an AudioSource is paused,
                    // so we can call UnPause() on all sources
                    source.UnPause();
                }
            }
        }

        /// <summary>
        /// Sets the voice volume.
        /// </summary>
        /// <param name="volume">The new voice volume level (0.0 to 1.0).</param>
        public static void SetVoiceVolume(float volume)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }
            
            if (!_instance.enableVoice)
            {
                Debug.LogWarning("Voice Extension is not enabled.");
                return;
            }

            _instance.voiceVolume = Mathf.Clamp01(volume);

            // Update all voice sources volumes
            if (_instance._voiceSources != null)
            {
                foreach (var source in _instance._voiceSources)
                {
                    source.volume = _instance.voiceVolume * _instance.masterVolume;
                }
            }
        }

        /// <summary>
        /// Gets the current voice volume.
        /// </summary>
        /// <returns>The voice volume (0.0 to 1.0).</returns>
        public static float GetVoiceVolume()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return 0f;
            }
            
            if (!_instance.enableVoice)
            {
                Debug.LogWarning("Voice Extension is not enabled.");
                return 0f;
            }

            return _instance.voiceVolume;
        }
        
        /// <summary>
        /// Fades the music volume to a target volume over a duration.
        /// </summary>
        /// <param name="targetVolume">The target volume (0.0 to 1.0).</param>
        /// <param name="duration">The duration over which to fade.</param>
        public static void FadeVoiceVolume(float targetVolume, float duration)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }
            
            if (!_instance.enableVoice)
            {
                Debug.LogWarning("Voice Extension is not enabled.");
                return;
            }

            _instance.StartCoroutine(_instance.FadeVoiceVolumeCoroutine(targetVolume, duration));
        }
        
        #endregion
    }
}
