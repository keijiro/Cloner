using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Cloner
{
    [CustomEditor(typeof(Tube)), CanEditMultipleObjects]
    public class TubeEditor : Editor
    {
        #region Custom inspector

        SerializedProperty _divisions;
        SerializedProperty _segments;

        void OnEnable()
        {
            _divisions = serializedObject.FindProperty("_divisions");
            _segments = serializedObject.FindProperty("_segments");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_divisions);
            EditorGUILayout.PropertyField(_segments);
            var rebuild = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            // Rebuild the mesh when the properties are changed.
            if (rebuild) foreach (var t in targets) ((Tube)t).RebuildMesh();
        }

        #endregion

        #region Menu items

        [MenuItem("Assets/Create/Cloner/Tube")]
        static void CreteTube()
        {
            // Make a proper path from the current selection.
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
                path = "Assets";
            else if (Path.GetExtension(path) != "")
                path = path.Replace(Path.GetFileName(path), "");
            var assetPathName = AssetDatabase.GenerateUniqueAssetPath(path + "/Tube.asset");

            // Create a tube asset.
            var asset = ScriptableObject.CreateInstance<Tube>();
            AssetDatabase.CreateAsset(asset, assetPathName);
            AssetDatabase.AddObjectToAsset(asset.mesh, asset);

            // Build an initial mesh for the asset.
            asset.RebuildMesh();

            // Save the generated mesh asset.
            AssetDatabase.SaveAssets();

            // Tweak the selection.
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        #endregion
    }
}
