using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// > [!Note]
    /// > This class is available only in Tessera Pro
    /// </summary>
    public static class HexPrismDirExtensions
    {
        public static int GetSide(this SylvesHexPrismDir dir)
        {
            switch (dir)
            {
                case SylvesHexPrismDir.Right: return 0;
                case SylvesHexPrismDir.ForwardRight: return 1;
                case SylvesHexPrismDir.ForwardLeft: return 2;
                case SylvesHexPrismDir.Left: return 3;
                case SylvesHexPrismDir.BackLeft: return 4;
                case SylvesHexPrismDir.BackRight: return 5;
            }
            throw new Exception();
        }

        public static bool IsUpDown(this SylvesHexPrismDir dir)
        {
            switch (dir)
            {
                case SylvesHexPrismDir.Up:
                case SylvesHexPrismDir.Down:
                    return true;
                default:
                    return false;
            }
            throw new Exception();
        }

        public static Vector3 Up(this SylvesHexPrismDir dir)
        {
            switch (dir)
            {
                case SylvesHexPrismDir.Up:
                case SylvesHexPrismDir.Down:
                    return Vector3.forward;
                default:
                    return Vector3.up;
            }
        }

        public static Vector3 Forward(this SylvesHexPrismDir dir)
        {
            switch (dir)
            {

                case SylvesHexPrismDir.Up:
                    return Vector3.up;
                case SylvesHexPrismDir.Down:
                    return Vector3.down;
                default:
                    return Quaternion.Euler(0, -60 * dir.GetSide(), 0) * Vector3.right;
            }
        }
    }
}
