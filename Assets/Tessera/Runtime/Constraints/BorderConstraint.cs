using DeBroglie.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tessera
{

    [Serializable]
    public class BorderItem
    {
        public Sylves.CellDir cellDir;
        public TesseraTileBase tile;
    }

    /// <summary>
    /// Forces cells near the edge to be a particular tile.
    /// Compare with <see cref="Tessera.TesseraGenerator.skyBox"/>.
    /// > [!Note]
    /// > This class is available only in Tessera Pro
    /// </summary>
    [AddComponentMenu("Tessera/Border Constraint", 21)]
    [RequireComponent(typeof(TesseraGenerator))]
    public class BorderConstraint : TesseraConstraint
    {
        public BorderItem[] borders;

        private void OnValidate()
        {
            if(borders == null || borders.Length == 0)
            {
                var cellGrid = GetComponent<TesseraGenerator>().CellGrid;
                borders = cellGrid.GetCellType().GetCellDirs().Select(dir => new BorderItem
                {
                    cellDir = dir,
                    tile = null,
                }).ToArray();
            }
        }

        internal override IEnumerable<ITesseraInitialConstraint> GetInitialConstraints(Sylves.IGrid grid)
        {
            var cellType = grid.GetCellType();

            // TODO: Share this with other initial constraints, somehow?
            var constrained = new HashSet<Sylves.Cell>();

            foreach (var borderItem in borders)
            {
                if (borderItem.tile == null)
                    continue;
                var displayName = cellType.GetDisplayName(borderItem.cellDir);
                foreach (var cell in grid.GetCells())
                {
                    if (constrained.Contains(cell))
                        continue;
                    if (!grid.TryMove(cell, borderItem.cellDir, out var destCell, out var _, out var _))
                    {
                        constrained.Add(cell);
                        yield return new TesseraVolumeFilter
                        {
                            name = $"BorderConstraint {displayName} ({cell})",
                            cells = new List<Sylves.Cell> { cell },
                            tiles = new List<TesseraTileBase> { borderItem.tile },
                            volumeType = VolumeType.TilesetFilter,
                        };
                    }
                }
            }
        }
    }
}
