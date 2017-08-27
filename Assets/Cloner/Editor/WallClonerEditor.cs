// Cloner - An example of use of procedural instancing.
// https://github.com/keijiro/Cloner

using UnityEngine;
using UnityEditor;

namespace Cloner
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(WallCloner))]
    public sealed class WallClonerEditor : Editor
    {
        SerializedProperty _columnCount;
        SerializedProperty _rowCount;
        SerializedProperty _template;
        SerializedProperty _templateScale;

        SerializedProperty _extent;
        SerializedProperty _scrollSpeed;

        SerializedProperty _displacementByNoise;
        SerializedProperty _rotationByNoise;
        SerializedProperty _scaleByNoise;
        SerializedProperty _scaleByPulse;

        SerializedProperty _noiseFrequency;
        SerializedProperty _noiseSpeed;
        SerializedProperty _pulseProbability;
        SerializedProperty _pulseSpeed;

        SerializedProperty _material;
        SerializedProperty _gradient;
        SerializedProperty _bounds;
        SerializedProperty _randomSeed;

        static class Labels
        {
            public static GUIContent frequency = new GUIContent("Frequency");
            public static GUIContent orientation = new GUIContent("Orientation");
            public static GUIContent position = new GUIContent("Position");
            public static GUIContent probability = new GUIContent("Probability");
            public static GUIContent scale = new GUIContent("Scale");
            public static GUIContent scalePulse = new GUIContent("Scale (pulse)");
            public static GUIContent speed = new GUIContent("Speed");
        }

        void OnEnable()
        {
            _columnCount = serializedObject.FindProperty("_columnCount");
            _rowCount = serializedObject.FindProperty("_rowCount");
            _template = serializedObject.FindProperty("_template");
            _templateScale = serializedObject.FindProperty("_templateScale");

            _extent = serializedObject.FindProperty("_extent");
            _scrollSpeed = serializedObject.FindProperty("_scrollSpeed");

            _displacementByNoise = serializedObject.FindProperty("_displacementByNoise");
            _rotationByNoise = serializedObject.FindProperty("_rotationByNoise");
            _scaleByNoise = serializedObject.FindProperty("_scaleByNoise");
            _scaleByPulse = serializedObject.FindProperty("_scaleByPulse");

            _noiseFrequency = serializedObject.FindProperty("_noiseFrequency");
            _noiseSpeed = serializedObject.FindProperty("_noiseSpeed");
            _pulseProbability = serializedObject.FindProperty("_pulseProbability");
            _pulseSpeed = serializedObject.FindProperty("_pulseSpeed");

            _material = serializedObject.FindProperty("_material");
            _gradient = serializedObject.FindProperty("_gradient");
            _bounds = serializedObject.FindProperty("_bounds");
            _randomSeed = serializedObject.FindProperty("_randomSeed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_columnCount);
            EditorGUILayout.PropertyField(_rowCount);
            EditorGUILayout.PropertyField(_template);
            var reallocate = EditorGUI.EndChangeCheck();
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_templateScale, Labels.scale);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_extent);
            EditorGUILayout.PropertyField(_scrollSpeed);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Modifier");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_displacementByNoise, Labels.position);
            EditorGUILayout.PropertyField(_rotationByNoise, Labels.orientation);
            EditorGUILayout.PropertyField(_scaleByNoise, Labels.scale);
            EditorGUILayout.PropertyField(_scaleByPulse, Labels.scalePulse);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Noise Field");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_noiseFrequency, Labels.frequency);
            EditorGUILayout.PropertyField(_noiseSpeed, Labels.speed);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Pulse Noise");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pulseProbability, Labels.probability);
            EditorGUILayout.PropertyField(_pulseSpeed, Labels.speed);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_material);
            EditorGUILayout.PropertyField(_gradient);
            EditorGUILayout.PropertyField(_bounds);
            EditorGUILayout.PropertyField(_randomSeed);

            serializedObject.ApplyModifiedProperties();

            if (reallocate)
                foreach (MonoBehaviour r in targets)
                    r.SendMessage("ReallocateBuffer");
        }
    }
}
