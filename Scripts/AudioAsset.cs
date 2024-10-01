/*
 * Harmony - Simple Audio Manager
 * Author: Niccolò Chiodo
 * Copyright © 2024 Niccolò Chiodo
 * 
 * View License in HarmonyAudio/LICENSE.md
 * 
 * Description:
 * A scriptable object that holds a single or multiple audio clips for assignment in the library.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HarmonyAudio.Scripts
{
    [CreateAssetMenu(fileName = "New Audio Asset", menuName = "HarmonyAudio/Audio Asset")]
    public class AudioAsset : ScriptableObject
    {
        public bool allowMultipleClips = false;

        [Tooltip("Single Audio Clip with Volume")]
        public ClipWithVolume singleClip;

        [Tooltip("List of Audio Clips with individual volumes")]
        public List<ClipWithVolume> multipleClips = new List<ClipWithVolume>();
    }
    
    [Serializable]
    public class ClipWithVolume
    {
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
    }
}