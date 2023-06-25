using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeBroglie;
using DeBroglie.Constraints;
using DeBroglie.Models;
using DeBroglie.Rot;
using DeBroglie.Topo;
using DeBroglie.Trackers;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// Ensures that the generation is symmetric when x-axis mirrored.
    /// If there are any tile constraints, they will not be mirrored.
    /// > [!Note]
    /// > This class is available only in Tessera Pro
    /// </summary>
    [AddComponentMenu("Tessera/Mirror Constraint", 20)]
    [RequireComponent(typeof(TesseraGenerator))]
    public class MirrorConstraint : TesseraConstraint
    {
        // Unused legacy field
        [SerializeField]
        private bool hasSymmetricTiles;

        // Unused legacy field
        [SerializeField]
        private List<TesseraTileBase> symmetricTilesX = new List<TesseraTileBase>();

        // Unused legacy field
        [SerializeField]
        private List<TesseraTileBase> symmetricTilesY = new List<TesseraTileBase>();

        // Unused legacy field
        [SerializeField]
        private List<TesseraTileBase> symmetricTilesZ = new List<TesseraTileBase>();

        public Axis axis;

        public enum Axis
        {
            X,
            Y,
            Z,
            W,
        }

        internal override IEnumerable<ITileConstraint> GetTileConstraint(TileModelInfo tileModelInfo, Sylves.IGrid grid)
        {
            var generator = GetComponent<TesseraGenerator>();
            if (generator.surfaceMesh != null)
            {
                throw new Exception("Mirror constraint not supported on surface meshes");
            }

            var cellType = generator.CellType;
            var modelTiles = new HashSet<ModelTile>(tileModelInfo.AllTiles.Select(x => (ModelTile)x.Item1.Value));
            Sylves.CellRotation cellRotation;
            Sylves.GridSymmetry symmetry;
            if (cellType == SylvesExtensions.CubeCellType)
            {
                cellRotation = (axis == Axis.X ? Sylves.CubeRotation.ReflectX : axis == Axis.Y ? Sylves.CubeRotation.ReflectY : Sylves.CubeRotation.ReflectZ);
                var bound = (Sylves.CubeBound)grid.GetBound();
                symmetry = new Sylves.GridSymmetry
                {
                    Src = new Sylves.Cell(),
                    Dest = axis == Axis.X ? new Sylves.Cell(bound.size.x-1, 0,0) : axis == Axis.Y ? new Sylves.Cell(0, bound.size.y - 1, 0) : new Sylves.Cell(0,0,bound.size.z - 1),
                    Rotation = cellRotation,
                };
            }
            else if (cellType == SylvesExtensions.SquareCellType)
            {
                cellRotation = (axis == Axis.X ? Sylves.SquareRotation.ReflectX : Sylves.SquareRotation.ReflectY);
                var bound = (Sylves.SquareBound)grid.GetBound();
                symmetry = new Sylves.GridSymmetry
                {
                    Src = new Sylves.Cell(),
                    Dest = axis == Axis.X ? new Sylves.Cell(bound.size.x - 1, 0, 0) : new Sylves.Cell(0, bound.size.y - 1, 0),
                    Rotation = cellRotation,
                };
            }
            else if (cellType == SylvesExtensions.HexPrismCellType)
            {
                var hexRotation = (axis == Axis.X ? Sylves.HexRotation.PTReflectX : axis == Axis.Y ? throw new Exception("HexPrisms cannot be mirrored in vertical axis") : axis == Axis.Z ? Sylves.HexRotation.PTReflectX * Sylves.HexRotation.Rotate60(2) : Sylves.HexRotation.PTReflectX * Sylves.HexRotation.Rotate60(4));
                cellRotation = hexRotation;
                var bound = (Sylves.HexPrismBound)grid.GetBound();
                // Find the dead center of the bounds
                // Too lazy to figure out the correct way of doing this
                var size1 = bound.hexBound.max.y;
                var size2 = bound.hexBound.max.z;
                var corners = new[] { new Vector3Int(0, 0, 0), new Vector3Int(-size1 + 1, size1 - 1, 0), new Vector3Int(-size2 + 1, 0, size2 - 1), new Vector3Int(-size1 - size2 + 2, size1 - 1, size2 - 1) };
                var sum = corners.Aggregate((x, y) => x + y);

                // Work out where (0,0,) would map to under hex rotation, translating so that sum/4 is a fixed point.
                var m2 = Vector3Int.RoundToInt(0.25f * (Vector3)(sum - hexRotation.Multiply(sum)));
                // Convert from hex-prism to hex-prims bounds
                m2.z = 0;

                symmetry = new Sylves.GridSymmetry
                {
                    Src = new Sylves.Cell(),
                    Dest = new Sylves.Cell() + m2,
                    Rotation = cellRotation,
                };
            }
            else if (cellType == SylvesExtensions.TrianglePrismCellType)
            {
                // TODO
                cellRotation = Sylves.HexRotation.FTReflectX;
                throw new Exception("todo");
            }
            else
            {
                throw new Exception($"Unknown cellType {cellType.GetType()}");
            }

            yield return new InnerMirrorConstraint
            {
                grid = grid,
                symmetry = symmetry,
                cellType = generator.CellType,
                canonicalization = tileModelInfo.Canonicalization,
                rotation = cellRotation,
            };
        }

        private class InnerMirrorConstraint : SymmetryConstraint
        {
            public Sylves.IGrid grid;
            public Sylves.GridSymmetry symmetry;
            public Sylves.ICellType cellType;
            public Dictionary<Tile, Tile> canonicalization;
            public Sylves.CellRotation rotation;

            protected override bool TryMapIndex(TilePropagator propagator, int i, out int i2)
            {
                var cell = grid.GetCellByIndex(i);
                grid.TryApplySymmetry(symmetry, cell, out var d, out var _);
                //Debug.Log($"{cell}, {i}, {d} {grid.IsCellInGrid(d)}");
                if (grid.TryApplySymmetry(symmetry, cell, out var dest, out var _) && grid.IsCellInGrid(dest))
                {
                    i2 = grid.GetIndex(dest);
                    return true;
                }
                else
                {
                    i2 = 0;
                    return false;
                }
            }

            protected override bool TryMapTile(Tile tile, out Tile tile2)
            {
                var modelTile = (ModelTile)tile.Value;

                var newRotation = cellType.Multiply(rotation, modelTile.Rotation);
                var modelTile2 = new Tile(new ModelTile
                {
                    Tile = modelTile.Tile,
                    Rotation = newRotation,
                    Offset = modelTile.Offset,
                });
                return canonicalization.TryGetValue(modelTile2, out tile2);
            }
        }
    }
}
