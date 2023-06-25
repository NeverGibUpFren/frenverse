using System.Linq;
using UnityEngine;

namespace Tessera
{
    public class InstantiateOutput : ITesseraTileOutput
    {
        private readonly Transform transform;

        public InstantiateOutput(Transform transform)
        {
            this.transform = transform;
        }

        public bool IsEmpty => transform.childCount == 0;

        public bool SupportsIncremental => false;

        public void ClearTiles(IEngineInterface engine)
        {
            var children = transform.Cast<Transform>().ToList();
            foreach (var child in children)
            {
                engine.Destroy(child.gameObject);
            }
        }

        public void UpdateTiles(TesseraCompletion completion, IEngineInterface engine)
        {
            foreach (var i in completion.tileInstances)
            {
                foreach (var go in TesseraGenerator.Instantiate(i, transform, engine))
                {
                    engine.RegisterCreatedObjectUndo(go);
                }
            }
        }
    }
}
