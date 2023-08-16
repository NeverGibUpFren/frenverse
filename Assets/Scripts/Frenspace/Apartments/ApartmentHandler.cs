using System.Collections.Generic;
using Frenspace.Player;
using UnityEngine;
using UnityEditor;


namespace Frenspace.Apartments
{
  public class ApartmentHandler : MonoBehaviour
  {
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
          var cellPos = hit.point + ray.direction * .4f; // elongate the rayhit through the "bulding walls"
          cellPos = new Vector3(Mathf.Round(cellPos.x), Mathf.Floor(cellPos.y), Mathf.Round(cellPos.z)); // ceil
          cellPos += new Vector3(0f, 0.5f, 0f); // cell center

          Debug.DrawLine(ray.origin, hit.point + ray.direction * .1f, Color.red, 10f);
          Debug.DrawLine(cellPos, cellPos + Vector3.up, Color.red, 10f);
          var t = cityTileData.GetTileAt(cellPos);
          if (t != null)
          {
            GatherTiles(t);
            EnterApartmentPreview(cellPos);
            // SetupApartmentPreview(tile.transform);
          }
        }
      }
    }

    void GatherTiles(TesseraDataOutput.TileData t)
    {
      var gathered = new List<TesseraDataOutput.TileData> { t };
      int rev = 0;
      GatherNeighbors(t.name.Split("_")[0], t.pos, gathered, ref rev);

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
      foreach (var tile in gatheredTiles)
      {
        var block = new GameObject();
        block.transform.parent = parent.transform;
        block.name = "Tile";

        var dirs = new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        foreach (var dir in dirs)
        {
          var path = "Apartments/Wall";
          Debug.DrawLine(tile.pos + dir, tile.pos + dir + Vector3.up, Color.green, 10f);
          var td = cityTileData.GetTileAt(tile.pos + dir);
          var splits = tile.name.Split("_");
          if (td == null)
          {
            // no tile means its air so we use our own type of tile
            path += $"_{splits[1]}_{splits[2]}";
          }
          else
          {
            // if tile is the same type we don't want a wall
            if (td.name.Contains(splits[0]))
              continue;
          }

          var prefab = Resources.Load<GameObject>(path);
          var wall = Instantiate(prefab);
          wall.transform.parent = block.transform;
          wall.transform.localPosition -= dir * 0.5f;
          wall.transform.Rotate(new Vector3(dir.x, 1f, dir.z - 1f) * 90f);
          var rot = wall.transform.rotation;
          wall.transform.rotation = Quaternion.Euler(0, rot.eulerAngles.y, rot.eulerAngles.z);
        }

        // top and bottom
        var top = Instantiate(Resources.Load<GameObject>("Apartments/Ceiling"));
        top.transform.parent = block.transform;
        top.transform.localPosition += Vector3.up * 0.5f;
        top.transform.Rotate(new Vector3(0, 0, 180f));

        var bottom = Instantiate(Resources.Load<GameObject>("Apartments/Floor"));
        bottom.transform.parent = block.transform;
        bottom.transform.localPosition += Vector3.down * 0.5f;
        bottom.transform.Rotate(new Vector3(0, 0, 0f));

        block.transform.localPosition = (tile.pos - parent.transform.position) * -1f;
        // block.transform.localRotation = new Quaternion();
      }
    }

    public void EnterApartmentPreview(Vector3 pos)
    {
      lastApartment = SetupApartment(pos);

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

    public GameObject SetupApartment(Vector3 pos)
    {
      var parent = new GameObject();
      parent.transform.parent = transform;
      parent.name = "Apartment";
      parent.transform.rotation = new Quaternion();
      parent.transform.position = pos;

      BuildApartment(parent.transform);

      parent.transform.Rotate(new Vector3(0, 180f, 0));
      return parent;
    }

  }
}
