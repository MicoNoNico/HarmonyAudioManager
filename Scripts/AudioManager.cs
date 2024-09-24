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
        private AudioSource _sfxSource;
        [SerializeField] private int sfxSourcePoolSize = 10;

        [SerializeField, HideInInspector] private float masterVolume = 1f;
        [SerializeField, HideInInspector] private float musicVolume = 1f;
        [SerializeField, HideInInspector] private float sfxVolume = 1f;
        
        private List<AudioSource> _sfxSources;
        private Dictionary<string, AudioClip> _musicClipsDict;
        private Dictionary<string, AudioClip> _sfxClipsDict;

        #endregion

        #region Initialization

        private void Initialize()
        {
            LoadVolumeSettings();

            // Initialize the SFX AudioSource pool
            _sfxSources = new List<AudioSource>();
            for (int i = 0; i < sfxSourcePoolSize; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume * masterVolume;
                _sfxSources.Add(source);
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
                AudioSource availableSource = _instance._sfxSources.Find(s => !s.isPlaying);
                if (availableSource != null)
                {
                    availableSource.volume = _instance.sfxVolume * _instance.masterVolume;
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
        public static float GetSfxVolume()
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return 0f;
            }

            return _instance.sfxVolume;
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

            _instance.sfxVolume = Mathf.Clamp01(volume);
            // Update all SFX sources volumes
            if (_instance._sfxSources != null)
            {
                foreach (var source in _instance._sfxSources)
                {
                    source.volume = _instance.sfxVolume * _instance.masterVolume;
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
            if (_sfxSources != null)
            {
                foreach (var source in _sfxSources)
                {
                    source.volume = sfxVolume * masterVolume;
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
            PlayerPrefs.SetFloat("SfxVolume", _instance.sfxVolume);
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
            _instance.sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);

            _instance.UpdateAudioSourceVolumes();
        }

        #endregion
    }
}
