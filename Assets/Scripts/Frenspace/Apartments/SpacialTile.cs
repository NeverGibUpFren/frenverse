using System.Collections.Generic;
using UnityEngine;

namespace Frenspace.Apartment
{
  public class SpacialTile
  {
    public SpacialTile left, right, up, down, forward, back;

    public GameObject tile;

    public List<SpacialTile> neighbors
    {
      get
      {
        return new List<SpacialTile> { left, right, up, down, forward, back }.FindAll(t => t != null);
      }
    }
  }
}
