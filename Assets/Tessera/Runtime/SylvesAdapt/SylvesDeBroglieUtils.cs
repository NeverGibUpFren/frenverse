using DeBroglie;
using DeBroglie.Models;
using DeBroglie.Topo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using static Tessera.SylvesExtensions;

namespace Tessera
{
    // Utilities for converting from IGrid to ITopology, and similar mappings
    internal static class SylvesDeBroglieUtils
    {
        #region Topology Conversion

        private const bool ForceGenericTopology = false;

        // NB: For convenience, the topology indices always match the grid indices
        public static ITopology GetTopology(Sylves.IGrid grid, Sylves.IBound sizeOverride = null)
        {
            grid = grid.Unwrapped;
            if (ForceGenericTopology || grid is Sylves.MeshGrid || grid is Sylves.HexPrismGrid || grid is Sylves.TrianglePrismGrid)
            {
                if (sizeOverride != null)
                {
                    throw new Exception("");
                }
                return new GenericTopology(grid);
            }
            if (grid is Sylves.CubeGrid cg)
            {
                var size = ((Sylves.CubeBound)(sizeOverride ?? grid.GetBound())).size;
                return new GridTopology(size.x, size.y, size.z, false);
            }
            if (grid is Sylves.SquareGrid sg)
            {
                var size = ((Sylves.SquareBound)(sizeOverride ?? grid.GetBound())).size;
                return new GridTopology(size.x, size.y, false);
            }
            throw new Exception($"Unsupported Grid type {grid.GetType()}");
        }

        public static TileModel GetTileModel(Sylves.IGrid grid, ModelType modelType, TileModelInfo tileModelInfo, List<TesseraTilemap> samples, Vector3Int overlapSize, TesseraPalette palette)
        {
            if(modelType == ModelType.AdjacentPaint)
            {
                return GetAdjacentPaintTileModel(grid, tileModelInfo, palette);
            }
            else if (modelType == ModelType.Overlapping)
            {
                return GetOverlappedTileModel(grid, tileModelInfo, samples, overlapSize, palette);
            }
            else if ( modelType == ModelType.Adjacent)
            {
                return GetAdjacentTileModel(grid, tileModelInfo, samples, palette);
            }
            else
            {
                throw new Exception($"Unkown model type {modelType}");
            }
        }

        #region SampleBased
        private static TesseraTilemap ApplySymmetry(TesseraTilemap sample, Sylves.GridSymmetry gridSymmetry, TileModelInfo tileModelInfo)
        {
            var sampleData = new Dictionary<Vector3Int, ModelTile>();
            var grid = sample.Grid;
            
            foreach (var kv in sample.Data)
            {
                var cell = kv.Key;
                var modelTile = kv.Value;
                if (!grid.Unbounded.TryApplySymmetry(gridSymmetry, (Sylves.Cell)cell, out var destCell, out var cellRotation))
                {
                    //Debug.LogWarning($"Couldn't rotate {cell} by {gridSymmetry.rotation}");
                    continue;
                }
                // Canonical rotation
                var modelTileRotated = new ModelTile
                {
                    Tile = modelTile.Tile,
                    Offset = modelTile.Offset,
                    Rotation = grid.GetCellType().Multiply(cellRotation, modelTile.Rotation),
                };
                if (!tileModelInfo.Canonicalization.TryGetValue(new Tile(modelTileRotated), out var canonicalTile))
                {
                    //Debug.LogWarning($"Couldn't find canonical tile for {cell} rotation {mt.Rotation}");
                    continue;
                }

                sampleData[(Vector3Int)destCell] = (ModelTile)canonicalTile.Value;
            }

            if (sampleData.Count == 0)
                return null;

            if (!grid.Unbounded.TryApplySymmetry(gridSymmetry, sample.Grid.GetBound(), out var newBound))
            {
                return null;
            }

            return new TesseraTilemap
            {
                Grid = grid.Unbounded.BoundBy(newBound),
                Data = sampleData,
            };
        }

        private static ITopoArray<Tile> TilemapToTopoArray(TesseraTilemap sample)
        {
            var sampleTopology = GetTopology(sample.Grid);
            var sampleData = new Tile[sampleTopology.IndexCount];
            foreach (var kv in sample.Data)
            {
                var cell = kv.Key;
                // Insert into sample
                var index = sampleTopology.GetIndex(cell.x, cell.y, cell.z);
                if (index < 0 || index >= sampleData.Length)
                {
                    throw new Exception();
                }
                sampleData[index] = new Tile(kv.Value);
            }
            var mask = sampleTopology.GetIndices().Select(i => sampleData[i] != new Tile(null)).ToArray();
            var sampleArray = TopoArray.Create(sampleData, sampleTopology.WithMask(mask));
            return sampleArray;
        }

