using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tessera
{

    /// <summary>
    /// GameObjects with this behaviour record adjacency information for use with a <see cref="TesseraGenerator"/>.
    /// </summary>
    [AddComponentMenu("Tessera/Tessera Square Tile")]
    public class TesseraSquareTile : TesseraTileBase
    {

        public TesseraSquareTile()
        {
            sylvesFaceDetails = new List<SylvesOrientedFace>
            {
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)Sylves.SquareDir.Left, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)Sylves.SquareDir.Right, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)Sylves.SquareDir.Up, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)Sylves.SquareDir.Down, new FaceDetails() ),
            };
            rotationGroupType = RotationGroupType.XY;
        }

        public override Sylves.IGrid SylvesCellGrid => SylvesExtensions.SquareGridInstance;
        public override Sylves.ICellType SylvesCellType => SylvesExtensions.SquareCellType;

        public BoundsInt GetBounds()
        {
            var min = sylvesOffsets[0];
            var max = min;
            foreach (var o in sylvesOffsets)
            {
                min = Vector3Int.Min(min, o);
                max = Vector3Int.Max(max, o);
            }

            return new BoundsInt(min, max - min);
        }
    }
}