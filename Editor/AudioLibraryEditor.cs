using System.Collections.Generic;
using System.IO;
using System.Text;
using HarmonyAudio.Scripts;
using HarmonyAudio.Scripts.Enums;
using UnityEditor;
using UnityEngine;

namespace HarmonyAudio.Editor
{
    [CustomEditor(typeof(AudioLibrary))]
    public class AudioLibraryEditor : UnityEditor.Editor
    {
        private SerializedProperty musicClipsProp;
        private SerializedProperty soundClipsProp;
        private SerializedProperty voiceClipsProp;

        private void OnEnable()
        {
            // Cache the serialized properties
            musicClipsProp = serializedObject.FindProperty("musicClips");
            soundClipsProp = serializedObject.FindProperty("soundClips");
            voiceClipsProp = serializedObject.FindProperty("voiceClips");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AudioLibrary audioLibrary = (AudioLibrary)target;
            var audioManager = FindObjectOfType<AudioManager>();

            // Draw music clips field
            EditorGUILayout.PropertyField(musicClipsProp, true);

            // Draw sound effects clips field
            EditorGUILayout.PropertyField(soundClipsProp, true);

            // Only show voice clips if VoiceManager is added
            if (audioManager != null && audioManager.GetComponent<VoiceManager>() != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Voice Clips", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(voiceClipsProp, true);
            }

            serializedObject.ApplyModifiedProperties();
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Regenerate Library", GUILayout.Height(25)))
            {
                GenerateEnums(audioLibrary);
            }
        }

        private void GenerateEnums(AudioLibrary audioLibrary)
        {
            string enumDirectory = "Assets/HarmonyAudio/Scripts/Enums";
            if (!Directory.Exists(enumDirectory))
            {
                Directory.CreateDirectory(enumDirectory);
            }

            GenerateEnumFile(enumDirectory, "MusicClips", audioLibrary.musicClips);
            GenerateEnumFile(enumDirectory, "SoundClips", audioLibrary.soundClips);
            GenerateEnumFile(enumDirectory, "VoiceClips", audioLibrary.voiceClips);

            AssetDatabase.Refresh();
            Debug.Log("Library regenerated successfully.");
        }

        private void GenerateEnumFile<T>(string directory, string enumName, List<T> clips) where T : class
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("namespace HarmonyAudio.Scripts.Enums");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic enum {enumName}");
            sb.AppendLine("\t{");

            foreach (var clip in clips)
            {
                AudioClip audioClip = null;

                if (typeof(T) == typeof(NamedMusicClip))
                {
                    audioClip = (clip as NamedMusicClip).clip;
                }
                else if (typeof(T) == typeof(NamedSoundClip))
                {
                    audioClip = (clip as NamedSoundClip).clip;
                }

                if (audioClip != null)
                {
                    string clipName = audioClip.name;
                    string sanitizedName = SanitizeEnumName(clipName);
                    sb.AppendLine($"\t\t{sanitizedName},");
                }
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string filePath = Path.Combine(directory, $"{enumName}.cs");
            File.WriteAllText(filePath, sb.ToString());
        }


        private string SanitizeEnumName(string clipName)
        {
            // Remove invalid characters and replace spaces with underscores
            string sanitized = System.Text.RegularExpressions.Regex.Replace(clipName, @"[^a-zA-Z0-9_]", "");
            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized; // Enums cannot start with a digit
            }
            return sanitized;
        }

    }
}
