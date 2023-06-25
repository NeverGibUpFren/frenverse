using System;

namespace Tessera
{
    internal class ForEachOutput : ITesseraTileOutput
    {
        private Action<TesseraTileInstance> onCreate;

        public ForEachOutput(Action<TesseraTileInstance> onCreate)
        {
            this.onCreate = onCreate;
        }

        public bool IsEmpty => throw new NotImplementedException();

        public bool SupportsIncremental => throw new NotImplementedException();

        public void ClearTiles(IEngineInterface engine)
        {
            throw new NotImplementedException();
        }

        public void UpdateTiles(TesseraCompletion completion, IEngineInterface engine)
        {
            foreach (var i in completion.tileInstances)
            {
                onCreate(i);
            }
        }
    }
}