        private static IEnumerable<Sylves.GridSymmetry> GetBoundsSymmetries(Sylves.IGrid grid)
        {
            var cellType = grid.GetCellType();
            var bound = grid.GetBound();

            var unwrapped = grid.Unwrapped;
            if(!(unwrapped is Sylves.CubeGrid) && !(unwrapped is Sylves.SquareGrid))
            {
                // This shouldn't be called as other grids use GenericTopology
                throw new Exception();
            }

            foreach (var rotation in cellType.GetRotations(true))
            {
                var untranslatedSymmetry = new Sylves.GridSymmetry
                {
                    Rotation = rotation,
                    Src = new Sylves.Cell(),
                    Dest = new Sylves.Cell(),
                };

                if (!grid.Unbounded.TryApplySymmetry(untranslatedSymmetry, bound, out var destBound))
                    continue;
                if (destBound is Sylves.CubeBound cb)
                {
                    // Translate so that all cells are non-negative,a s needed by DeBroglie
                    yield return new Sylves.GridSymmetry
                    {
                        Rotation = rotation,
                        Src = new Sylves.Cell(),
                        Dest = (Sylves.Cell)(-cb.min),
                    };
                } else if (destBound is Sylves.SquareBound sb)
                {
                    // Translate so that all cells are non-negative,a s needed by DeBroglie
                    yield return new Sylves.GridSymmetry
                    {
                        Rotation = rotation,
                        Src = new Sylves.Cell(),
                        Dest = new Sylves.Cell(-sb.min.x, -sb.min.y),
                    };
                }
            }
        }

        private static IList<ITopoArray<Tile>> SampleToTopoArrays(Sylves.IGrid grid, TileModelInfo tileModelInfo, TesseraTilemap sample)
        {
            var results = new List<ITopoArray<Tile>>();

            // TODO: Validate sample topology is compatible

            // TODO: Be smarter about which symmetries the tiles actually support
            // Currently this is very naive, and just generates lots of empty samples.
            foreach (var gridSymmetry in GetBoundsSymmetries(sample.Grid))
            {
                var rotatedSample = ApplySymmetry(sample, gridSymmetry, tileModelInfo);
                if (rotatedSample == null)
                    continue;
                var sampleArray = TilemapToTopoArray(rotatedSample);
                results.Add(sampleArray);
            }
            return results;
        }

        public static TileModel GetOverlappedTileModel(Sylves.IGrid grid, TileModelInfo tileModelInfo, List<TesseraTilemap> samples, Vector3Int overlapSize, TesseraPalette palette)
        {
            var unwrapped = grid.Unwrapped;
            if (unwrapped is Sylves.CubeGrid || unwrapped is Sylves.SquareGrid)
            {
                // Hack to make working with flat grids easier

                if (unwrapped is Sylves.CubeGrid cg)
                {
                    overlapSize = samples.Aggregate(overlapSize, (s, sample) => Vector3Int.Min(s, ((Sylves.CubeBound)sample.Grid.GetBound()).size));
                }
                else if (unwrapped is Sylves.SquareGrid sg)
                {
                    var overlapSize2d = samples.Aggregate(new Vector2Int(overlapSize.x, overlapSize.y), (s, sample) => Vector2Int.Min(s, ((Sylves.SquareBound)sample.Grid.GetBound()).size));
                    overlapSize = new Vector3Int(overlapSize2d.x, overlapSize2d.y, 1);
                }
                else
                {
                    throw new Exception();
                }

                var model = new OverlappingModel(overlapSize.x, overlapSize.y, overlapSize.z);

                foreach (var sample in samples)
                {
                    foreach (var topoArray in SampleToTopoArrays(grid, tileModelInfo, sample))
                    {
                        var topology = topoArray.Topology;
                        if (topology.Width < overlapSize.x || topology.Height < overlapSize.y || topology.Depth < overlapSize.z)
                            continue;
                        model.AddSample(topoArray);

                    }
                }
                return model;
            }
            else
            {
                throw new Exception("Overlapping model only supports cube grid");
            }
        }


