// AudioAssetEditor.cs

using HarmonyAudio.Scripts;
using UnityEditor;

namespace HarmonyAudio.Editor
{
    [CustomEditor(typeof(AudioAsset))]
    public class AudioAssetEditor : UnityEditor.Editor
    {
        SerializedProperty _allowMultipleClipsProp;
        SerializedProperty _singleClipProp;
        SerializedProperty _multipleClipsProp;

        private void OnEnable()
        {
            _allowMultipleClipsProp = serializedObject.FindProperty("allowMultipleClips");
            _singleClipProp = serializedObject.FindProperty("singleClip");
            _multipleClipsProp = serializedObject.FindProperty("multipleClips");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_allowMultipleClipsProp);

            if (_allowMultipleClipsProp.boolValue)
            {
                EditorGUILayout.PropertyField(_multipleClipsProp, true);
            }
            else
            {
                EditorGUILayout.PropertyField(_singleClipProp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}