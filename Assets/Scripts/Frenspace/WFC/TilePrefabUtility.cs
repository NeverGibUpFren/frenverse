using System.Collections.Generic;
using System.Linq;
using Tessera;
using UnityEditor;
using UnityEngine;

public static class TilePrefabUtility {
  [MenuItem("Assets/Generate Building Tiles", isValidateFunction: true)]
  private static bool CheckIfFBX() {
    if (Selection.gameObjects.Length == 0)
      return false;

    for (int i = 0; i < Selection.gameObjects.Length; i++)
      if (!AssetDatabase.GetAssetPath(Selection.gameObjects[i]).ToLowerInvariant().EndsWith(".fbx"))
        return false;

    return true;
  }

  [MenuItem("Assets/Generate Building Tiles")]
  private static void GenerateBuildingTiles() {
    var CITY_TILE_PATH = "Assets/Source/City/Tiles/";

    Dictionary<string, (Mesh, string[])> meshes = new();

    foreach (var mr in Selection.gameObjects[0].GetComponentsInChildren<MeshRenderer>(true)) {
      meshes.Add(
        mr.gameObject.name,
        (
          mr.gameObject.GetComponent<MeshFilter>().sharedMesh,
          mr.sharedMaterials.Select(sm => sm.name).ToArray())
      );
    }

    var colors = AssetDatabase.GetSubFolders(CITY_TILE_PATH + "Buildings/Variants").Skip(1);
    var basePrefabs = AssetDatabase.FindAssets("*", new[] { CITY_TILE_PATH + "Buildings/Variants/Base" }).Select(g => AssetDatabase.GUIDToAssetPath(g));

    // update base meshes itself
    foreach (var pp in basePrefabs) {
      var type = pp.Split("/")[^1].Split(".")[0];

      if (type.Contains("Empty")) continue;
      if (!meshes.ContainsKey(type)) continue;

      var prefab = PrefabUtility.LoadPrefabContents(pp);

      var (mesh, mats) = meshes[type];
      prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
      prefab.GetComponent<MeshRenderer>().sharedMaterials = mats
      .Select(matName => matName.Contains("Wall") ? $"Wall_Red" : matName)
      .Select(n => CITY_TILE_PATH + "Models/Materials/" + n + ".mat")
      .Select(path => (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material)))
      .ToArray();

      PrefabUtility.SaveAsPrefabAsset(prefab, pp);
      PrefabUtility.UnloadPrefabContents(prefab);
    }

    // for every building folder update mesh and mats
    foreach (var path in colors) {
      var color = path.Split("/")[^1];

      foreach (var prefabPath in basePrefabs) {
        var type = prefabPath.Split("/")[^1].Split(".")[0];

        var prefab = PrefabUtility.LoadPrefabContents(prefabPath);

        if (!type.Contains("Empty")) {
          if (!meshes.ContainsKey(type)) {
            Debug.Log("No mesh found for " + type);
            PrefabUtility.UnloadPrefabContents(prefab);
            continue;
          }

          var (mesh, mats) = meshes[type];
          prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
          prefab.GetComponent<MeshRenderer>().sharedMaterials = mats
          .Select(matName => matName.Contains("Wall") ? $"Wall_{color}" : matName)
          .Select(n => CITY_TILE_PATH + "Models/Materials/" + n + ".mat")
          .Select(path => (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material)))
          .ToArray();
        }

        var t = prefab.GetComponent<TesseraTile>();
        var colIdx = t.palette.entries.FindIndex(c => c.name == color);
        foreach (var side in t.sylvesFaceDetails) {
          if (t.palette.GetEntry(side.faceDetails.center).name == "Black") {
            side.faceDetails.top = colIdx;
            side.faceDetails.topLeft = colIdx;
            side.faceDetails.topRight = colIdx;
            side.faceDetails.bottom = colIdx;
            side.faceDetails.bottomLeft = colIdx;
            side.faceDetails.bottomRight = colIdx;
            side.faceDetails.center = colIdx;
            side.faceDetails.left = colIdx;
            side.faceDetails.right = colIdx;
          }
        }

        PrefabUtility.SaveAsPrefabAsset(prefab, path + "/" + color + "_" + type + ".prefab");
        PrefabUtility.UnloadPrefabContents(prefab);
      }
    }

    var APARTMENTS_PATH = "Assets/Resources/Apartments/";
    foreach (var path in AssetDatabase.FindAssets("*", new[] { APARTMENTS_PATH }).Select(g => AssetDatabase.GUIDToAssetPath(g))) {
      var interiorType = path.Split("/")[^1].Split(".")[0];
      if (interiorType.Contains("_"))
        interiorType = "Wallpaper_" + interiorType;

      if (!meshes.ContainsKey(interiorType)) {
        Debug.Log("No mesh found for " + interiorType);
        continue;
      }

      var prefab = PrefabUtility.LoadPrefabContents(path);
      var (mesh, mats) = meshes[interiorType];
      prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
      prefab.GetComponent<MeshRenderer>().sharedMaterials = mats
      .Select(n => CITY_TILE_PATH + "Models/Materials/" + (n.Contains("Wallpaper") ? "Wallpaper_1" : n) + ".mat")
      .Select(path => (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material)))
      .ToArray();

      PrefabUtility.SaveAsPrefabAsset(prefab, path);
      PrefabUtility.UnloadPrefabContents(prefab);
    }
  }
}