using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tessera
{


    public enum TesseraMeshOutputMaterialGrouping
    {
        /// <summary>
        /// Merge everything to a single material 
        /// </summary>
        Single,
        /// <summary>
        /// Every material in the input becomes a material in the output 
        /// </summary>
        Unique,
        /// <summary>
        /// Every name of material in the input becomes a material in the output. 
        /// (useful when you have duplicate materials of the same name)
        /// </summary>
        UniqueByName,
    }

    public enum TesseraMeshOutputCollider
    {
        /// <summary>
        /// Don't do anything.
        /// </summary>
        None,
        /// <summary>
        /// Use the same mesh as MeshFilter, in a MeshCollider
        /// </summary>
        ReuseRenderMesh, 
        /// <summary>
        /// Merge all MeshColliders, other colliders ignored.
        /// </summary>
        Merge,
    }

    /// <summary>
    /// Attach this to a TesseraGenerator to output the tiles to a single mesh instead of instantiating them.
    /// > [!Note]
    /// > This class is available only in Tessera Pro
    /// </summary>
    // TODO: Manually handle mesh serialization for undo
    // https://answers.unity.com/questions/607527/is-this-possible-to-apply-undo-to-meshfiltermesh.html
    [RequireComponent(typeof(TesseraGenerator))]
    [AddComponentMenu("Tessera/Tessera Mesh Output", 40)]
    public class TesseraMeshOutput : MonoBehaviour, ITesseraTileOutput
    {
        // Material config
        public TesseraMeshOutputMaterialGrouping materialGrouping = TesseraMeshOutputMaterialGrouping.Unique;
        public Material singleMaterial;

        public TesseraMeshOutputCollider colliders = TesseraMeshOutputCollider.None;

        // Chunk config
        public bool useChunks;
        public Vector3 chunkSize = new Vector3(10, 10, 10);


        // Store of instances, for regenerating
        private Dictionary<Vector3Int, TesseraTileInstance> instances = new Dictionary<Vector3Int, TesseraTileInstance>();
        // chunks (for when useChunks is true)
        private Dictionary<Sylves.Cell, GameObject> chunks = new Dictionary<Sylves.Cell, GameObject>();

        #region Util
        /// <summary>
        /// Finds all the submeshes and the transform to tilespace
        /// </summary>
        private static IEnumerable<(Matrix4x4, MeshFilter, MeshRenderer, int subMesh)> GetSubmeshes(TesseraTileInstance i)
        {
            IEnumerable<(Matrix4x4, MeshFilter, MeshRenderer, int subMesh)> GetSubmeshes(GameObject subObject, Matrix4x4 transform)
            {
                var meshFilter = subObject.GetComponent<MeshFilter>();
                var meshRenderer = subObject.GetComponent<MeshRenderer>();
                if (meshFilter == null)
                {
                    yield break;
                }
                if (meshRenderer == null)
                {
                    throw new Exception($"Expected MeshRenderer to accompany MeshFilter on {subObject}");
                }
                var mesh = meshFilter.sharedMesh;
                if (mesh == null)
                {
                    yield break;
                }
                for (var subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
                {
                    yield return (
                        transform,
                        meshFilter,
                        meshRenderer,
                        subMesh
                    );
                }
            }


            if (i.Tile.instantiateChildrenOnly)
            {
                foreach (Transform child in i.Tile.transform)
                {
                    foreach(var t in GetSubmeshes(child.gameObject, Matrix4x4.TRS(child.localPosition, child.localRotation, child.localScale)))
                    {
                        yield return t;
                    }
                }
            }
            else
            {
                foreach (var t in GetSubmeshes(i.Tile.gameObject, Matrix4x4.identity))
                {
                    yield return t;
                }
            }
        }
        #endregion

        #region Material


        private static string GetMaterialName(Material m)
        {
            return m == null ? "null" : m.ToString() ?? "null";
        }

        private static Material GetMaterial(MeshRenderer meshRenderer, int subMesh) => meshRenderer.sharedMaterials.Length > subMesh ? meshRenderer.sharedMaterials[subMesh] : null;

        /// <summary>
        /// Returns a list of materials appropriate for this collection of instances
        /// and also a function that picks an appropriate material from this list for a given game object & submeshIndex
        /// </summary>
        private (List<Material> materials, Func<MeshRenderer, int, int> getMaterialIndex) GetMaterialsList(IEnumerable<TesseraTileInstance> tileInstances)
        {
            if(materialGrouping == TesseraMeshOutputMaterialGrouping.Single)
            {
                return (new List<Material> { singleMaterial }, (a, b) => 0);
            }
            if(materialGrouping == TesseraMeshOutputMaterialGrouping.Unique)
            {
                var materials = new List<Material>();
                var materialsByIndex = new Dictionary<Material, int>();

                foreach(var i in tileInstances)
                {
                    foreach(var (_, filter, renderer, subMesh) in GetSubmeshes(i))
                    {
                        var material = GetMaterial(renderer, subMesh);
                        if(!materialsByIndex.ContainsKey(material))
                        {
                            materialsByIndex[material] = materials.Count;
                            materials.Add(material);
                        }
                    }
                }

                return (materials, (renderer, subMesh) => materialsByIndex[GetMaterial(renderer, subMesh)]);
            }


            if (materialGrouping == TesseraMeshOutputMaterialGrouping.Unique)
            {
                var materials = new List<Material>();
                var materialsIndices = new Dictionary<Material, int>();

                foreach (var i in tileInstances)
                {
                    foreach (var (_, filter, renderer, subMesh) in GetSubmeshes(i))
                    {
                        var material = GetMaterial(renderer, subMesh);
                        if (!materialsIndices.ContainsKey(material))
                        {
                            materialsIndices[material] = materials.Count;
                            materials.Add(material);
                        }
                    }
                }

                return (materials, (renderer, subMesh) => materialsIndices[GetMaterial(renderer, subMesh)]);
            }

            if (materialGrouping == TesseraMeshOutputMaterialGrouping.UniqueByName)
            {
                var materials = new List<Material>();
                var materialsIndices = new Dictionary<string, int>();

                foreach (var i in tileInstances)
                {
                    foreach (var (_, filter, renderer, subMesh) in GetSubmeshes(i))
                    {
                        var material = GetMaterial(renderer, subMesh);
                        var name = GetMaterialName(material);
                        if (!materialsIndices.ContainsKey(name))
                        {
                            materialsIndices[name] = materials.Count;
                            materials.Add(material);
                        }
                    }
                }

                return (materials, (renderer, subMesh) => materialsIndices[GetMaterialName(GetMaterial(renderer, subMesh))]);
            }

            throw new Exception($"Unknown material grouping {materialGrouping}");

        }

        #endregion

        #region Chunking

        public Sylves.CubeGrid ChunkGrid => new Sylves.CubeGrid(chunkSize);

        public Sylves.Cell GetChunk(Sylves.IGrid grid, Sylves.Cell cell)
        {
            if (useChunks)
            {
                ChunkGrid.FindCell(grid.GetCellCenter(cell), out var chunk);
                return chunk;
            }
            else
            {
                return new Sylves.Cell();
            }
        }
        public IEnumerable<Sylves.Cell> GetCellsInChunk(Sylves.IGrid grid, Sylves.Cell chunk)
        {
            if (useChunks)
            {
                var min = Vector3.Scale(chunkSize, (Vector3)(Vector3Int)chunk);
                var max = min + chunkSize;
                return grid.GetCellsIntersectsApprox(min, max)
                    .Where(c => GetChunk(grid, c) == chunk);
            }
            else
            {
                return grid.GetCells();
            }
        }
        #endregion

        #region ITesseraTileOutput
        public bool IsEmpty
        {
            get
            {
                if(useChunks)
                {
                    return chunks.Count == 0;
                }
                else
                {
                    var targetMeshFilter = GetComponent<MeshFilter>();
                    return targetMeshFilter == null || targetMeshFilter.sharedMesh == null;
                }
            }
        }

        public bool SupportsIncremental => true;

        public void ClearTiles(IEngineInterface engine)
        {
            instances = new Dictionary<Vector3Int, TesseraTileInstance>();

            // Clear if useChunks is false
            var targetMeshFilter = GetComponent<MeshFilter>();
            if (targetMeshFilter != null)
            {
                targetMeshFilter.mesh = null;
            }

            // Clear if useChunks is true
            foreach(var chunkGameObject in chunks.Values)
            {
                DestroySoonish(chunkGameObject);
            }
            chunks = new Dictionary<Sylves.Cell, GameObject>();
            // Also clear children, for good measure (helps with app reloads)
            foreach(Transform child in transform)
            {
                DestroySoonish(child.gameObject);
            }

        }

        private void DestroySoonish(UnityEngine.Object o)
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(o);
            }
            else
            {
                GameObject.DestroyImmediate(o);
            }
        }

        public void UpdateTiles(TesseraCompletion completion, IEngineInterface engine)
        {
            var changedChunks = new HashSet<Sylves.Cell>();

            // Update instances.
            // NB: Unlike many arrays, a given TileInstance only appears once
            // in the instances dict, no matter how big
            foreach (var i in completion.tileInstances)
            {
                foreach (var cell in i.Cells)
                {
                    instances.Remove(cell);
                    changedChunks.Add(GetChunk(completion.grid, (Sylves.Cell)cell));
                }
                if (i.Tile == null)
                {
                    continue;
                }

                instances[i.Cells.First()] = i;
            }

            foreach(var chunk in changedChunks)
            {
                RegenChunk(completion.grid, chunk);
            }
        }

        #endregion

        private void ClearChunk(Sylves.IGrid grid, Sylves.Cell chunk)
        {
            if (useChunks)
            {
                if (chunks.TryGetValue(chunk, out var chunkGameObject_))
                {
                    DestroySoonish(chunkGameObject_);
                }
            }
            else
            {
                var meshFilter_ = this.GetComponent<MeshFilter>();
                meshFilter_.mesh = null;
            }
        }

        private void RegenChunkMeshes(Sylves.IGrid grid, Sylves.Cell chunk, List<TesseraTileInstance> tileInstances, GameObject chunkGameObject)
        {
            var (materials, getMaterialIndex) = GetMaterialsList(tileInstances);

            var combineInstances = materials.Select(_ => new List<CombineInstance>()).ToArray();

            var generatorSpaceToChunkSpace = chunkGameObject.transform.worldToLocalMatrix * gameObject.transform.localToWorldMatrix;

            // Convert the tileInstances to CombineInstance objects
            // grouped by material
            foreach (var i in tileInstances)
            {
                foreach (var (transform, filter, renderer, subMesh) in GetSubmeshes(i))
                {
                    var materialIndex = getMaterialIndex(renderer, subMesh);


                    CombineInstance ci;
                    if (i.MeshDeformation != null)
                    {
                        // Make new mesh, converting to tile space, then to generator space
                        var newMesh = (i.MeshDeformation * transform).Transform(filter.sharedMesh, subMesh);
                        ci = new CombineInstance
                        {
                            mesh = newMesh,
                            transform = generatorSpaceToChunkSpace,
                            subMeshIndex = 0,
                        };
                    }
                    else
                    {
                        ci = new CombineInstance
                        {
                            mesh = filter.sharedMesh,
                            transform = generatorSpaceToChunkSpace * Matrix4x4.TRS(i.LocalPosition, i.LocalRotation, i.LocalScale) * transform,
                            subMeshIndex = subMesh,
                        };
                    }

                    combineInstances[materialIndex].Add(ci);
                }
            }

            // Create a consolidated submesh for each material
            var submeshes = new CombineInstance[materials.Count];
            for (var materialIndex = 0; materialIndex < materials.Count; materialIndex++)
            {
                var m = new Mesh();
                m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                m.CombineMeshes(combineInstances[materialIndex].ToArray(), mergeSubMeshes: true);
                submeshes[materialIndex] = new CombineInstance
                {
                    mesh = m,
                    transform = Matrix4x4.identity,
                };
            }

            // combine all the submeshes into a single mesh
            var finalMesh = new Mesh();
            finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            finalMesh.CombineMeshes(submeshes, mergeSubMeshes: false);

            // Actually set up the game object
            if (!chunkGameObject.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                meshFilter = chunkGameObject.AddComponent<MeshFilter>();
            }
            meshFilter.mesh = finalMesh;

            if (!chunkGameObject.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            }
            meshRenderer.sharedMaterials = materials.ToArray();
        }

        private void RegenChunkColliders(Sylves.IGrid grid, Sylves.Cell chunk, List<TesseraTileInstance> tileInstances, GameObject chunkGameObject)
        {

            if (colliders == TesseraMeshOutputCollider.None)
            {
                // nothing
            }
            else if (colliders == TesseraMeshOutputCollider.ReuseRenderMesh)
            {

                if (!chunkGameObject.TryGetComponent<MeshCollider>(out var meshCollider))
                {
                    meshCollider = chunkGameObject.AddComponent<MeshCollider>();
                }
                meshCollider.sharedMesh = chunkGameObject.GetComponent<MeshFilter>().sharedMesh;
            }
            else if (colliders == TesseraMeshOutputCollider.Merge)
            {
                var generatorSpaceToChunkSpace = chunkGameObject.transform.worldToLocalMatrix * gameObject.transform.localToWorldMatrix;

                // Collect all mesh colliders
                var colliderMeshes = new List<CombineInstance>();
                foreach (var i in tileInstances)
                {
                    foreach (var c in i.Tile.GetComponents<MeshCollider>())
                    {
                        colliderMeshes.Add(new CombineInstance
                        {
                            mesh = c.sharedMesh,
                            transform = generatorSpaceToChunkSpace * Matrix4x4.TRS(i.LocalPosition, i.LocalRotation, i.LocalScale),
                        });
                    }
                }
                if (colliderMeshes.Count > 0)
                {
                    var combinedColliderMesh = new Mesh();
                    combinedColliderMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    combinedColliderMesh.CombineMeshes(colliderMeshes.ToArray(), mergeSubMeshes: true);
                    if (!chunkGameObject.TryGetComponent<MeshCollider>(out var meshCollider))
                    {
                        meshCollider = chunkGameObject.AddComponent<MeshCollider>();
                    }
                    meshCollider.sharedMesh = combinedColliderMesh;
                }
            }
        }

        private void RegenChunk(Sylves.IGrid grid, Sylves.Cell chunk)
        {
            var cells = GetCellsInChunk(grid, chunk).ToList();
            var tileInstances = cells
                .Where(c=> instances.ContainsKey((Vector3Int)c))
                .Select(c => instances[(Vector3Int)c])
                .ToList();

            // Establish the object in question
            GameObject chunkGameObject;
            if (useChunks)
            {
                // Need to check for null in case user has deleted!
                if (!chunks.TryGetValue(chunk, out chunkGameObject) || chunkGameObject == null)
                {
                    chunkGameObject = chunks[chunk] = new GameObject($"Chunk {chunk}", typeof(MeshFilter), typeof(MeshRenderer));
                    chunkGameObject.transform.parent = transform;
                    chunkGameObject.transform.localPosition = ChunkGrid.GetCellCenter(chunk);
                }
            }
            else
            {
                chunkGameObject = gameObject;
            }

            if (tileInstances.Count == 0)
            {
                ClearChunk(grid, chunk);
            }
            else
            {
                RegenChunkMeshes(grid, chunk, tileInstances, chunkGameObject);
                RegenChunkColliders(grid, chunk, tileInstances, chunkGameObject);
            }
        }

    }
}
