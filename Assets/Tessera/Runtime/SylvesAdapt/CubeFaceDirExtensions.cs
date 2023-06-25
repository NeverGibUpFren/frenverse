using System;
using UnityEngine;

namespace Tessera
{
    public static class CubeDirExtensions
    {
        /// <returns>Returns (0, 1, 0) vector for most faces, and returns (0, 0, 1) for the top/bottom faces.</returns>
        public static Vector3Int Up(this Sylves.CubeDir dir)
        {
            switch (dir)
            {
                case Sylves.CubeDir.Left:
                case Sylves.CubeDir.Right:
                case Sylves.CubeDir.Forward:
                case Sylves.CubeDir.Back:
                    return Vector3Int.up;
                case Sylves.CubeDir.Up:
                case Sylves.CubeDir.Down:
                    return new Vector3Int(0, 0, 1);
            }
            throw new Exception();
        }

        /// <returns>The normal vector for a given face.</returns>
        public static Vector3Int Forward(this Sylves.CubeDir dir)
        {
            switch (dir)
            {
                case Sylves.CubeDir.Left: return Vector3Int.left;
                case Sylves.CubeDir.Right: return Vector3Int.right;
                case Sylves.CubeDir.Up: return Vector3Int.up;
                case Sylves.CubeDir.Down: return Vector3Int.down;
                case Sylves.CubeDir.Forward: return new Vector3Int(0, 0, 1);
                case Sylves.CubeDir.Back: return new Vector3Int(0, 0, -1);
            }
            throw new Exception();
        }

        /// <returns>Returns the face dir with the opposite normal vector.</returns>
        public static Sylves.CubeDir Inverted(this Sylves.CubeDir dir)
        {

            switch (dir)
            {
                case Sylves.CubeDir.Left: return Sylves.CubeDir.Right;
                case Sylves.CubeDir.Right: return Sylves.CubeDir.Left;
                case Sylves.CubeDir.Up: return Sylves.CubeDir.Down;
                case Sylves.CubeDir.Down: return Sylves.CubeDir.Up;
                case Sylves.CubeDir.Forward: return Sylves.CubeDir.Back;
                case Sylves.CubeDir.Back: return Sylves.CubeDir.Forward;
            }
            throw new Exception();
        }
    }
}