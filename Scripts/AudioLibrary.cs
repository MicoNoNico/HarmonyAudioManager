/*
 * Harmony - Simple Audio Manager
 * Author: Niccolò Chiodo
 * Copyright © 2024 Niccolò Chiodo
 * 
 * View License in HarmonyAudio/LICENSE.md
 * 
 * Description:
 * A scriptable object that stores and manages audio clips for music and sound effects,
 * allowing efficient retrieval and organization within the audio manager system.
 */


using System.Collections.Generic;
using UnityEngine;

namespace HarmonyAudio.Scripts
{
    /// <summary>
    /// Represents a library of audio clips categorized into music and sound effects.
    /// Provides methods for retrieving audio clips by name.
    /// </summary>
    [CreateAssetMenu(fileName = "Audio Library", menuName = "HarmonyAudio/New Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        /// <summary>
        /// A list of named music audio clips.
        /// </summary>
        [Tooltip("List of music audio clips with their associated names.")]
        public List<NamedAudioClip> musicClips = new List<NamedAudioClip>();

        /// <summary>
        /// A list of named sound effect audio clips.
        /// </summary>
        [Tooltip("List of sound effect audio clips with their associated names.")]
        public List<NamedAudioClip> sfxClips = new List<NamedAudioClip>();

        private Dictionary<string, AudioClip> _musicClipsDict;
        private Dictionary<string, AudioClip> _sfxClipsDict;

        private void OnEnable()
        {
            InitializeDictionaries();
        }

        /// <summary>
        /// Initializes the dictionaries for quick lookup of audio clips by name.
        /// </summary>
        private void InitializeDictionaries()
        {
            // Initialize the music clips dictionary
            _musicClipsDict = new Dictionary<string, AudioClip>();
            foreach (var namedClip in musicClips)
            {
                if (!_musicClipsDict.ContainsKey(namedClip.clipName))
                    _musicClipsDict.Add(namedClip.clipName, namedClip.clip);
                else
                    Debug.LogWarning($"Duplicate music clip name '{namedClip.clipName}' found in AudioLibrary.");
            }

            // Initialize the sound effects clips dictionary
            _sfxClipsDict = new Dictionary<string, AudioClip>();
            foreach (var namedClip in sfxClips)
            {
                if (!_sfxClipsDict.ContainsKey(namedClip.clipName))
                    _sfxClipsDict.Add(namedClip.clipName, namedClip.clip);
                else
                    Debug.LogWarning($"Duplicate SFX clip name '{namedClip.clipName}' found in AudioLibrary.");
            }
        }

        /// <summary>
        /// Retrieves a music audio clip by its name.
        /// </summary>
        /// <param name="clipName">The name of the music clip to retrieve.</param>
        /// <returns>The <see cref="AudioClip"/> if found; otherwise, null.</returns>
        public AudioClip GetMusicClip(string clipName)
        {
            // Ensure the dictionary is initialized
            if (_musicClipsDict == null || _musicClipsDict.Count == 0)
                InitializeDictionaries();

            if (_musicClipsDict.TryGetValue(clipName, out var clip))
                return clip;

            Debug.LogWarning($"Music clip named '{clipName}' not found in AudioLibrary.");
            return null;
        }

        /// <summary>
        /// Retrieves a sound effect audio clip by its name.
        /// </summary>
        /// <param name="clipName">The name of the sound effect clip to retrieve.</param>
        /// <returns>The <see cref="AudioClip"/> if found; otherwise, null.</returns>
        public AudioClip GetSfxClip(string clipName)
        {
            // Ensure the dictionary is initialized
            if (_sfxClipsDict == null || _sfxClipsDict.Count == 0)
                InitializeDictionaries();

            if (_sfxClipsDict.TryGetValue(clipName, out var clip))
                return clip;

            Debug.LogWarning($"SFX clip named '{clipName}' not found in AudioLibrary.");
            return null;
        }
    }

    /// <summary>
    /// Represents an audio clip with an associated name.
    /// </summary>
    [System.Serializable]
    public class NamedAudioClip
    {
        /// <summary>
        /// The name associated with the audio clip.
        /// </summary>
        [Tooltip("The unique name for this audio clip.")]
        public string clipName;

        /// <summary>
        /// The audio clip asset.
        /// </summary>
        [Tooltip("The audio clip asset.")]
        public AudioClip clip;
    }
}
