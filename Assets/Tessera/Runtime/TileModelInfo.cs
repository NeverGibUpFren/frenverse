using DeBroglie;
using DeBroglie.Topo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// Contains an analysis of a set of model tiles. 
    /// In particular, it has adjacency information which lets
    /// you look up which tiles connect to each other, and "canonicalization"
    /// which describes tile symmetry.
    /// 
    /// </summary>
    internal class TileModelInfo
    {
        // All tiles, and the weights
        public List<(Tile, float)> AllTiles { get; set; }

        // Adjacencies necessary to make big tiles work
        public List<InternalAdjacency> InternalAdjacencies { get; set; }

        // Tiles sorted by their outside face
        public Dictionary<Sylves.CellDir, List<(FaceDetails, Tile)>> SylvesTilesByDirection { get; set; }

        // Relationship between Sylves dirs and DeBroglie dirs.
        public BiMap<Sylves.CellDir, Direction> SylvesDirectionMapping { get; set; }

        // For Symmetric tiles, picks a particular rotation to be the canonical one.
        // All the other equivalent rotations are not used in DeBroglie, but are listed here
        // so we can rotate tiles then find the canonical one.
        public Dictionary<Tile, Tile> Canonicalization { get; set; }

        // Inverse of Canonicalization
        public ILookup<Tile, Tile> Uncanonicalization { get; set; }

        internal class InternalAdjacency
        {
            public Tile Src { get; set; }
            public Tile Dest { get; set; }
            // Dir from Src to Dest in offset space (i.e.pre protation)
            public CellFaceDir OffsetDir { get; set; }
            // Dir from Src to Dest in gird space (i.e. post protation)
            public CellFaceDir GridDir { get; set; }
            public Sylves.CellDir SylvesGridDir { get; set; }
            public Sylves.CellDir SylvesInverseGridDir { get; set; }
        }


        /// <summary>
        /// Summarizes the tiles, in preparation for building a model.
        /// </summary>
        internal static TileModelInfo Create(List<TileEntry> tiles, Sylves.IGrid sylvesCellGrid)
        {
            var allTiles = new List<(Tile, float)>();
            var internalAdjacencies = new List<InternalAdjacency>();

            var sylvesCellType = sylvesCellGrid.GetCellType();

            var sylvesDirectionMapping = SylvesDeBroglieUtils.GetDirectionMapping(sylvesCellGrid);

            var sylvesTilesByDirection = sylvesCellType.GetCellDirs().ToDictionary(d => d, _ => new List<(FaceDetails, Tile)>());

            var tileCosts = new Dictionary<TesseraTile, int>();

            if (tiles == null || tiles.Count == 0)
            {
                throw new Exception("Cannot run generator with zero tiles configured.");
            }

            // Canonicalize
            var canonical = new Dictionary<Tile, Tile>();
            foreach (var tile in tiles.Select(x => x.tile))
            {
                var rots = sylvesCellType.GetRotations(tile.rotatable, tile.reflectable, tile.rotationGroupType).ToList();
                var done = new HashSet<Sylves.CellRotation>();
                foreach (var rot1 in rots)
                {
                    if (tile.symmetric)
                    {
                        foreach (var rot2 in rots)
                        {
                            if (done.Contains(rot2))
                                continue;

                            if (IsPaintEquivalent(tile, rot1, rot2, out var _, out var realign))
                            {
                                foreach (var kv in realign)
                                {
                                    var modelTile1 = new ModelTile
                                    {
                                        Tile = tile,
                                        Rotation = rot1,
                                        Offset = kv.Key,
                                    };
                                    var modelTile2 = new ModelTile
                                    {
                                        Tile = tile,
                                        Rotation = rot2,
                                        Offset = kv.Value,
                                    };
                                    canonical[new Tile(modelTile2)] = new Tile(modelTile1);
                                }
                                //Debug.Log($"Canonicalize {tile} {rot2} -> {rot1}");
                                done.Add(rot2);
                            }
                        }
                    }
                    else
                    {
                        foreach(var offset in tile.sylvesOffsets)
                        {
                            var modelTile = new Tile(new ModelTile
                            {
                                Tile = tile,
                                Rotation = rot1,
                                Offset = offset,
                            });
                            canonical[modelTile] = modelTile;
                        }
                    }
                }
            }
            var isCanonical = new HashSet<Tile>(canonical.Values);
            var uncanonical = canonical.ToLookup(x => x.Value, x => x.Key);

            // Generate all tiles, and extract their face details
            foreach (var tileEntry in tiles)
            {
                var tile = tileEntry.tile;

                if (tile == null)
                    continue;
                if (!IsContiguous(tile))
                {
                    Debug.LogWarning($"Cannot use {tile} as it is not contiguous");
                    continue;
                }

                // For sylves

                foreach (var rot in sylvesCellType.GetRotations(tile.rotatable, tile.reflectable, tile.rotationGroupType))
                {
                    var sylvesOffsets = tile.sylvesOffsets;
                    // Set up internal connections
                    foreach (var offset in sylvesOffsets)
                    {
                        var modelTile = new Tile(new ModelTile(tile, rot, offset));

                        if (!isCanonical.Contains(modelTile))
                            continue;

                        var frequency = tileEntry.weight * uncanonical[modelTile].Count() / sylvesOffsets.Count;
                        allTiles.Add((modelTile, frequency));

                        // Skip tiles without any internal connections
                        if (sylvesOffsets.Count <= 1)
                            continue;

                        foreach (var dir in sylvesCellType.GetCellDirs())
                        {
                            if (!sylvesCellGrid.TryMove((Sylves.Cell)offset, dir, out var offset2, out var inverseDir, out var connection))
                                continue;
                            if(connection != new Sylves.Connection())
                            {
                                throw new NotImplementedException();
                            }
                            if (!sylvesOffsets.Contains((Vector3Int)offset2))
                                continue;
                            var modelTile2 = new Tile(new ModelTile(tile, rot, (Vector3Int)offset2));

                            if (!isCanonical.Contains(modelTile2))
                                continue;

                            internalAdjacencies.Add(new InternalAdjacency
                            {
                                Src = modelTile,
                                Dest = modelTile2,
                                SylvesGridDir = sylvesCellType.Rotate(dir, rot),
                                SylvesInverseGridDir = sylvesCellType.Rotate(inverseDir, rot),
                            });
                        }
                    }

                    // Set up external connections
                    foreach (var (offset, dir, faceDetails) in tile.sylvesFaceDetails)
                    {
                        var modelTile = new Tile(new ModelTile(tile, rot, offset));

                        if (!isCanonical.Contains(modelTile))
                            continue;

                        var (rFaceDir, rFaceDetails) = sylvesCellType.RotateBy(dir, faceDetails, rot);
                        sylvesTilesByDirection[rFaceDir].Add((rFaceDetails, modelTile));
                    }
                }
            }

            return new TileModelInfo
            {
                AllTiles = allTiles,
                InternalAdjacencies = internalAdjacencies,
                SylvesTilesByDirection = sylvesTilesByDirection,
                SylvesDirectionMapping = sylvesDirectionMapping,
                Canonicalization = canonical,
                Uncanonicalization = uncanonical,
            };
        }

        /// <summary>
        /// Returns true if the tile has the same paint in these two different orientations, and if so
        /// the mapping from rot1 to rot2.
        /// </summary>
        private static bool IsPaintEquivalent(TesseraTileBase tile, Sylves.CellRotation rot1, Sylves.CellRotation rot2, out Sylves.CellRotation rotation, out IDictionary<Vector3Int, Vector3Int> realign)
        {
            var cellGrid = tile.SylvesCellGrid;
            var cellType = cellGrid.GetCellType();
            rotation = cellType.Multiply(cellType.Invert(rot1), rot2);
            var offsets = new HashSet<Sylves.Cell>(tile.sylvesOffsets.Select(x=> (Sylves.Cell)(x)));
            var symmetry = cellGrid.FindGridSymmetry(offsets, offsets, (Sylves.Cell)tile.sylvesOffsets.First(), rotation);

            if (symmetry == null)
            {
                realign = null;
                return false;
            }
            // Check paint
            realign = new Dictionary<Vector3Int, Vector3Int>();
            foreach (var (offset, faceDir, faceDetails) in tile.sylvesFaceDetails)
            {
                if (!cellGrid.TryApplySymmetry(symmetry, (Sylves.Cell)offset, out var dest, out var r2))
                {
                    return false;
                }
                var offset2 = (Vector3Int)dest;
                realign[offset] = offset2;
                if (r2 != rotation)
                {
                    throw new NotSupportedException();
                }
                var (faceDir2, faceDetails2) = cellType.RotateBy(faceDir, faceDetails, rotation);
                var otherFaceDetails = tile.Get(offset2, faceDir2);
                if (!faceDetails2.IsEquivalent(otherFaceDetails))
                    return false;
            }
            return true;
        }

        private static bool IsContiguous(TesseraTileBase tile)
        {
            if (!(tile is TesseraTile))
            {
                // TODO
                return true;
            }

            if (tile.sylvesOffsets.Count == 1)
                return true;

            // Floodfill offset
            var offsets = new HashSet<Vector3Int>(tile.sylvesOffsets);
            var toRemove = new Stack<Vector3Int>();
            toRemove.Push(offsets.First());
            while (toRemove.Count > 0)
            {
                var o = toRemove.Pop();
                offsets.Remove(o);

                foreach (CubeFaceDir faceDir in Enum.GetValues(typeof(CubeFaceDir)))
                {
                    var o2 = o + faceDir.Forward();
                    if (offsets.Contains(o2))
                    {
                        toRemove.Push(o2);
                    }
                }
            }

            return offsets.Count == 0;
        }
    }
}
