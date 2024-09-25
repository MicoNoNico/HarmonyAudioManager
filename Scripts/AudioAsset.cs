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

using System.Collections.Generic;
using UnityEngine;

namespace HarmonyAudio.Scripts
{
    [CreateAssetMenu(fileName = "New Audio Asset", menuName = "HarmonyAudio/Audio Asset")]
    public class AudioAsset : ScriptableObject
    {
        public bool allowMultipleClips = false;

        [Tooltip("Single Audio Clip")]
        public AudioClip singleClip;

        [Tooltip("List of Audio Clips")]
        public List<AudioClip> multipleClips = new List<AudioClip>();
    }
}