        public static TileModel GetAdjacentTileModel(Sylves.IGrid grid, TileModelInfo tileModelInfo, List<TesseraTilemap> samples, TesseraPalette palette)
        {
            var unwrapped = grid.Unwrapped;
            if (unwrapped is Sylves.CubeGrid || unwrapped is Sylves.SquareGrid)
            {
                var model = new AdjacentModel();

                foreach (var sample in samples)
                {
                    foreach (var topoArray in SampleToTopoArrays(grid, tileModelInfo, sample))
                    {
                        var topology = topoArray.Topology;
                        model.AddSample(topoArray);

                    }
                }
                return model;
            }
            else
            {
                throw new Exception("Adjacent model only supports cube grid");
            }
        }


        #endregion

        #region AdjacentPaint
        // This is tightly couped with GetTopology, should they be merged?
        public static TileModel GetAdjacentPaintTileModel(Sylves.IGrid grid, TileModelInfo tileModelInfo, TesseraPalette palette)
        {
            var cellType = grid.GetCellType();
            var unwrappedGrid = grid.Unwrapped;
            if (ForceGenericTopology || unwrappedGrid is Sylves.MeshGrid || unwrappedGrid is Sylves.HexPrismGrid || unwrappedGrid is Sylves.TrianglePrismGrid)
            {
                var directionMapping = GetDirectionMapping(unwrappedGrid);
                var edgeLabelMapping = GetEdgeLabelMapping(grid, directionMapping);
                var info = new GraphInfo
                {
                    DirectionsCount = directionMapping.Count,
                    EdgeLabelCount = edgeLabelMapping.Count,
                    EdgeLabelInfo = edgeLabelMapping.OrderBy(t=>t.Item2).Select(t =>
                    {
                        var ((direction, inverseDirection, cellRotation), edgeLabel) = t;
                        var rotation = new DeBroglie.Rot.Rotation();// Unused by Tessera
                        return (direction, inverseDirection, rotation);
                    })
                    .ToArray(),
                };

                var model = new GraphAdjacentModel(info);

                foreach (var (tile, frequency) in tileModelInfo.AllTiles)
                {
                    model.SetFrequency(tile, frequency);
                }

                var allTiles = new HashSet<Tile>(tileModelInfo.AllTiles.Select(x => x.Item1));

                foreach (var ia in tileModelInfo.InternalAdjacencies)
                {
                    var d = tileModelInfo.SylvesDirectionMapping[ia.SylvesGridDir];
                    foreach (var el in Enumerable.Range(0, info.EdgeLabelCount))
                    {
                        // Is there a way to make this internal direction work with this internal adjacency,
                        // by only rotating Tile2?
                        // (rotating tile1 would involve double counting, as InternalAdjacnecies already includes multiple rotations)
                        var (direction, inverseDirection, connection) = edgeLabelMapping[(EdgeLabel)el];
                        if (direction != d)
                            continue;
                        if (!cellType.TryGetRotation(ia.SylvesInverseGridDir, tileModelInfo.SylvesDirectionMapping[inverseDirection], connection, out var cellRotation))
                            continue;
                        var mt = (ModelTile)ia.Src.Value;
                        var mt2 = (ModelTile)ia.Dest.Value;
                        var otherTile = new Tile(new ModelTile
                        {
                            Tile = mt2.Tile,
                            Offset = mt2.Offset,
                            Rotation = cellType.Multiply(cellRotation, mt2.Rotation),
                        });
                        // TODO: How come this doesn't canonicalize?
                        if (!allTiles.Contains(otherTile))
                            continue;

                        model.AddAdjacency(ia.Src, otherTile, (EdgeLabel)el);
                    }
                }

                for (var el = 0; el < info.EdgeLabelCount; el++)
                {
                    var elInfo = info.EdgeLabelInfo[el];
                    var asdf = edgeLabelMapping[(EdgeLabel)el];
                    var cellDir1 = tileModelInfo.SylvesDirectionMapping[elInfo.Item1];
                    var cellDir2 = tileModelInfo.SylvesDirectionMapping[elInfo.Item2];
                    var tiles1 = tileModelInfo.SylvesTilesByDirection[cellDir1];
                    var tiles2 = tileModelInfo.SylvesTilesByDirection[cellDir2];
                    var adjacencies = GetAdjacencies(palette, el, tiles1, tiles2);
                    foreach (var (t1, t2, _) in adjacencies)
                    {
                        model.AddAdjacency(t1, t2, (EdgeLabel)el);
                    }
                }
                return model;
            }
            else if(unwrappedGrid is Sylves.CubeGrid || unwrappedGrid is Sylves.SquareGrid)
            {
                var model = cellType == Sylves.SquareCellType.Instance ? new AdjacentModel(DirectionSet.Cartesian2d) : new AdjacentModel(DirectionSet.Cartesian3d);

                foreach (var (tile, frequency) in tileModelInfo.AllTiles)
                {
                    model.SetFrequency(tile, frequency);
                }

                foreach (var ia in tileModelInfo.InternalAdjacencies)
                {
                    var d = tileModelInfo.SylvesDirectionMapping[ia.SylvesGridDir];
                    model.AddAdjacency(ia.Src, ia.Dest, d);
                }

                var adjacencies = cellType.GetDirPairs().SelectMany(t => {
                    return GetAdjacencies(palette, tileModelInfo.SylvesDirectionMapping[t.Item1], tileModelInfo.SylvesTilesByDirection[t.Item1], tileModelInfo.SylvesTilesByDirection[t.Item2]);
                }).ToList();

                foreach (var (t1, t2, d) in adjacencies)
                {
                    model.AddAdjacency(t1, t2, d);
                }
                return model;
            }
            throw new Exception($"Unsupported Grid type {unwrappedGrid.GetType()}");
        }

