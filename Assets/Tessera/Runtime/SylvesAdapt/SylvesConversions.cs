using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// Utilities for converting legacy Tessera enumerations to Sylves equivalents
    /// </summary>
    public static class SylvesConversions
    {
        private static BiMap<HexPrismFaceDir, SylvesHexPrismDir> hexDirMapping = new BiMap<HexPrismFaceDir, SylvesHexPrismDir>(new[]{
            (HexPrismFaceDir.Right,        SylvesHexPrismDir.Right),
            (HexPrismFaceDir.ForwardRight, SylvesHexPrismDir.ForwardRight),
            (HexPrismFaceDir.ForwardLeft,  SylvesHexPrismDir.ForwardLeft),
            (HexPrismFaceDir.Left,         SylvesHexPrismDir.Left),
            (HexPrismFaceDir.BackLeft,     SylvesHexPrismDir.BackLeft),
            (HexPrismFaceDir.BackRight,    SylvesHexPrismDir.BackRight),
            (HexPrismFaceDir.Up,           SylvesHexPrismDir.Up),
            (HexPrismFaceDir.Down,         SylvesHexPrismDir.Down),
        });


        private static BiMap<TrianglePrismFaceDir, SylvesTrianglePrismDir> trianglePrismDirMapping = new BiMap<TrianglePrismFaceDir, SylvesTrianglePrismDir>(new[]{
            (TrianglePrismFaceDir.BackRight,    SylvesTrianglePrismDir.BackRight),
            (TrianglePrismFaceDir.Back,         SylvesTrianglePrismDir.Back),
            (TrianglePrismFaceDir.BackLeft,     SylvesTrianglePrismDir.BackLeft),
            (TrianglePrismFaceDir.ForwardLeft,  SylvesTrianglePrismDir.ForwardLeft),
            (TrianglePrismFaceDir.Forward,      SylvesTrianglePrismDir.Forward),
            (TrianglePrismFaceDir.ForwardRight, SylvesTrianglePrismDir.ForwardRight),
            (TrianglePrismFaceDir.Up,           SylvesTrianglePrismDir.Up),
            (TrianglePrismFaceDir.Down,         SylvesTrianglePrismDir.Down),
        });

        
        private static BiMap<SquareFaceDir, Sylves.SquareDir> squareDirMapping = new BiMap<SquareFaceDir, Sylves.SquareDir>(new[]{
            (SquareFaceDir.Up, Sylves.SquareDir.Up),
            (SquareFaceDir.Down, Sylves.SquareDir.Down),
            (SquareFaceDir.Left, Sylves.SquareDir.Left),
            (SquareFaceDir.Right, Sylves.SquareDir.Right),
        });

        public static SylvesOrientedFace CubeOrientedFace(OrientedFace x)
        {
            x.faceDetails.faceType = FaceType.Square;
            return new SylvesOrientedFace
            {
                dir = (Sylves.CellDir)(int)x.faceDir,
                offset = x.offset,
                faceDetails = x.faceDetails,
            };
        }

        public static OrientedFace UndoCubeOrientedFace(SylvesOrientedFace x)
        {
            x.faceDetails.faceType = FaceType.Square;
            return new OrientedFace
            {
                faceDir= (CellFaceDir)(int)x.dir,
                offset = x.offset,
                faceDetails = x.faceDetails,
            };
        }

        public static Vector3Int CubeOffset(Vector3Int o) => o;
        public static Vector3Int UndoCubeOffset(Vector3Int o) => o;

        public static SylvesOrientedFace SquareOrientedFace(OrientedFace x)
        {
            x.faceDetails.faceType = FaceType.Edge;
            return new SylvesOrientedFace
            {
                dir = (Sylves.CellDir)squareDirMapping[(SquareFaceDir)x.faceDir],
                offset = x.offset,
                faceDetails = x.faceDetails,
            };
        }

        public static OrientedFace UndoSquareOrientedFace(SylvesOrientedFace x)
        {
            x.faceDetails.faceType = FaceType.Edge;
            return new OrientedFace
            {
                faceDir = (CellFaceDir)squareDirMapping[(Sylves.SquareDir)x.dir],
                offset = x.offset,
                faceDetails = x.faceDetails,
            };
        }

        public static Vector3Int SquareOffset(Vector3Int o) => o;
        public static Vector3Int UndoSquareOffset(Vector3Int o) => o;

        public static SylvesOrientedFace HexOrientedFace(OrientedFace x)
        {
            x.faceDetails.faceType = x.faceDir == (CellFaceDir)HexPrismFaceDir.Up || x.faceDir == (CellFaceDir)HexPrismFaceDir.Down ? FaceType.Hex : FaceType.Square;
            return new SylvesOrientedFace
            {
                dir = (Sylves.CellDir)hexDirMapping[(HexPrismFaceDir)x.faceDir],
                offset = HexOffset(x.offset),
                faceDetails = x.faceDetails,
            };
        }
        public static OrientedFace UndoHexOrientedFace(SylvesOrientedFace x)
        {
            var faceDir = (CellFaceDir)hexDirMapping[(SylvesHexPrismDir)x.dir];
            x.faceDetails.faceType = faceDir == (CellFaceDir)HexPrismFaceDir.Up || faceDir == (CellFaceDir)HexPrismFaceDir.Down ? FaceType.Hex : FaceType.Square;
            return new OrientedFace
            {
                faceDir = faceDir,
                offset = UndoHexOffset(x.offset),
                faceDetails = x.faceDetails,
            };
        }

        public static SylvesOrientedFace TrianglePrismOrientedFace(OrientedFace x)
        {
            x.faceDetails.faceType = x.faceDir == (CellFaceDir)TrianglePrismFaceDir.Up || x.faceDir == (CellFaceDir)TrianglePrismFaceDir.Down ? FaceType.Triangle : FaceType.Square;
            return new SylvesOrientedFace
            {
                dir = (Sylves.CellDir)trianglePrismDirMapping[(TrianglePrismFaceDir)x.faceDir],
                offset = TriangleOffset(x.offset),
                faceDetails = x.faceDetails,
            };
        }

        public static OrientedFace UndoTrianglePrismOrientedFace(SylvesOrientedFace x)
        {
            var faceDir = (CellFaceDir)trianglePrismDirMapping[(SylvesTrianglePrismDir)x.dir];
            x.faceDetails.faceType = faceDir == (CellFaceDir)TrianglePrismFaceDir.Up || faceDir == (CellFaceDir)TrianglePrismFaceDir.Down ? FaceType.Triangle : FaceType.Square;
            return new OrientedFace
            {
                faceDir = faceDir,
                offset = UndoTriangleOffset(x.offset),
                faceDetails = x.faceDetails,
            };
        }

        public static Vector3Int HexOffset(Vector3Int o)
        {
            return new Vector3Int(o.x, -o.z, o.y);
        }

        public static Vector3Int UndoHexOffset(Vector3Int o)
        {
            return new Vector3Int(o.x, o.z, -o.y);
        }

        public static Vector3Int TriangleOffset(Vector3Int o)
        {
            // Translation is handled in SylvesExtensions.GetCellCenter.
            return new Vector3Int(o.x - 2 * o.z, -o.z, o.y);
        }
        public static Vector3Int UndoTriangleOffset(Vector3Int o)
        {
            // Translation is handled in SylvesExtensions.GetCellCenter.
            return new Vector3Int(o.x + 2 * o.y, o.z, -o.y);
        }
    }
}
