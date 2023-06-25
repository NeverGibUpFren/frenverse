using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using Tessera;
using UnityEngine;

namespace Tessera
{
    [CustomEditor(typeof(TesseraUncertainty), true)]
    public class TesseraUncertaintyEditor : Editor
    {
        private static bool showDetails = false;

        public override void OnInspectorGUI()
        {
            var u = target as TesseraUncertainty;
            if(u.modelTiles == null)
            {
                GUIStyle textStyle = EditorStyles.label;
                textStyle.wordWrap = true;
                EditorGUILayout.LabelField("Tessera will automatically fill details here if this object is used as an \"uncertainty tile\".", textStyle);
                return;
            }
            //EditorGUILayout.LabelField(u.modelTiles.Count().ToString());

            if (u.modelTiles.Count == 0)
            {
                EditorGUILayout.LabelField("Contradiction! No available tiles");
                return;
            }


            showDetails = EditorGUILayout.Toggle("Show details", showDetails);
            var styleHighlight = GUI.skin.FindStyle("MeTransitionSelectHead");

            if (showDetails)
            {
                EditorGUILayout.LabelField("All possible tiles and rotations:", EditorStyles.boldLabel);
                var uniqueTiles = u.modelTiles.GroupBy(x => x.Tile);
                EditorGUI.indentLevel++;
                foreach (var g in uniqueTiles)
                {
                    EditorGUILayout.LabelField(g.Key.name);
                    EditorGUI.indentLevel++;
                    var r = EditorGUILayout.BeginVertical();
                    GUI.Box(r, GUIContent.none, GUI.skin.box);

                    foreach(var mt in g)
                    {
                        var t = $"Rotation {mt.Rotation}, Offset {mt.Offset}";
                        EditorGUILayout.LabelField(new GUIContent(t));
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("All possible tiles:", EditorStyles.boldLabel);
                var uniqueTiles = u.modelTiles.Select(x => x.Tile).Distinct();
                EditorGUI.indentLevel++;
                foreach (var tile in uniqueTiles)
                {
                    EditorGUILayout.LabelField(tile.name);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
