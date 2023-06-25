using DeBroglie.Rot;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tessera
{


    /// <summary>
    /// Utility for working with meshes.
    /// > [!Note]
    /// > This class is available only in Tessera Pro
    /// </summary>
    public static class MeshUtils
    {
        // Creates an axis aligned cube that corresponds with a box collider
        private static Mesh CreateBoxMesh(Vector3 center, Vector3 size)
        {
            Vector3[] vertices = {
                new Vector3 (-0.5f, -0.5f, -0.5f),
                new Vector3 (+0.5f, -0.5f, -0.5f),
                new Vector3 (+0.5f, +0.5f, -0.5f),
                new Vector3 (-0.5f, +0.5f, -0.5f),
                new Vector3 (-0.5f, +0.5f, +0.5f),
                new Vector3 (+0.5f, +0.5f, +0.5f),
                new Vector3 (+0.5f, -0.5f, +0.5f),
                new Vector3 (-0.5f, -0.5f, +0.5f),
            };
            vertices = vertices.Select(v => center + Vector3.Scale(size, v)).ToArray();
            int[] triangles = {
                0, 2, 1,
	            0, 3, 2,
                2, 3, 4,
	            2, 4, 5,
                1, 2, 5,
	            1, 5, 6,
                0, 7, 4,
	            0, 4, 3,
                5, 4, 7,
	            5, 7, 6,
                0, 6, 7,
	            0, 1, 6
            };

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            return mesh;
        }

        /// <summary>
        /// Applies Transform gameObject and its children.
        /// Components affected:
        /// * MeshFilter
        /// * MeshColldier
        /// * BoxCollider
        /// </summary>
        public static void TransformRecursively(GameObject gameObject, Sylves.Deformation meshDeformation)
        {
            foreach (var child in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var childDeformation = (child.transform.worldToLocalMatrix * gameObject.transform.localToWorldMatrix) * meshDeformation * (gameObject.transform.worldToLocalMatrix * child.transform.localToWorldMatrix);
                if (!child.sharedMesh.isReadable) continue;
                var mesh = childDeformation.Deform(child.sharedMesh);
                mesh.hideFlags = HideFlags.HideAndDontSave;
                child.mesh = mesh;
            }
            foreach (var child in gameObject.GetComponentsInChildren<Collider>())
            {
                var childDeformation = (child.transform.worldToLocalMatrix * gameObject.transform.localToWorldMatrix) * meshDeformation * (gameObject.transform.worldToLocalMatrix * child.transform.localToWorldMatrix);
                if (child is MeshCollider meshCollider)
                {
                    meshCollider.sharedMesh = childDeformation.Deform(meshCollider.sharedMesh);
                }
                else if (child is BoxCollider boxCollider)
                {
                    // Convert box colliders to mesh colliders.
                    var childGo = child.gameObject;
                    var newMeshCollider = childGo.AddComponent<MeshCollider>();
                    newMeshCollider.enabled = child.enabled;
                    newMeshCollider.hideFlags = child.hideFlags;
                    newMeshCollider.isTrigger = child.isTrigger;
                    newMeshCollider.sharedMaterial = child.sharedMaterial;
                    newMeshCollider.name = child.name;
                    newMeshCollider.convex = false;// Cannot be sure of this
                    var mesh = CreateBoxMesh(boxCollider.center, boxCollider.size);
                    mesh.hideFlags = HideFlags.HideAndDontSave;
                    newMeshCollider.sharedMesh = childDeformation.Deform(mesh);
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(child);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(child);
                    }
                }
                else
                {
                    Debug.LogWarning($"Collider {child} is not a type Tessera supports deforming onto a mesh.");

                }
            }
        }
    }
}
