using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// If this is set on the "uncertainty tile" used by TesseraGenerator/AnimatedGenerator,
    /// it will be populated with data about which tiles are actually possible.
    /// </summary>
    public class TesseraUncertainty : MonoBehaviour
    {
        public ISet<ModelTile> modelTiles { get; internal set; }
    }
}
