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
using UnityEngine;

namespace HarmonyAudio.Scripts
{
    public class AudioManager : MonoBehaviour
    {
        #region Singleton

        public static AudioManager Instance { get; private set; }

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Make the AudioManager persist across scenes
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

        private void Start()
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
        /// <param name="musicClipName">The name of the music clip to play.</param>
        /// <param name="loop">Whether the music should loop.</param>
        public void PlayMusic(string musicClipName, bool loop = true)
        {
            AudioClip clip = audioLibrary.GetMusicClip(musicClipName);
            if (clip != null)
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.volume = musicVolume * masterVolume;
                musicSource.Play();
            }
            else
            {
                Debug.LogWarning($"Music clip named '{musicClipName}' not found.");
            }
        }


        /// <summary>
        /// Stops the currently playing music.
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
            // Reset the volume (for fade coroutine)
            musicSource.volume = musicVolume;
        }

        /// <summary>
        /// Pauses the currently playing music.
        /// </summary>
        public void PauseMusic()
        {
            musicSource.Pause();
        }

        /// <summary>
        /// Resumes the paused music.
        /// </summary>
        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        /// <summary>
        /// Fades the music volume to a target volume over a duration.
        /// </summary>
        /// <param name="targetVolume">The target volume (0.0 to 1.0).</param>
        /// <param name="duration">The duration over which to fade.</param>
        public void FadeMusicVolume(float targetVolume, float duration)
        {
            StartCoroutine(FadeMusicVolumeCoroutine(targetVolume, duration));
        }
        
        /// <summary>
        /// Gets the current music volume.
        /// </summary>
        /// <returns>The music volume (0.0 to 1.0).</returns>
        public float GetMusicVolume()
        {
            return musicVolume;
        }

        #endregion

        #region Sound Effect Playback Methods

        /// <summary>
        /// Plays a sound effect by name.
        /// </summary>
        /// <param name="sfxClipName">The name of the sound effect clip to play.</param>
        public void PlaySoundEffect(string sfxClipName)
        {
            AudioClip clip = audioLibrary.GetSfxClip(sfxClipName);
            if (clip != null)
            {
                AudioSource availableSource = _sfxSources.Find(s => !s.isPlaying);
                if (availableSource != null)
                {
                    availableSource.volume = sfxVolume * masterVolume;
                    availableSource.PlayOneShot(clip);
                }
                else
                {
                    Debug.LogWarning("All SFX AudioSources are busy.");
                }
            }
            else
            {
                Debug.LogWarning($"SFX clip named '{sfxClipName}' not found.");
            }
        }

        
        /// <summary>
        /// Gets the current sound effects volume.
        /// </summary>
        /// <returns>The SFX volume (0.0 to 1.0).</returns>
        public float GetSfxVolume()
        {
            return sfxVolume;
        }

        #endregion

        #region Volume Control Methods

        /// <summary>
        /// Sets the music volume.
        /// </summary>
        /// <param name="volume">The new volume level (0.0 to 1.0).</param>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume * masterVolume;
        }

        /// <summary>
        /// Sets the sound effects volume.
        /// </summary>
        /// <param name="volume">The new volume level (0.0 to 1.0).</param>
        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            // Update all SFX sources volumes
            if (_sfxSources != null)
            {
                foreach (var source in _sfxSources)
                {
                    source.volume = sfxVolume * masterVolume;
                }
            }
        }
        
        /// <summary>
        /// Sets the master volume.
        /// </summary>
        /// <param name="volume">The new master volume level (0.0 to 1.0).</param>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAudioSourceVolumes();
        }

        /// <summary>
        /// Gets the current master volume.
        /// </summary>
        /// <returns>The master volume (0.0 to 1.0).</returns>
        public float GetMasterVolume()
        {
            return masterVolume;
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
        public void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
            PlayerPrefs.Save();
        }

        public void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);

            UpdateAudioSourceVolumes();
        }

        #endregion
    }
}
