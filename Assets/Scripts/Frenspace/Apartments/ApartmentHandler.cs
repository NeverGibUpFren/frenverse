using System.Collections.Generic;
using Frenspace.Player;
using UnityEngine;

using Tessera;

namespace Frenspace.Apartments
{
  public class ApartmentHandler : MonoBehaviour
  {
    public GameObject WallPrefab;
    public TesseraDataOutput cityTileData;


    private List<TesseraDataOutput.TileData> gatheredTiles;

    private Vector3 beforePortPosition;
    private GameObject lastApartment;

    void Update()
    {
      if (Input.GetKeyDown(KeyCode.R))
      {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 25))
        {
          var cellPos = hit.point + ray.direction.normalized * .1f; // elongate the rayhit through the "bulding walls"
          cellPos = new Vector3(Mathf.Ceil(cellPos.x), Mathf.Ceil(cellPos.y), Mathf.Ceil(cellPos.z)); // ceil
          cellPos -= new Vector3(0.5f, 0.5f, 0.5f); // cell center

          // Debug.Log(cellPos);
          // Debug.DrawLine(ray.origin, hit.point + ray.direction.normalized * .1f, Color.red, 10f);
          var t = cityTileData.GetTileAt(cellPos);
          if (t != null)
          {
            GatherTiles(t);
            EnterApartmentPreview();
            // SetupApartmentPreview(tile.transform);
          }
        }
      }
    }

    void GatherTiles(TesseraDataOutput.TileData t)
    {
      var gathered = new List<TesseraDataOutput.TileData> { t };
      int rev = 0;
      GatherNeighbors(t.name.Split("Tile")[0], t.pos, gathered, ref rev);

      if (gathered.Count < 1)
      {
        Debug.Log("No apt foudn");
        return;
      }

      gatheredTiles = gathered;
    }

    void GatherNeighbors(string type, Vector3 pos, List<TesseraDataOutput.TileData> gathered, ref int rev)
    {
      rev += 1;
      if (rev > 1000)
      {
        return;
      }

      var dirs = new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back }; // Vector3.up, Vector3.down,
      foreach (var dir in dirs)
      {
        var t = cityTileData.GetTileAt(pos + dir);
        if (t != null && t.name.Contains(type) && !gathered.Contains(t))
        {
          gathered.Add(t);
          GatherNeighbors(type, t.pos, gathered, ref rev);
        }
      }
    }

    void SetupApartmentPreview(Transform t)
    {
      // var parent = Instantiate(EmptyParentPrefab);
      // parent.transform.parent = selectionScene;

      // foreach (var tile in gatheredTiles)
      // {
      //   var copy = Instantiate(tile);
      //   copy.hideFlags = HideFlags.None;
      //   copy.SetActive(true);
      //   copy.transform.parent = parent.transform;
      //   copy.transform.localPosition -= t.localPosition;
      // }

      // var innerShell = Instantiate(EmptyParentPrefab);
      // innerShell.transform.parent = parent.transform;
      // BuildApartment(innerShell.transform);
      // innerShell.transform.Rotate(new Vector3(0, 180f, 0));

      // parent.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
      // parent.transform.localPosition = new Vector3();

      // foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
      // {
      //   child.gameObject.layer = LayerMask.NameToLayer("ApartmentPreview");
      // }

      // transform.GetChild(1).gameObject.SetActive(true);
    }

    void BuildApartment(Transform parent)
    {
      var center = gatheredTiles[0].pos;

      foreach (var tile in gatheredTiles)
      {
        var block = new GameObject();
        block.transform.parent = parent.transform;
        block.name = "Tile";

        var dirs = new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        foreach (var dir in dirs)
        {
          var td = GetTileAtPos(tile.pos + dir) != null;
          if (!td)
          {
            // place wall
            var wall = Instantiate(WallPrefab);
            wall.transform.parent = block.transform;
            wall.transform.localPosition -= dir * 0.5f;
            wall.transform.Rotate(new Vector3(dir.x, 1f, dir.z - 1f) * 90f);
            var rot = wall.transform.rotation;
            wall.transform.rotation = Quaternion.Euler(0, rot.eulerAngles.y, rot.eulerAngles.z);
          }
        }

        // top and bottom
        var top = Instantiate(WallPrefab);
        top.transform.parent = block.transform;
        top.transform.localPosition += Vector3.up * 0.5f;
        top.transform.Rotate(new Vector3(0, 0, 90f));

        var bottom = Instantiate(WallPrefab);
        bottom.transform.parent = block.transform;
        bottom.transform.localPosition += Vector3.down * 0.5f;
        bottom.transform.Rotate(new Vector3(0, 0, -90f));

        block.transform.localPosition = (tile.pos - center) * -1f;
        // block.transform.localRotation = new Quaternion();
      }
    }

    public void EnterApartmentPreview()
    {
      lastApartment = SetupApartment();

      var player = GameObject.FindWithTag("Player");
      beforePortPosition = player.transform.position;
      player.GetComponent<Movement>().Port(lastApartment.transform.position);

      // transform.GetChild(1).gameObject.SetActive(false);
    }

    [ContextMenu("LeaveApartment")]
    public void LeaveApartment()
    {
      GameObject.FindWithTag("Player").GetComponent<Movement>().Port(beforePortPosition);
      Destroy(lastApartment);
    }

    public GameObject SetupApartment()
    {
      var parent = new GameObject();
      parent.transform.parent = transform;
      parent.name = "Apartment";

      BuildApartment(parent.transform);

      parent.transform.localRotation = new Quaternion();
      parent.transform.localPosition = gatheredTiles[0].pos;
      parent.transform.Rotate(new Vector3(0, 180f, 0));

      return parent;
    }

    public TesseraDataOutput.TileData GetTileAtPos(Vector3 pos)
    {
      foreach (TesseraDataOutput.TileData child in gatheredTiles)
      {
        if (Vector3.SqrMagnitude(child.pos - pos) < 0.01f)
        {
          return child;
        }
      }
      return null;
    }
  }
}
