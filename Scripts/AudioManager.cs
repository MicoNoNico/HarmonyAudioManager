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

        [Header("References")]
        [SerializeField] private AudioLibrary audioLibrary;
        [SerializeField] private AudioSource musicSource;

        [Header("Pooling Settings")]
        [SerializeField] private int initialSoundPoolSize = 5;
        [SerializeField] private int maxSoundPoolSize = 10; // Set to 0 for unlimited
        [SerializeField, HideInInspector] private int initialVoicePoolSize = 3;
        [SerializeField, HideInInspector] private int maxVoicePoolSize = 10; // Set to 0 for unlimited
        
        [SerializeField, HideInInspector] private float masterVolume = 1f;
        [SerializeField, HideInInspector] private float musicVolume = 1f;
        [SerializeField, HideInInspector] private float soundsVolume = 1f;
        [SerializeField, HideInInspector] private float voiceVolume = 1f;
        
        [SerializeField, HideInInspector] public bool enableVoice;
        
        private List<AudioSource> _soundSources;
        private List<AudioSource> _voiceSources;
        
        private Dictionary<string, AudioClip> _musicClipsDict;
        private Dictionary<string, AudioClip> _sfxClipsDict;
        
        private GameObject _soundSourcesParent;
        private GameObject _voiceSourcesParent;

        #endregion

        #region Initialization

        private void Initialize()
        {
            LoadVolumeSettings();
            
            // Create parent GameObjects
            _soundSourcesParent = new GameObject("SoundSources");
            _soundSourcesParent.transform.parent = this.transform;

            _voiceSourcesParent = new GameObject("VoiceSources");
            _voiceSourcesParent.transform.parent = this.transform;

            // Initialize the SFX AudioSource pool
            _soundSources = new List<AudioSource>();
            for (int i = 0; i < initialSoundPoolSize; i++)
            {
                CreateNewSoundSource(true);
            }

            // Initialize the Voice AudioSource pool if voice is enabled
            if (enableVoice)
            {
                _voiceSources = new List<AudioSource>();
                for (int i = 0; i < initialVoicePoolSize; i++)
                {
                    CreateNewVoiceSource(true);
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

            AudioAsset asset = _instance.audioLibrary.GetMusicAsset(musicClip);
            if (asset != null)
            {
                AudioClipWithVolume clip = GetClipFromAsset(asset);

                if (clip != null)
                {
                    _instance.musicSource.clip = clip.Clip;
                    _instance.musicSource.loop = loop;
                    _instance.musicSource.volume = _instance.musicVolume * _instance.masterVolume * clip.Volume;
                    _instance.musicSource.Play();
                }
                else
                {
                    Debug.LogWarning($"No clip found in AudioAsset '{musicClip}'.");
                }
            }
            else
            {
                Debug.LogWarning($"Music asset '{musicClip}' not found.");
            }
        }

        /// <summary>
        /// Plays a random music clip from an array.
        /// </summary>
        /// <param name="musicClip">The array of music clips to choose from.</param>
        /// <param name="loop">Whether the music should loop.</param>
        public static void PlayRandomMusic(MusicClips musicClip, bool loop = true)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            AudioAsset asset = _instance.audioLibrary.GetMusicAsset(musicClip);
            if (asset != null)
            {
                AudioClipWithVolume randomClip = GetRandomClipFromAsset(asset);

                if (randomClip != null)
                {
                    _instance.musicSource.clip = randomClip.Clip;
                    _instance.musicSource.loop = loop;
                    _instance.musicSource.volume = _instance.musicVolume * _instance.masterVolume * randomClip.Volume;
                    _instance.musicSource.Play();
                }
                else
                {
                    Debug.LogWarning($"No clip found in AudioAsset '{musicClip}' to play.");
                }
            }
            else
            {
                Debug.LogWarning($"Sound asset '{musicClip}' not found.");
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
        public static void PlaySound(SoundClips soundClip)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            AudioAsset asset = _instance.audioLibrary.GetSoundAsset(soundClip);
            if (asset != null)
            {
                AudioClipWithVolume clipWithVolume = GetClipFromAsset(asset);

                if (clipWithVolume != null && clipWithVolume.Clip != null)
                {
                    AudioSource availableSource = _instance._soundSources.Find(s => !s.isPlaying);
                    if (availableSource != null)
                    {
                        availableSource.volume = _instance.soundsVolume * _instance.masterVolume * clipWithVolume.Volume;
                        availableSource.PlayOneShot(clipWithVolume.Clip);
                    }
                    else
                    {
                        if (_instance.maxSoundPoolSize == 0 ||
                            _instance._soundSources.Count < _instance.maxSoundPoolSize)
                        {
                            AudioSource newSource = _instance.CreateNewSoundSource(); // Dynamic source
                            newSource.volume = _instance.soundsVolume * _instance.masterVolume * clipWithVolume.Volume;
                            newSource.PlayOneShot(clipWithVolume.Clip);

                            // Start cleanup coroutine
                            _instance.StartCoroutine(_instance.CleanupSoundSourceAfterPlay(newSource, clipWithVolume.Clip));
                        }
                        else
                        {
                            Debug.LogWarning("All Sound AudioSources are busy, and max pool size reached.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"No clip found for '{soundClip}'.");
                }
            }
            else
            {
                Debug.LogWarning($"Sound asset '{soundClip}' not found.");
            }
        }

        /// <summary>
        /// Plays a random sound effect by name.
        /// </summary>
        /// <param name="soundClip"></param>
        public static void PlayRandomSound(SoundClips soundClip)
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance not found.");
                return;
            }

            AudioAsset asset = _instance.audioLibrary.GetSoundAsset(soundClip);
            if (asset != null)
            {
                AudioClipWithVolume clipWithVolume = GetRandomClipFromAsset(asset);

                if (clipWithVolume != null && clipWithVolume.Clip != null)
                {
                    AudioSource availableSource = _instance._soundSources.Find(s => !s.isPlaying);
                    if (availableSource != null)
                    {
                        availableSource.volume = _instance.soundsVolume * _instance.masterVolume * clipWithVolume.Volume;
                        availableSource.PlayOneShot(clipWithVolume.Clip);
                    }
                    else
                    {
                        if (_instance.maxSoundPoolSize == 0 ||
                            _instance._soundSources.Count < _instance.maxSoundPoolSize)
                        {
                            AudioSource newSource = _instance.CreateNewSoundSource(); // Dynamic source
                            newSource.volume = _instance.soundsVolume * _instance.masterVolume * clipWithVolume.Volume;
                            newSource.PlayOneShot(clipWithVolume.Clip);

                            // Start cleanup coroutine
                            _instance.StartCoroutine(_instance.CleanupSoundSourceAfterPlay(newSource, clipWithVolume.Clip));
                        }
                        else
                        {
                            Debug.LogWarning("All Sound AudioSources are busy, and max pool size reached.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"No clip found for '{soundClip}'.");
                }
            }
            else
            {
                Debug.LogWarning($"Sound asset '{soundClip}' not found.");
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
        public static void SetSoundVolume(float volume)
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
        
        /// <summary>
        /// Coroutine to clean up a sound source after it has finished playing.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="clip"></param>
        /// <returns></returns>
        private IEnumerator CleanupSoundSourceAfterPlay(AudioSource source, AudioClip clip)
        {
            // Wait for the clip to finish playing
            yield return new WaitForSeconds(clip.length + 0.1f);

            // Ensure the source has stopped playing
            while (source.isPlaying)
            {
                yield return null;
            }

            // Check if the source is dynamic (not initial)
            AudioSourceMetadata metadata = source.GetComponent<AudioSourceMetadata>();
            if (metadata != null && !metadata.isInitial)
            {
                // Remove from the pool
                _soundSources.Remove(source);

                // Destroy the GameObject
                Destroy(source.gameObject);
            }
        }

        /// <summary>
        /// Coroutine to clean up a voice source after it has finished playing.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="clip"></param>
        /// <returns></returns>
        private IEnumerator CleanupVoiceSourceAfterPlay(AudioSource source, AudioClip clip)
        {
            // Wait for the clip to finish playing
            yield return new WaitForSeconds(clip.length + 0.1f);

            // Ensure the source has stopped playing
            while (source.isPlaying)
            {
                yield return null;
            }

            // Check if the source is dynamic (not initial)
            AudioSourceMetadata metadata = source.GetComponent<AudioSourceMetadata>();
            if (metadata != null && !metadata.isInitial)
            {
                // Remove from the pool
                _voiceSources.Remove(source);

                // Destroy the GameObject
                Destroy(source.gameObject);
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

            AudioAsset asset = _instance.audioLibrary.GetVoiceAsset(voiceClip);
            if (asset != null)
            {
                AudioClipWithVolume clipWithVolume = GetClipFromAsset(asset);

                if (clipWithVolume != null && clipWithVolume.Clip != null)
                {
                    AudioSource availableSource = _instance._voiceSources.Find(s => !s.isPlaying);
                    if (availableSource != null)
                    {
                        availableSource.clip = clipWithVolume.Clip;
                        availableSource.loop = loop;
                        availableSource.volume = _instance.voiceVolume * _instance.masterVolume * clipWithVolume.Volume;
                        availableSource.Play();

                        if (!loop)
                        {
                            // Start cleanup coroutine
                            _instance.StartCoroutine(_instance.CleanupVoiceSourceAfterPlay(availableSource, clipWithVolume.Clip));
                        }
                    }
                    else
                    {
                        if (_instance.maxVoicePoolSize == 0 || _instance._voiceSources.Count < _instance.maxVoicePoolSize)
                        {
                            AudioSource newSource = _instance.CreateNewVoiceSource(); // Dynamic source
                            newSource.clip = clipWithVolume.Clip;
                            newSource.loop = loop;
                            newSource.volume = _instance.voiceVolume * _instance.masterVolume * clipWithVolume.Volume;
                            newSource.Play();

                            if (!loop)
                            {
                                // Start cleanup coroutine
                                _instance.StartCoroutine(_instance.CleanupVoiceSourceAfterPlay(newSource, clipWithVolume.Clip));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("All Voice AudioSources are busy, and max pool size reached.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"No clip found for '{voiceClip}'.");
                }
            }
            else
            {
                Debug.LogWarning($"Voice asset '{voiceClip}' not found.");
            }
        }
        
        /// <summary>
        /// Plays a random voice clip by name.
        /// </summary>
        /// <param name="voiceClip"></param>
        /// <param name="loop"></param>
        public static void PlayRandomVoice(VoiceClips voiceClip, bool loop = false)
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

            AudioAsset asset = _instance.audioLibrary.GetVoiceAsset(voiceClip);
            if (asset != null)
            {
                AudioClipWithVolume clipWithVolume = GetRandomClipFromAsset(asset);

                if (clipWithVolume != null && clipWithVolume.Clip != null)
                {
                    AudioSource availableSource = _instance._voiceSources.Find(s => !s.isPlaying);
                    if (availableSource != null)
                    {
                        availableSource.clip = clipWithVolume.Clip;
                        availableSource.loop = loop;
                        availableSource.volume = _instance.voiceVolume * _instance.masterVolume * clipWithVolume.Volume;
                        availableSource.Play();

                        if (!loop)
                        {
                            // Start cleanup coroutine
                            _instance.StartCoroutine(_instance.CleanupVoiceSourceAfterPlay(availableSource, clipWithVolume.Clip));
                        }
                    }
                    else
                    {
                        if (_instance.maxVoicePoolSize == 0 || _instance._voiceSources.Count < _instance.maxVoicePoolSize)
                        {
                            AudioSource newSource = _instance.CreateNewVoiceSource(); // Dynamic source
                            newSource.clip = clipWithVolume.Clip;
                            newSource.loop = loop;
                            newSource.volume = _instance.voiceVolume * _instance.masterVolume * clipWithVolume.Volume;
                            newSource.Play();

                            if (!loop)
                            {
                                // Start cleanup coroutine
                                _instance.StartCoroutine(_instance.CleanupVoiceSourceAfterPlay(newSource, clipWithVolume.Clip));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("All Voice AudioSources are busy, and max pool size reached.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"No clip found for '{voiceClip}'.");
                }
            }
            else
            {
                Debug.LogWarning($"Voice asset '{voiceClip}' not found.");
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
        
        #region Helper Methods
        
        /// <summary>
        /// Gets a clip from the AudioAsset.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        private static AudioClipWithVolume GetClipFromAsset(AudioAsset asset)
        {
            if (asset.allowMultipleClips)
            {
                if (asset.multipleClips.Count > 0)
                {
                    var clipWithVolume = asset.multipleClips[0];
                    return new AudioClipWithVolume(clipWithVolume.clip, clipWithVolume.volume);
                }
            }
            else
            {
                if (asset.singleClip != null && asset.singleClip.clip != null)
                {
                    return new AudioClipWithVolume(asset.singleClip.clip, asset.singleClip.volume);
                }
            }
            return null;
        }
        
        /// <summary>
        /// Gets a random clip from the AudioAsset, if it has multiple clips.
        /// </summary>
        /// <param name="asset">The AudioAsset to retrieve a random clip from.</param>
        /// <returns>A random AudioClip, or null if the asset is empty or invalid.</returns>

        private static AudioClipWithVolume GetRandomClipFromAsset(AudioAsset asset)
        {
            if (asset.allowMultipleClips && asset.multipleClips.Count > 0)
            {
                int randomIndex = Random.Range(0, asset.multipleClips.Count);
                var clipWithVolume = asset.multipleClips[randomIndex];
                return new AudioClipWithVolume(clipWithVolume.clip, clipWithVolume.volume);
            }
            else if (!asset.allowMultipleClips)
            {
                if (asset.singleClip != null && asset.singleClip.clip != null)
                {
                    return new AudioClipWithVolume(asset.singleClip.clip, asset.singleClip.volume);
                }
            }
            return null;
        }

        private AudioSource CreateNewSoundSource(bool isInitial = false)
        {
            GameObject sourceObj = new GameObject("SoundSource");
            sourceObj.transform.parent = _soundSourcesParent.transform;
            AudioSource source = sourceObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = soundsVolume * masterVolume;

            // Attach metadata component
            AudioSourceMetadata metadata = sourceObj.AddComponent<AudioSourceMetadata>();
            metadata.isInitial = isInitial;

            _soundSources.Add(source);
            return source;
        }

        private AudioSource CreateNewVoiceSource(bool isInitial = false)
        {
            GameObject sourceObj = new GameObject("VoiceSource");
            sourceObj.transform.parent = _voiceSourcesParent.transform;
            AudioSource source = sourceObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = voiceVolume * masterVolume;

            // Attach metadata component
            AudioSourceMetadata metadata = sourceObj.AddComponent<AudioSourceMetadata>();
            metadata.isInitial = isInitial;

            _voiceSources.Add(source);
            return source;
        }
        
        #endregion
    }
}

namespace HarmonyAudio.Scripts
{
    public class AudioClipWithVolume
    {
        public readonly AudioClip Clip;
        public readonly float Volume;

        public AudioClipWithVolume(AudioClip clip, float volume)
        {
            this.Clip = clip;
            this.Volume = volume;
        }
    }
}

