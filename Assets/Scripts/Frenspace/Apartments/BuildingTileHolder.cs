using UnityEngine;

namespace Frenspace.Apartments
{

  public class BuildingTileHolder : MonoBehaviour
  {

    public void OptimizeChildren()
    {
      foreach (Transform child in transform)
      {
        child.hideFlags = HideFlags.HideInHierarchy;
        child.gameObject.SetActive(false);
      }
    }

    public Transform GetTileAtPos(Vector3 pos)
    {
      foreach (Transform child in transform)
      {
        if (Vector3.SqrMagnitude(child.position - pos) < 0.01f)
        {
          return child;
        }
      }
      return null;
    }

  }

}