        private static IEnumerable<(Tile, Tile, T)> GetAdjacencies<T>(TesseraPalette palette, T d, List<(FaceDetails, Tile)> tiles1, List<(FaceDetails, Tile)> tiles2)
        {
            foreach (var (fd1, t1) in tiles1)
            {
                foreach (var (fd2, t2) in tiles2)
                {
                    if (palette.Match(fd1, fd2))
                    {
                        yield return (t1, t2, d);
                    }
                }
            }
        }

        #endregion



        public static BiMap<Sylves.CellDir, Direction> CubeMapping = new BiMap<Sylves.CellDir , Direction>(new[]
        {
            ((Sylves.CellDir)Sylves.CubeDir.Right, Direction.XPlus),
            ((Sylves.CellDir)Sylves.CubeDir.Left, Direction.XMinus),
            ((Sylves.CellDir)Sylves.CubeDir.Up, Direction.YPlus),
            ((Sylves.CellDir)Sylves.CubeDir.Down, Direction.YMinus),
            ((Sylves.CellDir)Sylves.CubeDir.Forward, Direction.ZPlus),
            ((Sylves.CellDir)Sylves.CubeDir.Back, Direction.ZMinus),
        });

        public static BiMap<Sylves.CellDir, Direction> SquareMapping = new BiMap<Sylves.CellDir, Direction>(new[]
        {
            ((Sylves.CellDir)Sylves.SquareDir.Right, Direction.XPlus),
            ((Sylves.CellDir)Sylves.SquareDir.Left, Direction.XMinus),
            ((Sylves.CellDir)Sylves.SquareDir.Up, Direction.YPlus),
            ((Sylves.CellDir)Sylves.SquareDir.Down, Direction.YMinus),
        });

        // Swaps Up and Forward, going from Sylves conventions to DeBroglies
        public static BiMap<Sylves.CellDir, Direction> HexPrismMapping = new BiMap<Sylves.CellDir, Direction>(new[]
        {
            // TODO: Use SylvesHexPrismDir instead?
            ((Sylves.CellDir)Sylves.PTHexPrismDir.Right, Direction.XPlus),
            ((Sylves.CellDir)Sylves.PTHexPrismDir.Left, Direction.XMinus),
            ((Sylves.CellDir)Sylves.PTHexPrismDir.Forward, Direction.YPlus),
            ((Sylves.CellDir)Sylves.PTHexPrismDir.Back, Direction.YMinus),
            ((Sylves.CellDir)Sylves.PTHexPrismDir.UpRight, (Direction)6),
            ((Sylves.CellDir)Sylves.PTHexPrismDir.UpLeft, Direction.ZPlus),
            ((Sylves.CellDir)Sylves.PTHexPrismDir.DownRight, Direction.ZMinus),
            ((Sylves.CellDir)Sylves.PTHexPrismDir.DownLeft, (Direction)7),
        });

