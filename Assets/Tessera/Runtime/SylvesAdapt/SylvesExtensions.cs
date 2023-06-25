using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tessera
{
    public static class SylvesExtensions
    {
        public static readonly Sylves.IGrid CubeGridInstance = new Sylves.TransformModifier(new Sylves.CubeGrid(1), Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, -0.5f)));
        public static readonly Sylves.ICellType CubeCellType = Sylves.CubeCellType.Instance;

        public static readonly Sylves.IGrid SquareGridInstance = new Sylves.TransformModifier(new Sylves.SquareGrid(1), Matrix4x4.Translate(new Vector3(-0.5f, -0.5f)));
        public static readonly Sylves.ICellType SquareCellType = Sylves.SquareCellType.Instance;

        // Similar sizing concerns to triangle grids (see below)
        public static readonly Sylves.IGrid HexPrismGridInstance = new Sylves.XZHexPrismGrid(1, 1);
        public static readonly Sylves.ICellType HexPrismCellType = Sylves.XZCellTypeModifier.Get(Sylves.HexPrismCellType.Get(Sylves.HexOrientation.PointyTopped));

        // This is size 1 to match Sylves conventions (e.g. deformations), called "cell" space.
        // Tessera triangle grids with cellSize.x=1 are actually larger (affects both "tile" space and "grid" space)
        // GetCellCenter/GetCellSizeTransform defined below includes a scaling factor to adjust for this
        public static Sylves.IGrid TrianglePrismGridInstance => new Sylves.TransformModifier(new Sylves.XZTrianglePrismGrid(1, 1), Matrix4x4.Translate(new Vector3(0.5f, 0, -Mathf.Sqrt(3) / 6)));
        public static readonly Sylves.ICellType TrianglePrismCellType = Sylves.XZCellTypeModifier.Get(Sylves.HexPrismCellType.Get(Sylves.HexOrientation.FlatTopped));


        #region CellRotation
        public static Sylves.CellRotation GetReflectX(this Sylves.ICellType cellType)
        {
            if(cellType is Sylves.CubeCellType)
            {
                return Sylves.CubeRotation.ReflectX;
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        private readonly static IDictionary<RotationGroupType, Sylves.CellRotation[]> CubeRotationsByGroupType = new Dictionary<RotationGroupType, Sylves.CellRotation[]>()
        {
            {
                RotationGroupType.None,
                new[]
                {
                    (Sylves.CellRotation)(Sylves.CubeRotation.Identity),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX),
                }
            },
            {
                RotationGroupType.XZ,
                new[]
                {
                    (Sylves.CellRotation)(Sylves.CubeRotation.Identity),
                    (Sylves.CellRotation)(Sylves.CubeRotation.RotateXZ),
                    (Sylves.CellRotation)(Sylves.CubeRotation.RotateXZ * Sylves.CubeRotation.RotateXZ),
                    (Sylves.CellRotation)(Sylves.CubeRotation.RotateXZ * Sylves.CubeRotation.RotateXZ * Sylves.CubeRotation.RotateXZ),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.Identity),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.RotateXZ),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.RotateXZ * Sylves.CubeRotation.RotateXZ),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.RotateXZ * Sylves.CubeRotation.RotateXZ * Sylves.CubeRotation.RotateXZ),
                }
            },
            {
                RotationGroupType.XY,
                new[]
                {
                    (Sylves.CellRotation)(Sylves.CubeRotation.Identity),
                    (Sylves.CellRotation)(Sylves.CubeRotation.RotateXY),
                    (Sylves.CellRotation)(Sylves.CubeRotation.RotateXY * Sylves.CubeRotation.RotateXY),
                    (Sylves.CellRotation)(Sylves.CubeRotation.RotateXY * Sylves.CubeRotation.RotateXY * Sylves.CubeRotation.RotateXY),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.Identity),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.RotateXY),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.RotateXY * Sylves.CubeRotation.RotateXY),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.RotateXY * Sylves.CubeRotation.RotateXY * Sylves.CubeRotation.RotateXY),
                }
            },
            {
                RotationGroupType.YZ,
                new[]
                {
                    (Sylves.CellRotation)(Sylves.CubeRotation.Identity),
                    (Sylves.CellRotation)(Sylves.CubeRotation.RotateYZ),
                    (Sylves.CellRotation)(Sylves.CubeRotation.RotateYZ * Sylves.CubeRotation.RotateYZ),
                    (Sylves.CellRotation)(Sylves.CubeRotation.RotateYZ * Sylves.CubeRotation.RotateYZ * Sylves.CubeRotation.RotateYZ),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.Identity),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.RotateYZ),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.RotateYZ * Sylves.CubeRotation.RotateYZ),
                    (Sylves.CellRotation)(Sylves.CubeRotation.ReflectX * Sylves.CubeRotation.RotateYZ * Sylves.CubeRotation.RotateYZ * Sylves.CubeRotation.RotateYZ),
                }
            },
            {
                RotationGroupType.All,
                Sylves.CubeRotation.GetRotations(true).Select(x => (Sylves.CellRotation)x).ToArray()
            },
        };

        public static IList<Sylves.CellRotation> GetRotations(this Sylves.ICellType cellType, bool rotatable = true, bool reflectable = true, RotationGroupType rotationGroupType = RotationGroupType.All)
        {
            if (rotatable && cellType is Sylves.CubeCellType)
            {
                var rotations = CubeRotationsByGroupType[rotationGroupType];
                if (reflectable)
                {
                    return rotations;
                }
                else
                {
                    return new ArraySegment<Sylves.CellRotation>(rotations, 0, rotations.Length / 2);
                }
            }
            else
            {
                if (rotatable)
                {
                    return cellType.GetRotations(reflectable);
                }
                else if (reflectable)
                {
                    return new[] { cellType.GetIdentity(), cellType.GetReflectX() };
                }
                else
                {
                    return new[] { cellType.GetIdentity() };
                }
            }
        }

        public static (Sylves.CellDir, FaceDetails) RotateBy(this Sylves.ICellType cellType, Sylves.CellDir dir, FaceDetails faceDetails, Sylves.CellRotation rot)
        {
            cellType.Rotate(dir, rot, out var resultDir, out var connection);
            faceDetails = faceDetails.Clone();

            if(connection.Sides == 0)
            {
                if (connection.Mirror)
                {
                    faceDetails.ReflectX();
                }
            }
            else if(connection.Sides == 4)
            {
                if (connection.Mirror)
                {
                    faceDetails.ReflectY();
                }
                for (var i = 0; i < connection.Rotation; i++)
                {
                    faceDetails.RotateCw();
                }
            }
            else if(connection.Sides == 6)
            {
                if (faceDetails.faceType == FaceType.Triangle)
                {
                    if (connection.Mirror)
                    {
                        faceDetails.TriangleReflectX();
                    }
                    for (var i = 0; i < connection.Rotation; i++)
                    {
                        faceDetails.TriangleRotateCcw60();
                    }
                }
                else
                {
                    if (connection.Mirror)
                    {
                        faceDetails.HexReflectY();
                    }
                    for (var i = 0; i < connection.Rotation; i++)
                    {
                        faceDetails.HexRotateCcw();
                    }
                }
            }
            else
            {
                throw new Exception($"Unexpected number of sides");
            }
            return (resultDir, faceDetails);
        }

        #endregion

        #region Grid Methods
        public static Sylves.ICellType GetCellType(this Sylves.IGrid grid) => grid.GetCellTypes().Single();

        public static bool FindCell(
            this Sylves.IGrid grid,
            Vector3 tileCenter,
            Matrix4x4 tileLocalToGridMatrix,
            out Sylves.Cell cell,
            out Sylves.CellRotation rotation)
        {
            return grid.FindCell(tileLocalToGridMatrix * Matrix4x4.Translate(tileCenter), out cell, out rotation);
        }

        // Standin for the old ICellType.GetCellCenter
        // This method exists because we use a one size fits all cell grid for each celltype to save on allocations
        // Not convinced that's a great idea.
        public static Vector3 GetCellCenter(this Sylves.IGrid sylvesCellGrid, Vector3Int offset, Vector3 center, Vector3 cellSize)
        {
            // Should match
            // GetCellSizeTransform(...) * grid.GetCellCenter(offset)

            var unwrapped = sylvesCellGrid.Unwrapped;
            if (unwrapped is Sylves.CubeGrid)
            {
                return center + Vector3.Scale(cellSize, sylvesCellGrid.GetCellCenter((Sylves.Cell)offset));
            }
            if (unwrapped is Sylves.SquareGrid)
            {
                return center + Vector3.Scale(new Vector3(cellSize.x, cellSize.y, 1), sylvesCellGrid.GetCellCenter((Sylves.Cell)offset));
            }
            else if (unwrapped is Sylves.HexPrismGrid)
            {
                var s = cellSize.x * Mathf.Sqrt(3) * 2 / 3;
                return center + Vector3.Scale(new Vector3(s, cellSize.y, s), sylvesCellGrid.GetCellCenter((Sylves.Cell)offset));
            }
            else if(unwrapped is Sylves.TrianglePrismGrid)
            {
                var s = cellSize.x * Mathf.Sqrt(3);
                return center + Vector3.Scale(new Vector3(s, cellSize.y, s), sylvesCellGrid.GetCellCenter((Sylves.Cell)offset));
            }
            else
            {
                throw new Exception($"Unknown grid type {unwrapped.GetType()}");
            }
        }

        /// <summary>
        /// Returns a transform from the canonical cell (of unit size) to a specific cell size and center.
        /// </summary>
        public static Matrix4x4 GetCellSizeTransform(this Sylves.IGrid sylvesCellGrid, Vector3 center, Vector3 cellSize)
        {
            var unwrapped = sylvesCellGrid.Unwrapped;
            if (unwrapped is Sylves.CubeGrid)
            {
                return Matrix4x4.Translate(center) * Matrix4x4.Scale(cellSize);
            }
            else if (unwrapped is Sylves.SquareGrid)
            {
                return Matrix4x4.Translate(center) * Matrix4x4.Scale(new Vector3(cellSize.x, cellSize.y, 1));
            }
            else if (unwrapped is Sylves.HexPrismGrid)
            {
                // See note on TrianglePrismGridInstance re this scaling
                var s = cellSize.x * Mathf.Sqrt(3) * 2 / 3;
                return Matrix4x4.Translate(center) * Matrix4x4.Scale(new Vector3(s, cellSize.y, s));

            }
            else if (unwrapped is Sylves.TrianglePrismGrid)
            {
                // See note on TrianglePrismGridInstance re this scaling
                var s = cellSize.x * Mathf.Sqrt(3);
                return Matrix4x4.Translate(center) * Matrix4x4.Scale(new Vector3(s, cellSize.y, s));
            }
            else
            {
                throw new Exception($"Unknown grid type {unwrapped.GetType()}");
            }
        }

        #endregion

        #region CellType methods
        public static string GetDisplayName(this Sylves.ICellType cellType, Sylves.CellDir cellDir)
        {
            if(cellType is Sylves.CubeCellType)
            {
                return ((Sylves.CubeDir)cellDir).ToString();
            }
            if (cellType is Sylves.HexCellType ht)
            {
                if (ht.Orientation == Sylves.HexOrientation.PointyTopped)
                {
                    return ((SylvesHexPrismDir)cellDir).ToString();
                }
                else
                {
                    return ((SylvesTrianglePrismDir)cellDir).ToString();
                }
            }
            return cellDir.ToString();
        }

        public static IEnumerable<(Sylves.CellDir, Sylves.CellDir)> GetDirPairs(this Sylves.ICellType cellType)
        {
            foreach (var dir in cellType.GetCellDirs())
            {
                var i = cellType.Invert(dir);
                if (i == null) continue;
                if ((int)dir > (int)i.Value) continue;
                yield return (dir, i.Value);
            }
        }
#endregion
    }
}
