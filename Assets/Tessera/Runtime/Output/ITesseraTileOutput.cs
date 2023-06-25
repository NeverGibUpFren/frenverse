namespace Tessera
{
    public interface ITesseraTileOutput
    {
        /// <summary>
        /// Is this output safe to use with AnimatedGenerator
        /// </summary>
        bool SupportsIncremental { get; }

        /// <summary>
        /// Is the output currently empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Clear the output
        /// </summary>
        void ClearTiles(IEngineInterface engine);

        /// <summary>
        /// Update a chunk of tiles.
        /// If inremental updates are supported, then:
        ///  * Tiles can replace other tiles, as indicated by the <see cref="TesseraTileInstance.Cells"/> field.
        ///  * A tile of null indicates that the tile should be erased
        /// </summary>
        void UpdateTiles(TesseraCompletion completion, IEngineInterface engine);
    }
}
