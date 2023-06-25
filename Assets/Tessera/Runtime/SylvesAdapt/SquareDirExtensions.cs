using System;
using UnityEngine;

namespace Tessera
{
    public static class SquareFaceExtensions
    {
        public static int GetSide(this Sylves.SquareDir dir)
        {
            switch (dir)
            {
                case Sylves.SquareDir.Right: return 0;
                case Sylves.SquareDir.Up: return 1;
                case Sylves.SquareDir.Left: return 2;
                case Sylves.SquareDir.Down: return 3;
            }
            throw new Exception($"{dir} is not a valid value for Sylves.SquareDir");
        }

        /// <returns>The normal vector for a given face.</returns>
        public static Vector3Int Forward(this Sylves.SquareDir dir)
        {
            switch (dir)
            {
                case Sylves.SquareDir.Left: return Vector3Int.left;
                case Sylves.SquareDir.Right: return Vector3Int.right;
                case Sylves.SquareDir.Up: return Vector3Int.up;
                case Sylves.SquareDir.Down: return Vector3Int.down;
            }
            throw new Exception($"{dir} is not a valid value for Sylves.SquareDir");
        }

        /// <returns>Returns the face dir with the opposite normal vector.</returns>
        public static Sylves.SquareDir Inverted(this Sylves.SquareDir dir)
        {

            switch (dir)
            {
                case Sylves.SquareDir.Left: return Sylves.SquareDir.Right;
                case Sylves.SquareDir.Right: return Sylves.SquareDir.Left;
                case Sylves.SquareDir.Up: return Sylves.SquareDir.Down;
                case Sylves.SquareDir.Down: return Sylves.SquareDir.Up;
            }
            throw new Exception($"{dir} is not a valid value for Sylves.SquareDir");
        }
    }
}