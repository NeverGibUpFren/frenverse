using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DebugVertices : MonoBehaviour
{
  void Update()
  {
    RaycastHit hit;
    if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
      return;

    MeshCollider meshCollider = hit.collider as MeshCollider;
    if (meshCollider == null || meshCollider.sharedMesh == null)
      return;

    Mesh mesh = meshCollider.sharedMesh;
    Vector3[] vertices = mesh.vertices;
    int[] triangles = mesh.triangles;
    Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
    Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
    Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
    Transform hitTransform = hit.collider.transform;
    p0 = hitTransform.TransformPoint(p0);
    p1 = hitTransform.TransformPoint(p1);
    p2 = hitTransform.TransformPoint(p2);

    Debug.DrawLine(p0, p1, Color.red, 0.01f);
    Debug.DrawLine(p1, p2, Color.red, 0.01f);
    Debug.DrawLine(p2, p0, Color.red, 0.01f);

    if (Input.GetMouseButtonDown(0))
    {
      mesh.triangles = Cut(vertices, triangles);
    }
  }

  int[] Cut(Vector3[] vertices, int[] triangles)
  {
    List<int> indices = new List<int>(triangles);
    int count = indices.Count / 3;
    for (int i = count - 1; i >= 0; i--)
    {
      Vector3 V1 = vertices[indices[i * 3 + 0]];
      Vector3 V2 = vertices[indices[i * 3 + 1]];
      Vector3 V3 = vertices[indices[i * 3 + 2]];
      if (V1.y < 0.001f && V2.y < 0.001f && V3.y < 0.001f)
      {
        indices.RemoveRange(i * 3, 3);
      }
    }
    return indices.ToArray();
  }
}
