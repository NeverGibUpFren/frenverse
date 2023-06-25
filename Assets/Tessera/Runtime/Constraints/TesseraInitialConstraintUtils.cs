using DeBroglie;
using DeBroglie.Constraints;
using DeBroglie.Topo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Tessera.TesseraGeneratorHelper;

namespace Tessera
{
    /// <summary>
    /// Utilities applying ITesseraInitialConstraint
    /// </summary>
    internal class TesseraInitialConstraintHelper
    {
        private readonly TilePropagator propagator;
        private readonly Sylves.IGrid grid;
        private readonly Sylves.ICellType cellType;
        private readonly TileModelInfo tileModelInfo;
        private readonly TesseraPalette palette;
        private readonly HashSet<Tile> allTiles;

        // Use existing objects as initial constraint
        HashSet<(Vector3Int, Sylves.CellDir)> sylvesIsConstrained = new HashSet<(Vector3Int, Sylves.CellDir)>();

        public TesseraInitialConstraintHelper(TilePropagator propagator,
            Sylves.IGrid grid,
            TileModelInfo tileModelInfo,
            TesseraPalette palette)
        {
            this.propagator = propagator;
            this.grid = grid;
            this.cellType = grid.GetCellType();
            this.tileModelInfo = tileModelInfo;
            this.palette = palette;

            this.allTiles = new HashSet<Tile>(tileModelInfo.AllTiles.Select(x => x.Item1));
        }

        public void Apply(ITesseraInitialConstraint initialConstraint)
        {
            var topology = propagator.Topology;

            if (initialConstraint is TesseraInitialConstraint faceConstraint)
            {
                foreach (var (offset, dir, faceDetails) in faceConstraint.faceDetails)
                {
                    //if (grid.TryMoveByOffset(faceConstraint.cell, Vector3Int.zero, offset, grid.CellType.Invert(faceConstraint.rotation), out var dest, out var _))
                    if (grid.Unbounded.TryMoveByOffset((Sylves.Cell)faceConstraint.cell, Vector3Int.zero, offset, faceConstraint.rotation, out var dest, out var _))
                    {
                        var (rotatedFaceDir, rotatedFaceDetails) = cellType.RotateBy(dir, faceDetails, faceConstraint.rotation);
                        FaceConstrain((Vector3Int)dest, rotatedFaceDir, rotatedFaceDetails);
                    }
                }
            }
            else if (initialConstraint is TesseraPinConstraint pin)
            {
                foreach (var offset in pin.tile.sylvesOffsets)
                {
                    if (grid.TryMoveByOffset((Sylves.Cell)pin.cell, Vector3Int.zero, offset, pin.rotation, out var dest, out var _))
                    {
                        var modelTile = new Tile(new ModelTile
                        {
                            Tile = (TesseraTile)pin.tile,
                            Offset = offset,
                            Rotation = pin.rotation,
                        });
                        if (!tileModelInfo.Canonicalization.TryGetValue(modelTile, out var canonicalTile))
                        {
                            if (allTiles.Select(x => ((ModelTile)x.Value).Tile).Contains(pin.tile))
                            {
                                throw new Exception($"Cannot do pin {pin.name} as it is rotated in a way that the tile doesn't support.");
                            }
                            else
                            {
                                throw new Exception($"Cannot do pin {pin.name} as the tile is not included in the generator");
                            }
                        }
                        if (propagator.Topology.ContainsIndex(propagator.Topology.GetIndex(dest.x, dest.y, dest.z)))
                        {
                            propagator.Select(dest.x, dest.y, dest.z, canonicalTile);
                            // Only need apply the pin at a single offset
                            // as the solver will handle the rest.
                            break;
                        }
                    }
                }
            }
            else if (initialConstraint is TesseraVolumeFilter volumeFilter)
            {
                if (volumeFilter.volumeType == VolumeType.TilesetFilter)
                {
                    var tilesHs = new HashSet<TesseraTileBase>(volumeFilter.tiles);
                    var tileSet = propagator.CreateTileSet(tileModelInfo.AllTiles
                        .Select(x => x.Item1)
                        .Where(x => tilesHs.Contains(((ModelTile)x.Value).Tile)));
                    foreach (var cell in volumeFilter.cells)
                    {
                        if (propagator.Topology.ContainsIndex(propagator.Topology.GetIndex(cell.x, cell.y, cell.z)))
                        {
                            propagator.Select(cell.x, cell.y, cell.z, tileSet);
                        }
                    }
                }
            }
            else
            {
                throw new Exception($"Unexpected initial constraint type {initialConstraint.GetType()}");
            }
        }

        // Deprecate this, it depends on p being *outside* of bounds
        private void FaceConstrain(Vector3Int p, Sylves.CellDir dir, FaceDetails faceDetails)
        {
            var p1 = p;
            if (grid.TryMove((Sylves.Cell)p1, dir, out var p2, out var dir2, out var _))
            {
                FaceConstrain2((Vector3Int)p2, dir2, faceDetails);
            }
        }

        public void FaceConstrain2(Vector3Int p2, Sylves.CellDir inverseDir, FaceDetails faceDetails)
        {
            var mask = propagator.Topology.Mask;
            if (grid.IsCellInGrid((Sylves.Cell)p2) && (mask == null || mask[grid.GetIndex((Sylves.Cell)p2)]) && sylvesIsConstrained.Add((p2, inverseDir)))
            {
                //Debug.Log(("face constraint", p2, inverseDir, faceDetails));

                if (!tileModelInfo.SylvesTilesByDirection.TryGetValue(inverseDir, out var dirTiles))
                {
                    throw new Exception($"No tiles found in direction {inverseDir}");
                }

                var matchingTiles = dirTiles
                    .Where(x => palette.Match(faceDetails, x.Item1))
                    .Select(x => x.Item2)
                    .ToList();

                propagator.Select(p2.x, p2.y, p2.z, matchingTiles);
            }
        }


        // Alter the initial mask
        public static bool[] GetMask(Sylves.IGrid grid, ITopology topology, IEnumerable<ITesseraInitialConstraint> initialConstraints, bool includePins)
        {
            var mask = topology.Mask ?? Enumerable.Range(0, topology.IndexCount).Select(x => true).ToArray();

            var cellType = grid.GetCellType();

            // Use existing objects as mask
            foreach (var ic in initialConstraints)
            {
                if (ic is TesseraInitialConstraint faceConstraint)
                {
                    foreach (var offset in faceConstraint.offsets)
                    {
                        if (grid.TryMoveByOffset((Sylves.Cell)faceConstraint.cell, Vector3Int.zero, offset, faceConstraint.rotation, out var p2, out var _))
                        {
                            mask[grid.GetIndex(p2)] = false;
                        }
                    }
                }
                else if (ic is TesseraPinConstraint pin)
                {
                    if(includePins)
                    {
                        foreach(var offset in pin.tile.sylvesOffsets)
                        {
                            if (grid.TryMoveByOffset((Sylves.Cell)pin.cell, Vector3Int.zero, offset, pin.rotation, out var dest, out var _))
                            {
                                mask[grid.GetIndex(dest)] = false;
                            }
                        }
                    }
                }
                else if (ic is TesseraVolumeFilter volumeFilter)
                {
                    if(volumeFilter.volumeType == VolumeType.MaskOut)
                    {
                        foreach(var cell in volumeFilter.cells)
                        {
                            mask[grid.GetIndex((Sylves.Cell)cell)] = false;
                        }
                    }
                }
                else
                {
                    throw new Exception($"Unexpected initial constraint type {ic.GetType()}");
                }
            }

            return mask;
        }
    }
}
