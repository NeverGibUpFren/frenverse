using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tessera
{
    class UnityEditorEngineInterface : IEngineInterface
    {
        private readonly bool registerUndo;
        private readonly string undoName;

        public UnityEditorEngineInterface(bool registerUndo, string undoName)
        {
            this.registerUndo = registerUndo;
            this.undoName = undoName;
        }

        public GameObject Instantiate(GameObject gameObject, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (gameObject.scene.name != null)
            {
                return GameObject.Instantiate(gameObject, position, rotation, parent);
            }
            else
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(gameObject);
                if(go == null)
                {
                    Debug.Log(gameObject);
                    Debug.Log(PrefabUtility.GetPrefabAssetType(gameObject));
                }
                go.transform.parent = parent;
                go.transform.position = position;
                go.transform.rotation = rotation;
                return go;
            }
        }

        public void Destroy(UnityEngine.Object o)
        {
            if (registerUndo)
            {
                Undo.DestroyObjectImmediate(o);
            }
            else
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
        }
        public void RegisterCompleteObjectUndo(UnityEngine.Object objectToUndo)
        {
            if (registerUndo)
            {
                Undo.RegisterCompleteObjectUndo(objectToUndo, undoName);
            }
        }

        public void RegisterCreatedObjectUndo(UnityEngine.Object objectToUndo)
        {
            if (registerUndo)
            {
                Undo.RegisterCreatedObjectUndo(objectToUndo, undoName);
            }
        }
    }
}