        // Swaps Up and Forward, going from Sylves conventions to DeBroglies
        public static BiMap<Sylves.CellDir, Direction> TrianglePrismMapping = new BiMap<Sylves.CellDir, Direction>(new[]
        {
            ((Sylves.CellDir)SylvesTrianglePrismDir.BackRight, (Direction)6),
            ((Sylves.CellDir)SylvesTrianglePrismDir.Back, Direction.XPlus),
            ((Sylves.CellDir)SylvesTrianglePrismDir.BackLeft, Direction.ZMinus),
            ((Sylves.CellDir)SylvesTrianglePrismDir.ForwardLeft, (Direction)7),
            ((Sylves.CellDir)SylvesTrianglePrismDir.Forward, Direction.XMinus),
            ((Sylves.CellDir)SylvesTrianglePrismDir.ForwardRight, Direction.ZPlus),
            ((Sylves.CellDir)SylvesTrianglePrismDir.Up, Direction.YPlus),
            ((Sylves.CellDir)SylvesTrianglePrismDir.Down, Direction.YMinus),
        });

        public static BiMap<Sylves.CellDir, Direction> GetDirectionMapping(Sylves.IGrid cellGrid)
        {
            cellGrid = cellGrid.Unwrapped;
            if (cellGrid is Sylves.CubeGrid)
            {
                return CubeMapping;
            }
            if (cellGrid is Sylves.SquareGrid)
            {
                return SquareMapping;
            }
            if (cellGrid is Sylves.MeshGrid mg)
            {
                var cellType = mg.GetCellType();
                if (cellType is Sylves.CubeCellType)
                {
                    return CubeMapping;
                }
                if (cellType is Sylves.XZCellTypeModifier modifier)
                {
                    if(modifier.Underlying is Sylves.HexPrismCellType)
                    {
                        return TrianglePrismMapping;
                    }
                }
                throw new Exception($"Unknown cellType {cellType}");
            }
            if (cellGrid is Sylves.HexPrismGrid)
            {
                return HexPrismMapping;
            }
            if (cellGrid is Sylves.TrianglePrismGrid)
            {
                return TrianglePrismMapping;
            }
            throw new Exception($"Unknown cellGrid {cellGrid}");
        }

        private static BiMap<(Direction, Direction, Sylves.Connection), EdgeLabel> GetEdgeLabelMapping(Sylves.IGrid grid, BiMap<Sylves.CellDir, Direction> directionMapping) {
            var i = 0;
            var cellType = grid.GetCellType();
            return new BiMap<(Direction, Direction, Sylves.Connection), EdgeLabel>(
                    cellType.GetCellDirs()
                        // TODO: Filter rotations for triangle cell type?
                        // TODO: Worry about non-trivial connections.
                        .SelectMany(cellDir1 => cellType.GetCellDirs().Select(cellDir2 => ((directionMapping[cellDir1], directionMapping[cellDir2], new Sylves.Connection()), (EdgeLabel)(i++)))));
        }

        public class GenericTopology : ITopology
        {
            private readonly bool[] mask;
            private (int, Direction, EdgeLabel)[,] moves;
            private BiMap<(int, int, int), int> cellIndex;
            private int indexCount;
            private int directionCount;
            private Vector3Int size;

            public GenericTopology(
                (int, Direction, EdgeLabel)[,] moves,
                BiMap<(int, int, int), int> cellIndex,
                int indexCount,
                int directionCount,
                Vector3Int size, 
                bool[] mask)
            {
                this.mask = mask;
                this.moves = moves;
                this.cellIndex = cellIndex;
                this.indexCount = indexCount;
                this.directionCount = directionCount;
                this.size = size;
            }


            public GenericTopology(Sylves.IGrid grid, bool[] mask = null)
            {
                indexCount = grid.IndexCount;
                var cellType = grid.GetCellType();
                var directionMapping = GetDirectionMapping(grid);
                var edgeLabelMapping = GetEdgeLabelMapping(grid, directionMapping);
                directionCount = cellType.GetCellDirs().Count();
                cellIndex = new BiMap<(int, int, int), int>(grid.GetCells().Select(cell =>
                {
                    var index = grid.GetIndex(cell);
                    return ((cell.x, cell.y, cell.z), index);
                }));
                moves = new (int, Direction, EdgeLabel)[indexCount, directionCount];
                foreach (var cell in grid.GetCells())
                {
                    var i = grid.GetIndex(cell);
                    for (var d = 0; d < directionCount; d++)
                    {
                        var dir = (Direction)d;
                        var cellDir = directionMapping[dir];

                        if (grid.TryMove(cell, cellDir, out var dest, out var inverseCellDir, out var connection))
                        {
                            var desti = grid.GetIndex(dest);
                            var inverseDir = directionMapping[inverseCellDir];
                            var edgeLabel = edgeLabelMapping[(dir, inverseDir, connection)];
                            moves[i, d] = (desti, inverseDir, edgeLabel);
                        }
                        else
                        {
                            moves[i, d] = (-1, default, default);
                        }
                    }
                }
                if (mask == null)
                {

                    this.mask = new bool[indexCount];
                    foreach (var index in cellIndex.Select(x => x.Item2))
                    {
                        this.mask[index] = true;
                    }
                }
                else
                {
                    this.mask = mask;
                }
            }

