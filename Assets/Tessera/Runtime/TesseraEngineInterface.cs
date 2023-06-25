using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tessera
{
    public interface IEngineInterface
    {
        GameObject Instantiate(GameObject gameObject, Vector3 position, Quaternion rotation, Transform parent);
        void Destroy(UnityEngine.Object o);
        void RegisterCompleteObjectUndo(UnityEngine.Object objectToUndo);
        void RegisterCreatedObjectUndo(UnityEngine.Object objectToUndo);
    }

    public class UnityEngineInterface : IEngineInterface
    {
        private static UnityEngineInterface instance;

        public static UnityEngineInterface Instance => instance;

        static UnityEngineInterface()
        {
            instance = new UnityEngineInterface();
        }

        public GameObject Instantiate(GameObject gameObject, Vector3 position, Quaternion rotation, Transform parent)
        {
            return GameObject.Instantiate(gameObject, position, rotation, parent);
        }

        public void Destroy(UnityEngine.Object o)
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(o);
                if (o is GameObject go)
                    go.SetActive(false);
            }
            else
            {
                GameObject.DestroyImmediate(o);
            }
        }

        public void RegisterCompleteObjectUndo(UnityEngine.Object objectToUndo)
        {
            // Do nothing
        }

        public void RegisterCreatedObjectUndo(UnityEngine.Object objectToUndo)
        {
            // Do nothing
        }
    }
}
