using System.Collections.Generic;
using System.IO;
using System.Text;
using HarmonyAudio.Scripts;
using UnityEditor;
using UnityEngine;

namespace HarmonyAudio.Editor
{
    [CustomEditor(typeof(AudioLibrary))]
    public class AudioLibraryEditor : UnityEditor.Editor
    {
        private SerializedProperty _musicAssetsProp;
        private SerializedProperty _soundAssetsProp;
        private SerializedProperty _voiceAssetsProp;

        private void OnEnable()
        {
            _musicAssetsProp = serializedObject.FindProperty("musicAssets");
            _soundAssetsProp = serializedObject.FindProperty("soundAssets");
            _voiceAssetsProp = serializedObject.FindProperty("voiceAssets");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AudioLibrary audioLibrary = (AudioLibrary)target;
            var audioManager = FindObjectOfType<AudioManager>();

            // Draw music clips field
            EditorGUILayout.PropertyField(_musicAssetsProp, true);

            // Draw sound effects clips field
            EditorGUILayout.PropertyField(_soundAssetsProp, true);

            bool voiceEnabled = false;
            if (audioManager != null)
            {
                voiceEnabled = audioManager.enableVoice;
            }
            
            // Only show voice clips if Voice Extension is enabled
            if (voiceEnabled)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_voiceAssetsProp, true);
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
            var audioManager = FindObjectOfType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("AudioManager instance not found in the scene.");
                return;
            }
            
            string enumDirectory = audioManager.EnumGenerationPath;
            if (!Directory.Exists(enumDirectory))
            {
                Directory.CreateDirectory(enumDirectory);
            }

            GenerateEnumFile(enumDirectory, "MusicClips", audioLibrary.musicAssets);
            GenerateEnumFile(enumDirectory, "SoundClips", audioLibrary.soundAssets);
            if (audioManager.enableVoice) GenerateEnumFile(enumDirectory, "VoiceClips", audioLibrary.voiceAssets);

            AssetDatabase.Refresh();
            Debug.Log("Library regenerated successfully.");
        }

        private void GenerateEnumFile(string directory, string enumName, List<AudioAsset> assets)
        {
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine("namespace HarmonyAudio.Scripts.Enums");
            //sb.AppendLine("{");
            sb.AppendLine($"\tpublic enum {enumName}");
            sb.AppendLine("\t{");

            foreach (var asset in assets)
            {
                if (asset != null)
                {
                    string sanitizedName = SanitizeEnumName(asset.name);
                    sb.AppendLine($"\t\t{sanitizedName},");
                }
            }

            sb.AppendLine("\t}");
            //sb.AppendLine("}");

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
