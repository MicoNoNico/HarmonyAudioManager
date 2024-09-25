using HarmonyAudio.Scripts;
using UnityEditor;
using UnityEngine;

namespace HarmonyAudio.Editor
{
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _masterVolumeProp;
        private SerializedProperty _musicVolumeProp;
        private SerializedProperty _sfxVolumeProp;

        // Variable for tracking the foldout state
        private bool _volumeControlsFoldout = true;

        private void OnEnable()
        {
            _masterVolumeProp = serializedObject.FindProperty("masterVolume");
            _musicVolumeProp = serializedObject.FindProperty("musicVolume");
            _sfxVolumeProp = serializedObject.FindProperty("sfxVolume");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the default inspector for other properties
            DrawDefaultInspector();

            EditorGUILayout.Space();

            // Collapsible Volume Controls
            _volumeControlsFoldout = EditorGUILayout.Foldout(_volumeControlsFoldout, "Volume Controls", true);

            if (_volumeControlsFoldout)
            {
                // Indent for better visual organization
                EditorGUI.indentLevel++;

                // Master Volume Slider (1 to 100 scale)
                DrawVolumeSlider("Master Volume", _masterVolumeProp);

                // Music Volume Slider (1 to 100 scale)
                DrawVolumeSlider("Music Volume", _musicVolumeProp);

                // SFX Volume Slider (1 to 100 scale)
                DrawVolumeSlider("Sounds Volume", _sfxVolumeProp);

                // Reset indent level
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
            
            AudioManager audioManager = (AudioManager)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Extensions", EditorStyles.boldLabel);

            if (audioManager.GetComponent<VoiceManager>() == null)
            {
                if (GUILayout.Button("Add Voice Manager"))
                {
                    audioManager.gameObject.AddComponent<VoiceManager>();
                    EditorUtility.SetDirty(audioManager);
                }
            }
            else
            {
                EditorGUILayout.LabelField("Voice Manager Added");
            }
        }

        /// <summary>
        /// Draws a slider for the volume with values from 1 to 100, while internally converting to a 0-1 range.
        /// Ensures that the value is rounded to an integer.
        /// </summary>
        /// <param name="label">Label for the slider.</param>
        /// <param name="volumeProperty">Serialized property representing the volume value.</param>
        private void DrawVolumeSlider(string label, SerializedProperty volumeProperty)
        {
            EditorGUILayout.BeginHorizontal();

            // Label
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));

            // Slider with values from 1 to 100, but internally stored as 0.0 to 1.0
            float volumePercentage = volumeProperty.floatValue * 100f; // Convert from 0-1 to 0-100 for display
            volumePercentage = Mathf.Round(EditorGUILayout.Slider(volumePercentage, 0f, 100f)); // Round to nearest integer
            volumeProperty.floatValue = volumePercentage / 100f; // Convert back from 0-100 to 0-1 for internal use

            EditorGUILayout.EndHorizontal();
        }
    }
}
