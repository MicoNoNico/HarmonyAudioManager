// AudioAssetEditor.cs

using HarmonyAudio.Scripts;
using UnityEditor;
using UnityEngine;

namespace HarmonyAudio.Editor
{
    [CustomEditor(typeof(AudioAsset))]
    public class AudioAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _allowMultipleClipsProp;
        private SerializedProperty _singleClipProp;
        private SerializedProperty _multipleClipsProp;

        // Spatial Audio Properties
        private SerializedProperty _useSpatialAudioProp;
        private SerializedProperty _spatialBlendProp;
        private SerializedProperty _rolloffModeProp;
        private SerializedProperty _minDistanceProp;
        private SerializedProperty _maxDistanceProp;

        private void OnEnable()
        {
            _allowMultipleClipsProp = serializedObject.FindProperty("allowMultipleClips");
            _singleClipProp = serializedObject.FindProperty("singleClip");
            _multipleClipsProp = serializedObject.FindProperty("multipleClips");

            // Initialize Spatial Audio Properties
            _useSpatialAudioProp = serializedObject.FindProperty("useSpatialAudio");
            _spatialBlendProp = serializedObject.FindProperty("spatialBlend");
            _rolloffModeProp = serializedObject.FindProperty("rolloffMode");
            _minDistanceProp = serializedObject.FindProperty("minDistance");
            _maxDistanceProp = serializedObject.FindProperty("maxDistance");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_allowMultipleClipsProp);

            if (!_allowMultipleClipsProp.boolValue)
            {
                EditorGUILayout.PropertyField(_singleClipProp);
            }
            else
            {
                EditorGUILayout.PropertyField(_multipleClipsProp, true);
            }

            // Spatial Audio Settings
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_useSpatialAudioProp);

            if (_useSpatialAudioProp.boolValue)
            {
                EditorGUILayout.PropertyField(_spatialBlendProp, new GUIContent("Spatial Blend"));
                EditorGUILayout.PropertyField(_rolloffModeProp, new GUIContent("Rolloff Mode"));
                EditorGUILayout.PropertyField(_minDistanceProp, new GUIContent("Min Distance"));
                EditorGUILayout.PropertyField(_maxDistanceProp, new GUIContent("Max Distance"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
