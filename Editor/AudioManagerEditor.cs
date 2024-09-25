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
        private SerializedProperty _soundsVolumeProp;
        private SerializedProperty _voiceVolumeProp;
        private SerializedProperty _enableVoiceProp;

        // Variable for tracking the foldout state
        private bool _volumeControlsFoldout = true;

        private void OnEnable()
        {
            _masterVolumeProp = serializedObject.FindProperty("masterVolume");
            _musicVolumeProp = serializedObject.FindProperty("musicVolume");
            _soundsVolumeProp = serializedObject.FindProperty("soundsVolume");
            _voiceVolumeProp = serializedObject.FindProperty("voiceVolume");
            _enableVoiceProp = serializedObject.FindProperty("enableVoice");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
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

                // Sounds Volume Slider (1 to 100 scale)
                DrawVolumeSlider("Sounds Volume", _soundsVolumeProp);
                
                // If voice is enabled, draw the voice volume slider
                if (_enableVoiceProp.boolValue)
                {
                    // Voice Volume Slider (1 to 100 scale)
                    DrawVolumeSlider("Voice Volume", _voiceVolumeProp);
                }

                // Reset indent level
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            // Voice Extension Section
            EditorGUILayout.LabelField("Extensions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Voiceover extension", GUILayout.Width(150));

            string voiceButtonLabel = _enableVoiceProp.boolValue ? "Disable" : "Enable";
            if (GUILayout.Button(voiceButtonLabel, GUILayout.Width(60)))
            {
                _enableVoiceProp.boolValue = !_enableVoiceProp.boolValue;  // Toggle the enableVoice property
            }
            EditorGUILayout.EndHorizontal();
            
            if (_enableVoiceProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("voiceSourcePoolSize"), new GUIContent("Voice Sources Pool Size"));
            }
            
            // If voice extension is enabled, add a text box
            if (_enableVoiceProp.boolValue)
            {
                EditorGUILayout.HelpBox("Voice over extension enabled, make sure to add your new clips in your library!", MessageType.Info);
            }
            
            serializedObject.ApplyModifiedProperties();
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
