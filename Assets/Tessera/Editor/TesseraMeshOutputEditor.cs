using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Tessera
{
    [CustomEditor(typeof(TesseraMeshOutput))]
    class TesseraMeshOutputEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var meshOutput = (TesseraMeshOutput)target;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("materialGrouping"));
            if (meshOutput.materialGrouping == TesseraMeshOutputMaterialGrouping.Single)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("singleMaterial"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("colliders"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("useChunks"));
            if (meshOutput.useChunks)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("chunkSize"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
