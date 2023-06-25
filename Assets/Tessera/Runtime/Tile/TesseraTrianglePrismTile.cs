using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// GameObjects with this behaviour record adjacency information for use with a <see cref="TesseraGenerator"/>.
    /// </summary>
    [AddComponentMenu("Tessera/Tessera Triangle Prism Tile")]
    public class TesseraTrianglePrismTile : TesseraTileBase
    {
        public TesseraTrianglePrismTile()
        {
            sylvesFaceDetails = new List<SylvesOrientedFace>
            {
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesTrianglePrismDir.Back, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesTrianglePrismDir.Up, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesTrianglePrismDir.Down, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesTrianglePrismDir.ForwardLeft, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesTrianglePrismDir.ForwardRight, new FaceDetails() ),
            };
        }

        public override Sylves.IGrid SylvesCellGrid => SylvesExtensions.TrianglePrismGridInstance;
        public override Sylves.ICellType SylvesCellType => SylvesExtensions.TrianglePrismCellType;

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