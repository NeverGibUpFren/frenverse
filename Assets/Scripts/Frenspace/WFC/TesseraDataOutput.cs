using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Tessera;
using UnityEditor;

[RequireComponent(typeof(TesseraGenerator))]
public class TesseraDataOutput : MonoBehaviour, ITesseraTileOutput
{
    /// <summary>
    /// Attach this to a TesseraGenerator in order to generate a map of generated tiles
    /// </summary>

    public List<TesseraTileBase> TilesToExclude;

    [System.Serializable]
    public class TileData
    {
        public string name;
        public Vector3 pos;
    }

    [HideInInspector]
    [SerializeField]
    private TileData[] mapArr;
    private Dictionary<string, TileData> map;

    public bool IsEmpty => mapArr?.Length == 0;

    public bool SupportsIncremental => true;

    void Start()
    {
        Debug.Log(mapArr.Length);
        map = new();
        foreach (var td in mapArr)
        {
            map.Add(IdFromPos(td.pos), td);
        }
    }

    public void ClearTiles(IEngineInterface engine)
    {
        map = null;
        mapArr = null;
    }

    public void UpdateTiles(TesseraCompletion completion, IEngineInterface engine)
    {
        map = new();

        var exclude = TilesToExclude.ToDictionary(x => x);
        foreach (var i in completion.tileInstances)
        {
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

    // void OnDrawGizmosSelected()
    // {
    //     if (mapArr?.Length < 1) return;

    //     foreach (var p in mapArr)
    //     {
    //         // Draw a yellow sphere at the transform's position
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawSphere(p.pos, 0.04f);
    //     }
    // }

    public TileData GetTileAt(Vector3 pos)
    {
        var id = IdFromPos(pos);
        return map.GetValueOrDefault(id, null);
    }

    public string IdFromPos(Vector3 pos)
    {
        return pos.x.ToString("0.00") + pos.y.ToString("0.00") + pos.z.ToString("0.00");
    }
}