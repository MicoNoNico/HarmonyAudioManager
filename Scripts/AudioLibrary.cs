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
using HarmonyAudio.Scripts.Library;
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
        public List<AudioAsset> musicAssets = new List<AudioAsset>();

        /// <summary>
        /// A list of named sound effect audio clips.
        /// </summary>
        [Tooltip("List of sound effect audio clips with their associated names.")]
        public List<AudioAsset> soundAssets = new List<AudioAsset>();
        
        /// <summary>
        /// A list of named voice audio clips.
        /// </summary>
        [Tooltip("List of voice audio clips (only visible if VoiceManager is added).")]
        public List<AudioAsset> voiceAssets = new List<AudioAsset>();

        private Dictionary<MusicClips, AudioAsset> _musicAssetsDict;
        private Dictionary<SoundClips, AudioAsset> _soundAssetsDict;
        private Dictionary<VoiceClips, AudioAsset> _voiceAssetsDict;

        private void OnEnable()
        {
            InitializeDictionaries();
        }

        /// <summary>
        /// Initializes the dictionaries for quick lookup of audio clips by name.
        /// </summary>
        private void InitializeDictionaries()
        {
            // Initialize music assets dictionary
            _musicAssetsDict = InitializeAssetDictionary<MusicClips>(musicAssets);

            // Initialize sound assets dictionary
            _soundAssetsDict = InitializeAssetDictionary<SoundClips>(soundAssets);

            // Initialize voice assets dictionary
            _voiceAssetsDict = InitializeAssetDictionary<VoiceClips>(voiceAssets);
        }
        
        /// <summary>
        /// Initializes a dictionary from a list of audio assets.
        /// </summary>
        /// <param name="assets"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Dictionary<T, AudioAsset> InitializeAssetDictionary<T>(List<AudioAsset> assets) where T : struct, System.IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                Debug.LogError($"{typeof(T)} is not an enum type.");
                return null;
            }

            var dict = new Dictionary<T, AudioAsset>();
            foreach (var asset in assets)
            {
                if (asset != null)
                {
                    string sanitizedName = SanitizeEnumName(asset.name);
                    if (System.Enum.IsDefined(typeof(T), sanitizedName))
                    {
                        T enumValue = (T)System.Enum.Parse(typeof(T), sanitizedName);
                        if (!dict.ContainsKey(enumValue))
                        {
                            dict.Add(enumValue, asset);
                        }
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// Retrieves a music audio clip by its name.
        /// </summary>
        /// <param name="clipEnum">The name of the music clip to retrieve.</param>
        /// <returns>The <see cref="AudioClip"/> if found; otherwise, null.</returns>
        public AudioAsset GetMusicAsset(MusicClips clipEnum)
        {
            if (_musicAssetsDict == null || _musicAssetsDict.Count == 0)
                InitializeDictionaries();

            if (_musicAssetsDict.TryGetValue(clipEnum, out var asset))
                return asset;

            Debug.LogWarning($"Music asset '{clipEnum}' not found in AudioLibrary.");
            return null;
        }

        /// <summary>
        /// Retrieves a sound effect audio clip by its name.
        /// </summary>
        /// <param name="clipEnum">The name of the sound effect clip to retrieve.</param>
        /// <returns>The <see cref="AudioClip"/> if found; otherwise, null.</returns>
        public AudioAsset GetSoundAsset(SoundClips clipEnum)
        {
            if (_soundAssetsDict == null || _soundAssetsDict.Count == 0)
                InitializeDictionaries();

            if (_soundAssetsDict.TryGetValue(clipEnum, out var asset))
                return asset;

            Debug.LogWarning($"Sound asset '{clipEnum}' not found in AudioLibrary.");
            return null;
        }

        /// <summary>
        /// Retrieves a voice audio clip by its name.
        /// </summary>
        /// <param name="clipEnum">The name of the voice clip to retrieve.</param>
        /// <returns>The <see cref="AudioClip"/> if found; otherwise, null.</returns>
        public AudioAsset GetVoiceAsset(VoiceClips clipEnum)
        {
            if (_voiceAssetsDict == null || _voiceAssetsDict.Count == 0)
                InitializeDictionaries();

            if (_voiceAssetsDict.TryGetValue(clipEnum, out var asset))
                return asset;

            Debug.LogWarning($"Voice asset '{clipEnum}' not found in AudioLibrary.");
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
    public class NamedSoundGroup
    {
        public AudioAsset group;
    }

}
