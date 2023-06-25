using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tessera
{
    internal class UpdatableInstantiateOutput : ITesseraTileOutput
    {
        private Dictionary<Vector3Int, GameObject[]> instantiated = new Dictionary<Vector3Int, GameObject[]>();
        private readonly Transform transform;

        public UpdatableInstantiateOutput(Transform transform)
        {
            this.transform = transform;
        }

        public bool IsEmpty => transform.childCount == 0;

        public bool SupportsIncremental => true;

        private void Clear(Vector3Int p, IEngineInterface engine)
        {
            if (instantiated.TryGetValue(p, out var gos) && gos != null)
            {
                foreach (var go in gos)
                {
                    engine.Destroy(go);
                }
            }

            instantiated[p] = null;
        }

        public void ClearTiles(IEngineInterface engine)
        {
            foreach (var k in instantiated.Keys.ToList())
            {
                Clear(k, engine);
            }
        }

        public void UpdateTiles(TesseraCompletion completion, IEngineInterface engine)
        {
            foreach(var kv in completion.tileData)
            {
                Clear(kv.Key, engine);
            }
            foreach (var i in completion.tileInstances)
            {
                if (i.Tile != null)
                {
                    instantiated[i.Cells.First()] = TesseraGenerator.Instantiate(i, transform, engine);
                }
            }
        }
    }
}
