using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tessera
{
    /// <summary>
    /// Returned by TesseraGenerator after generation finishes
    /// </summary>
    public class TesseraCompletion
    {
        private IList<TesseraTileInstance> m_tileInstances;

        /// <summary>
        /// True if all tiles were successfully found.
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// The list of tiles to create.
        /// Big tiles will be listed a single time, and each <see cref="Tessera.TesseraTileInstance"/> has the world position set.
        /// </summary>
        public IList<TesseraTileInstance> tileInstances => m_tileInstances ?? (m_tileInstances = SylvesTesseraTilemapConversions.ToTileInstances(tileData, grid, gridTransform).ToList());

        /// <summary>
        /// The raw tile data, describing which tile is located in each cell.
        /// </summary>
        public IDictionary<Vector3Int, ModelTile> tileData { get; set; }

        /// <summary>
        /// The number of times the generation process was restarted.
        /// </summary>
        public int retries { get; set; }

        /// <summary>
        /// The number of times the generation process backtracked.
        /// </summary>
        public int backtrackCount { get; set; }

        /// <summary>
        /// If success is false, indicates where the generation failed.
        /// </summary>
        public Vector3Int? contradictionLocation { get; set; }

        /// <summary>
        /// Indicates these instances should be added to the previous set of instances.
        /// </summary>
        public bool isIncremental { get; set; }

        /// <summary>
        /// Describes the geometry and layout of the cells.
        /// See separate Sylves documentations for more details.
        /// https://www.boristhebrave.com/docs/sylves/1
        /// </summary>
        public Sylves.IGrid grid { get; set; }

        /// <summary>
        /// The position of the grid in world space. (Sylves grids always operate in local space).
        /// </summary>
        public TRS gridTransform { get; set; }

        /// <summary>
        /// Writes error information to Unity's log.
        /// </summary>
        public void LogErrror()
        {
            if (!success)
            {
                if (contradictionLocation != null)
                {
                    var loc = contradictionLocation;
                    Debug.LogError($"Failed to complete generation, issue at tile {loc}");
                }
                else
                {
                    Debug.LogError("Failed to complete generation");
                }
            }
        }
    }
}