using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Tessera;
using UnityEditor;
using TMPro;
using System;

[RequireComponent(typeof(TesseraGenerator))]
public class TesseraDataOutput : MonoBehaviour, ITesseraTileOutput {
    /// <summary>
    /// Attach this to a TesseraGenerator in order to generate a map of generated tiles
    /// </summary>

    public List<TesseraTileBase> TilesToExclude;

    [System.Serializable]
    public class TileData {
        public string name;
        public Vector3 pos;
    }

    [HideInInspector]
    [SerializeField]
    private TileData[] mapArr;
    private Dictionary<string, TileData> map;

    public bool IsEmpty => mapArr?.Length == 0;

    public bool SupportsIncremental => true;

    void Start() {
        map = new();
        foreach (var td in mapArr) {
            map.Add(IdFromPos(td.pos), td);
        }
    }

    public void ClearTiles(IEngineInterface engine) {
        map = null;
        mapArr = null;
    }

    public void UpdateTiles(TesseraCompletion completion, IEngineInterface engine) {
        map = new();

        var exclude = TilesToExclude.ToDictionary(x => x);
        foreach (var i in completion.tileInstances) {
            if (i.Tile == null) continue;
            if (exclude.ContainsKey(i.Tile)) continue;

            // var worldCenter = i.Rotation.eulerAngles.y > 0 ? (i.Position - i.Tile.center) : (i.Position + i.Tile.center);
            var worldCenter = i.Position + (i.Rotation * i.Tile.center);

            var id = IdFromPos(worldCenter);
            // Debug.Log(i.Tile.name + " | " + i.Rotation.eulerAngles);
            map[id] = new TileData() { name = i.Tile.name, pos = worldCenter };
        }

        mapArr = map.Select(o => o.Value).ToArray();
        new SerializedObject(gameObject).ApplyModifiedPropertiesWithoutUndo();
    }

    void OnDrawGizmosSelected() {
        if (mapArr?.Length < 1) return;

        var clrs = new Dictionary<string, Color>() {
            ["Orange"] = new Color(0.97f, 0.45f, 0.02f),
            ["Green"] = new Color(0f, 0.75f, 0.11f),
            ["Purple"] = new Color(0.54f, 0f, 0.54f),
            ["Sky1"] = new Color(0.1f, 0.1f, 0.3f),
            ["Red"] = Color.red,
        };

        foreach (var p in mapArr) {
            if (Vector3.Distance(SceneView.currentDrawingSceneView.camera.gameObject.transform.position, p.pos) > 4f) continue;
            // Draw a yellow sphere at the transform's position
            var splits = p.name.Split("_");
            Gizmos.color = clrs[splits[0]];
            Gizmos.DrawSphere(p.pos, 0.04f);
            Handles.Label(p.pos + Vector3.up * 0.1f, string.Join("_", splits.Skip(1)), new GUIStyle() { normal = { textColor = Color.red } });
        }
    }

    public TileData GetTileAt(Vector3 pos) {
        var id = IdFromPos(pos);
        return map.GetValueOrDefault(id, null);
    }

    public string IdFromPos(Vector3 pos) {
        return pos.x.ToString("0.00") + pos.y.ToString("0.00") + pos.z.ToString("0.00");
    }
}