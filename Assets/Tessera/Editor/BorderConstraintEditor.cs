using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Tessera
{
    [CustomEditor(typeof(BorderConstraint))]

    public class BorderConstraintEditor : Editor
    {
        private TesseraGenerator generator;
        private SerializedProperty list;
        private ReorderableList rl;

        const int k_fieldPadding = 2;
        const int k_elementPadding = 5;
        // This can be queried with EditorGUI.GetPropertyHeight. May equal EditorGUIUtility.singleLineHeight?
        const int k_propertyHeight = 18;

        public void OnEnable()
        {
            generator = (target as BorderConstraint).GetComponent<TesseraGenerator>();
            list = serializedObject.FindProperty("borders");
            rl = new ReorderableList(serializedObject, list, true, false, false, false);

            rl.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var cellType = generator.CellType;
                //Debug.Log("drawElementCallback");
                SerializedProperty targetElement = list.GetArrayElementAtIndex(index);
                if (targetElement.hasVisibleChildren)
                    rect.xMin += 10;
                var cellFaceDirProperty = targetElement.FindPropertyRelative("cellDir");
                var tileProperty = targetElement.FindPropertyRelative("tile");

                var tileRect = rect;
                tileRect.height = EditorGUI.GetPropertyHeight(tileProperty);
                var label = new GUIContent(cellType.GetDisplayName((Sylves.CellDir)cellFaceDirProperty.intValue));
                EditorGUI.PropertyField(tileRect, tileProperty, label);
            };

            rl.elementHeight = k_propertyHeight + k_elementPadding;
        }

        public override void OnInspectorGUI()
        {
            rl.DoLayoutList();

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete && rl.index >= 0)
            {
                list.GetArrayElementAtIndex(rl.index).FindPropertyRelative("tile").objectReferenceValue = null;
            }

            serializedObject.ApplyModifiedProperties();
        }


    }
}
