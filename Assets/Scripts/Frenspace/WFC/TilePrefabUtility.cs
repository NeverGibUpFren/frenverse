using System.Collections.Generic;
using System.Linq;
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

    // for every building folder update mesh and mats
    foreach (var path in AssetDatabase.GetSubFolders(CITY_TILE_PATH + "Buildings")) {
      var buildingType = path.Split("/")[^1];

      foreach (var prefabPath in AssetDatabase.FindAssets("*", new[] { path }).Select(g => AssetDatabase.GUIDToAssetPath(g))) {
        var tileName = prefabPath.Split("/")[^1].Split(".")[0].Replace("_Ground", "");
        if (tileName.Contains("Empty")) continue;

        if (!meshes.ContainsKey(tileName)) {
          Debug.Log("No mesh found for " + tileName);
          continue;
        }

        var prefab = PrefabUtility.LoadPrefabContents(prefabPath);

        var (mesh, mats) = meshes[tileName];
        prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
        prefab.GetComponent<MeshRenderer>().sharedMaterials = mats
        .Select(matName => matName.Contains("Wall") ? $"Wall_{buildingType}" : matName)
        .Select(n => CITY_TILE_PATH + "Models/Materials/" + n + ".mat")
        .Select(path => (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material)))
        .ToArray();

        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
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