using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// GameObjects with this behaviour record adjacency information for use with a <see cref="TesseraGenerator"/>.
    /// </summary>
    [AddComponentMenu("Tessera/Tessera Hex Tile")]
    public class TesseraHexTile : TesseraTileBase
    {
        public TesseraHexTile()
        {
            sylvesFaceDetails = new List<SylvesOrientedFace>
            {
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesHexPrismDir.Left, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesHexPrismDir.Right, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesHexPrismDir.Up, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesHexPrismDir.Down, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesHexPrismDir.ForwardLeft, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesHexPrismDir.ForwardRight, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesHexPrismDir.BackLeft, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)SylvesHexPrismDir.BackRight, new FaceDetails() ),
            };
        }

        public override Sylves.IGrid SylvesCellGrid => SylvesExtensions.HexPrismGridInstance;
        public override Sylves.ICellType SylvesCellType => SylvesExtensions.HexPrismCellType;

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