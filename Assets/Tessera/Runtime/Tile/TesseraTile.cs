using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tessera
{

    /// <summary>
    /// GameObjects with this behaviour record adjacency information for use with a <see cref="TesseraGenerator"/>.
    /// </summary>
    [AddComponentMenu("Tessera/Tessera Tile")]
    public class TesseraTile : TesseraTileBase
    {

        public TesseraTile()
        {
            sylvesFaceDetails = new List<SylvesOrientedFace>
            {
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)Sylves.CubeDir.Left, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)Sylves.CubeDir.Right, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)Sylves.CubeDir.Up, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)Sylves.CubeDir.Down, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)Sylves.CubeDir.Forward, new FaceDetails() ),
                new SylvesOrientedFace(Vector3Int.zero, (Sylves.CellDir)Sylves.CubeDir.Back, new FaceDetails() ),
            };
        }

        public override Sylves.IGrid SylvesCellGrid => SylvesExtensions.CubeGridInstance;
        public override Sylves.ICellType SylvesCellType => SylvesExtensions.CubeCellType;

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