            public int IndexCount => indexCount;

            public int DirectionsCount => directionCount;

            public int Width => size.x;

            public int Height => size.y;

            public int Depth => size.z;

            public bool[] Mask => mask;

            public void GetCoord(int index, out int x, out int y, out int z)
            {
                (x, y, z) = cellIndex[index];
            }

            public int GetIndex(int x, int y, int z)
            {
                return cellIndex[(x, y, z)];
            }

            public bool TryMove(int index, Direction direction, out int dest, out Direction inverseDirection, out EdgeLabel edgeLabel)
            {
                (dest, inverseDirection, edgeLabel) = moves[index, (int)direction];
                return (dest >= 0);
            }

            public bool TryMove(int index, Direction direction, out int dest)
            {
                return TryMove(index, direction, out dest, out var _, out var _);
            }

            public bool TryMove(int x, int y, int z, Direction direction, out int dest, out Direction inverseDirection, out EdgeLabel edgeLabel)
            {
                return TryMove(GetIndex(x, y, z), direction, out dest, out inverseDirection, out edgeLabel);
            }

            public bool TryMove(int x, int y, int z, Direction direction, out int dest)
            {
                return TryMove(GetIndex(x, y, z), direction, out dest);
            }

            public bool TryMove(int x, int y, int z, Direction direction, out int destx, out int desty, out int destz)
            {
                var b = TryMove(GetIndex(x, y, z), direction, out var dest);
                GetCoord(dest, out destx, out desty, out destz);
                return b;
            }

            public ITopology WithMask(bool[] mask)
            {
                var m2 = new bool[IndexCount];
                for (var i = 0; i < IndexCount; i++)
                {
                    m2[i] = this.mask[i] && mask[i];
                }
                return new GenericTopology(moves, cellIndex, indexCount, directionCount, size, m2);
            }
        }


        internal static Vector3Int? GetContradictionLocation(ITopoArray<ModelTile?> result, Sylves.IGrid grid)
        {
            var topology = result.Topology;
            var mask = topology.Mask ?? Enumerable.Range(0, topology.IndexCount).Select(x => true).ToArray();

            var empty = mask.ToArray();
            for (var x = 0; x < topology.Width; x++)
            {
                for (var y = 0; y < topology.Height; y++)
                {
                    for (var z = 0; z < topology.Depth; z++)
                    {
                        var p = new Sylves.Cell(x, y, z);
                        // Skip if already filled
                        if (!empty[grid.GetIndex(p)])
                            continue;
                        var modelTile = result.Get(x, y, z);
                        if (modelTile == null)
                            continue;
                        var tile = modelTile.Value.Tile;
                        if (tile == null)
                        {
                            return new Vector3Int(x, y, z);
                        }
                    }
                }
            }

            return null;
        }

        #endregion


        #region ModelTile conversion
        /// <summary>
        /// Converts from DeBroglie's array format back to Tessera's.
        /// Note these do not have a world position, you'll need to call .Align on them first.
        /// </summary>
        internal static IDictionary<Vector3Int, ModelTile> ToTileDictionary(ITopoArray<ModelTile?> result, Sylves.IGrid grid)
        {
            var data = new Dictionary<Vector3Int, ModelTile>();
            var topology = result.Topology;
            var mask = topology.Mask ?? Enumerable.Range(0, topology.IndexCount).Select(x => true).ToArray();

            var empty = mask.ToArray();
            foreach(var i in topology.GetIndices())
            {
                topology.GetCoord(i, out var x, out var y, out var z);
                var modelTile = result.Get(x, y, z);
                if (modelTile == null)
                    continue;
                data[new Vector3Int(x, y, z)] = modelTile.Value;
            }

            return data;
        }

        #endregion
    }
}
