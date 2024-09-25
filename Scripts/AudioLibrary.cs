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
using HarmonyAudio.Scripts.Enums;
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
        public List<NamedMusicClip> musicClips = new List<NamedMusicClip>();

        /// <summary>
        /// A list of named sound effect audio clips.
        /// </summary>
        [Tooltip("List of sound effect audio clips with their associated names.")]
        public List<NamedSoundClip> soundClips = new List<NamedSoundClip>();
        
        [Tooltip("List of voice audio clips (only visible if VoiceManager is added).")]
        public List<NamedVoiceClip> voiceClips = new List<NamedVoiceClip>();

        private Dictionary<MusicClips, AudioClip> _musicClipsDict;
        private Dictionary<SoundClips, AudioClip> _soundClipsDict;

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
            _musicClipsDict = new Dictionary<MusicClips, AudioClip>();
            foreach (var namedClip in musicClips)
            {
                if (namedClip.clip != null)
                {
                    string clipName = namedClip.clip.name;
                    string sanitizedName = SanitizeEnumName(clipName);

                    if (System.Enum.TryParse(sanitizedName, out MusicClips musicEnum))
                    {
                        if (!_musicClipsDict.ContainsKey(musicEnum))
                            _musicClipsDict.Add(musicEnum, namedClip.clip);
                        else
                            Debug.LogWarning($"Duplicate music clip enum '{musicEnum}' found in AudioLibrary.");
                    }
                    else
                    {
                        Debug.LogWarning($"Music clip '{clipName}' does not match any enum value.");
                    }
                }
            }

            // Initialize the sound effects clips dictionary
            _soundClipsDict = new Dictionary<SoundClips, AudioClip>();
            foreach (var namedClip in soundClips)
            {
                if (namedClip.clip != null)
                {
                    string clipName = namedClip.clip.name;
                    string sanitizedName = SanitizeEnumName(clipName);

                    if (System.Enum.TryParse(sanitizedName, out SoundClips soundEnum))
                    {
                        if (!_soundClipsDict.ContainsKey(soundEnum))
                            _soundClipsDict.Add(soundEnum, namedClip.clip);
                        else
                            Debug.LogWarning($"Duplicate sound clip enum '{soundEnum}' found in AudioLibrary.");
                    }
                    else
                    {
                        Debug.LogWarning($"Sound clip '{clipName}' does not match any enum value.");
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a music audio clip by its name.
        /// </summary>
        /// <param name="clipEnum">The name of the music clip to retrieve.</param>
        /// <returns>The <see cref="AudioClip"/> if found; otherwise, null.</returns>
        public AudioClip GetMusicClip(MusicClips clipEnum)
        {
            if (_musicClipsDict == null || _musicClipsDict.Count == 0)
                InitializeDictionaries();

            if (_musicClipsDict.TryGetValue(clipEnum, out var clip))
                return clip;

            Debug.LogWarning($"Music clip '{clipEnum}' not found in AudioLibrary.");
            return null;
        }

        /// <summary>
        /// Retrieves a sound effect audio clip by its name.
        /// </summary>
        /// <param name="clipEnum">The name of the sound effect clip to retrieve.</param>
        /// <returns>The <see cref="AudioClip"/> if found; otherwise, null.</returns>
        public AudioClip GetSoundClip(SoundClips clipEnum)
        {
            if (_soundClipsDict == null || _soundClipsDict.Count == 0)
                InitializeDictionaries();

            if (_soundClipsDict.TryGetValue(clipEnum, out var clip))
                return clip;

            Debug.LogWarning($"Sound clip '{clipEnum}' not found in AudioLibrary.");
            return null;
        }
        /// <summary>
        /// Sanitizes a string by removing invalid characters and replacing spaces with underscores.
        /// </summary>
        /// <param name="clipName"></param>
        /// <returns></returns>
        private string SanitizeEnumName(string clipName)
        {
            // Remove invalid characters and replace spaces with underscores
            string sanitized = System.Text.RegularExpressions.Regex.Replace(clipName, @"[^a-zA-Z0-9_]", "");

            if (string.IsNullOrEmpty(sanitized))
            {
                sanitized = "Unknown";
            }
            else if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized; // Enums cannot start with a digit
            }

            return sanitized;
        }

    }

    [System.Serializable]
    public class NamedMusicClip
    {
        [Tooltip("The audio clip asset.")]
        public AudioClip clip;
    }

    [System.Serializable]
    public class NamedSoundClip
    {
        [Tooltip("The audio clip asset.")]
        public AudioClip clip;
    }
    
    [System.Serializable]
    public class NamedVoiceClip
    {
        [Tooltip("The audio clip asset.")]
        public AudioClip clip;
    }

}
