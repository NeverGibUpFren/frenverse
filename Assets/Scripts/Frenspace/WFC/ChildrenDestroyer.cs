using UnityEngine;

public class ChildrenDestroyer : MonoBehaviour
{
  [ContextMenu("Destroy")]
  public void DestroyChildren()
  {
    while (transform.childCount > 0)
    {
      DestroyImmediate(transform.GetChild(0).gameObject);
    }
  }
}
