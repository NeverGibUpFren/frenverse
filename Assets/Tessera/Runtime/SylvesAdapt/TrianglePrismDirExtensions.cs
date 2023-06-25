using System;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// > [!Note]
    /// > This class is available only in Tessera Pro
    /// </summary>
    public static class TrianglePrismDirExtensions
    {
        public static int GetSide(this SylvesTrianglePrismDir dir)
        {
            switch (dir)
            {
                case SylvesTrianglePrismDir.Back: return 0;
                case SylvesTrianglePrismDir.BackRight: return 1;
                case SylvesTrianglePrismDir.ForwardRight: return 2;
                case SylvesTrianglePrismDir.Forward: return 3;
                case SylvesTrianglePrismDir.ForwardLeft: return 4;
                case SylvesTrianglePrismDir.BackLeft: return 5;
                
            }
            throw new Exception();
        }

        /*
        public static Vector3Int OffsetDelta(this SylvesTrianglePrismDir dir)
        {
            switch (dir)
            {
                case SylvesTrianglePrismDir.Forward: return new Vector3Int(-1, 0, 1);
                case SylvesTrianglePrismDir.Back: return new Vector3Int(1, 0, -1);
                case SylvesTrianglePrismDir.Up: return new Vector3Int(0, 1, 0);
                case SylvesTrianglePrismDir.Down: return new Vector3Int(0, -1, 0);
                case SylvesTrianglePrismDir.ForwardLeft: return new Vector3Int(-1, 0, 0);
                case SylvesTrianglePrismDir.BackRight: return new Vector3Int(1, 0, 0);
                case SylvesTrianglePrismDir.ForwardRight: return new Vector3Int(1, 0, 0);
                case SylvesTrianglePrismDir.BackLeft: return new Vector3Int(-1, 0, 0);
                default:
                    return Vector3Int.zero;
            }
        }
        */



        public static Vector3Int SylvesOffsetDelta(this SylvesTrianglePrismDir dir)
        {
			switch (dir)
            {
                case SylvesTrianglePrismDir.Forward: return new Vector3Int(1, -1, 0);
                case SylvesTrianglePrismDir.Back: return new Vector3Int(-1, 1, 0);
                case SylvesTrianglePrismDir.Up: return new Vector3Int(0, 0, 1);
                case SylvesTrianglePrismDir.Down: return new Vector3Int(0, 0, -1);
                case SylvesTrianglePrismDir.ForwardLeft: return new Vector3Int(-1, 0, 0);
                case SylvesTrianglePrismDir.BackRight: return new Vector3Int(1, 0, 0);
                case SylvesTrianglePrismDir.ForwardRight: return new Vector3Int(1, 0, 0);
                case SylvesTrianglePrismDir.BackLeft: return new Vector3Int(-1, 0, 0);
                default:
                    return Vector3Int.zero;
            }
        }

        public static bool IsValid(this SylvesTrianglePrismDir dir, Vector3Int offset)
        {
            return dir.IsValid(TrianglePrismGeometryUtils.PointsUp(offset));
        }


        public static bool IsValid(this SylvesTrianglePrismDir dir, bool pointsUp)
        {
            switch (dir)
            {
                case SylvesTrianglePrismDir.Forward: return !pointsUp;
                case SylvesTrianglePrismDir.Back: return pointsUp;
                case SylvesTrianglePrismDir.Up: return true;
                case SylvesTrianglePrismDir.Down: return true;
                case SylvesTrianglePrismDir.ForwardLeft: return pointsUp;
                case SylvesTrianglePrismDir.BackRight: return !pointsUp;
                case SylvesTrianglePrismDir.ForwardRight: return pointsUp;
                case SylvesTrianglePrismDir.BackLeft: return !pointsUp;
                default:
                    throw new Exception();
            }
        }

        public static bool IsUpDown(this SylvesTrianglePrismDir dir)
        {
            switch (dir)
            {
                case SylvesTrianglePrismDir.Up:
                case SylvesTrianglePrismDir.Down:
                    return true;
                default:
                    return false;
            }
            throw new Exception();
        }


        public static Vector3 Up(this SylvesTrianglePrismDir dir)
        {
            switch (dir)
            {
                case SylvesTrianglePrismDir.Up:
                case SylvesTrianglePrismDir.Down:
                    return Vector3.forward;
                default:
                    return Vector3.up;
            }
        }

        public static Vector3 Forward(this SylvesTrianglePrismDir dir)
        {
            switch (dir)
            {
                case SylvesTrianglePrismDir.Up:
                    return Vector3.up;
                case SylvesTrianglePrismDir.Down:
                    return Vector3.down;
                case SylvesTrianglePrismDir.Forward:
                    return Quaternion.Euler(0, -60 * 0, 0) * Vector3.forward;
                case SylvesTrianglePrismDir.ForwardLeft:
                    return Quaternion.Euler(0, -60 * 1, 0) * Vector3.forward;
                case SylvesTrianglePrismDir.BackLeft:
                    return Quaternion.Euler(0, -60 * 2, 0) * Vector3.forward;
                case SylvesTrianglePrismDir.Back:
                    return Quaternion.Euler(0, -60 * 3, 0) * Vector3.forward;
                case SylvesTrianglePrismDir.BackRight:
                    return Quaternion.Euler(0, -60 * 4, 0) * Vector3.forward;
                case SylvesTrianglePrismDir.ForwardRight:
                    return Quaternion.Euler(0, -60 * 5, 0) * Vector3.forward;
            }
            throw new Exception();
        }
    }
}
