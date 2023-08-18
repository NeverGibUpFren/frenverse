using UnityEngine;
using Tessera;
public class MultipassGeneratorParent : MonoBehaviour {
  public int dimension = 50;

  /// <summary>
  /// Editor function to update the dimensions of all child tessera generators.
  /// </summary>
  [ContextMenu("Update")]
  void UpdateValues() {
    foreach (Transform child in transform) {
      TesseraGenerator g = child.GetComponent<TesseraGenerator>();
      if (g != null) {
        g.size = new Vector3Int(dimension, g.size.y, dimension);
      }
    }

    var groundColl = transform.Find("Ground").GetComponent<BoxCollider>();
    groundColl.size = new Vector3(dimension, 0.001f, dimension);
  }
}