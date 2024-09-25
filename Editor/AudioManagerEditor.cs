using HarmonyAudio.Scripts;
using UnityEditor;
using UnityEngine;

namespace HarmonyAudio.Editor
{
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : UnityEditor.Editor
    {
        private Texture2D _headerImage;
        
        private SerializedProperty _masterVolumeProp;
        private SerializedProperty _musicVolumeProp;
        private SerializedProperty _soundsVolumeProp;
        private SerializedProperty _voiceVolumeProp;
        private SerializedProperty _enableVoiceProp;
        private SerializedProperty _initialVoicePoolSizeProp;
        private SerializedProperty _maxVoicePoolSizeProp;

        // Variable for tracking the foldout state
        private bool _volumeControlsFoldout = true;
        private bool _extensionsFoldout = true;

        private void OnEnable()
        {
            _headerImage = Resources.Load<Texture2D>("Banner");
            
            _masterVolumeProp = serializedObject.FindProperty("masterVolume");
            _musicVolumeProp = serializedObject.FindProperty("musicVolume");
            _soundsVolumeProp = serializedObject.FindProperty("soundsVolume");
            _voiceVolumeProp = serializedObject.FindProperty("voiceVolume");
            _enableVoiceProp = serializedObject.FindProperty("enableVoice");
            _initialVoicePoolSizeProp = serializedObject.FindProperty("initialVoicePoolSize");
            _maxVoicePoolSizeProp = serializedObject.FindProperty("maxVoicePoolSize");
        }

        public override void OnInspectorGUI()
        {
            // Custom box
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = new Texture2D(2, 2);
            boxStyle.padding = new RectOffset(10, 10, 10, 10);  // Padding inside the box
            
            serializedObject.Update();
            
            if (_headerImage != null)
            {
                float inspectorWidth = EditorGUIUtility.currentViewWidth - 30; // Get the current inspector width
                
                GUILayout.BeginHorizontal();
                //GUILayout.FlexibleSpace(); // Center the image
                GUILayout.Label(_headerImage, GUILayout.Width(inspectorWidth), GUILayout.Height(inspectorWidth/3)); // Adjust the size as needed
                //GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            
            DrawDefaultInspector();

            EditorGUILayout.Space(20);

            GUILayout.BeginVertical(boxStyle);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(10); // Adds padding before the foldout
            _volumeControlsFoldout = EditorGUILayout.Foldout(_volumeControlsFoldout, "Volume Controls", true);
            GUILayout.EndHorizontal();

            if (_volumeControlsFoldout)
            {
                EditorGUILayout.Space(10);
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
            
            GUILayout.EndVertical();

            EditorGUILayout.Space(20);
            
            GUILayout.BeginHorizontal();
            _extensionsFoldout = EditorGUILayout.Foldout(_extensionsFoldout, "Extensions", true, EditorStyles.foldoutHeader);
            GUILayout.EndHorizontal();

            if (_extensionsFoldout)
            {
                GUILayout.Space(10);
                // Begin the custom-colored section
                EditorGUILayout.BeginVertical(boxStyle);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Voice settings extension", GUILayout.Width(150));

                string voiceButtonLabel = _enableVoiceProp.boolValue ? "Disable" : "Enable";
                if (GUILayout.Button(voiceButtonLabel, GUILayout.Width(60)))
                {
                    _enableVoiceProp.boolValue = !_enableVoiceProp.boolValue;  // Toggle the enableVoice property
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                if (_enableVoiceProp.boolValue)
                {
                    EditorGUILayout.PropertyField(_initialVoicePoolSizeProp, new GUIContent("Initial Voice Pool Size"));
                    EditorGUILayout.PropertyField(_maxVoicePoolSizeProp, new GUIContent("Max Voice Pool Size (0 = Unlimited)"));
                }
                
                GUILayout.Space(10);
                
                // If voice extension is enabled, add a text box
                if (_enableVoiceProp.boolValue)
                {
                    EditorGUILayout.HelpBox("Voice over extension enabled, make sure to add your new clips in your library!", MessageType.Info);
                }
                
                // End the custom-colored section
                EditorGUILayout.EndVertical();                
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
            EditorGUILayout.LabelField(label, GUILayout.Width(150));

            // Slider with values from 1 to 100, but internally stored as 0.0 to 1.0
            float volumePercentage = volumeProperty.floatValue * 100f; // Convert from 0-1 to 0-100 for display
            volumePercentage = Mathf.Round(EditorGUILayout.Slider(volumePercentage, 0f, 100f)); // Round to nearest integer
            volumeProperty.floatValue = volumePercentage / 100f; // Convert back from 0-100 to 0-1 for internal use

            EditorGUILayout.EndHorizontal();
        }
    }
}
