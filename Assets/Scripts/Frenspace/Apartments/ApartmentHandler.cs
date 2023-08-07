using System.Collections.Generic;
using Frenspace.Player;
using UnityEngine;

namespace Frenspace.Apartments
{
  public class ApartmentHandler : MonoBehaviour
  {
    public Transform selectionScene;
    public GameObject EmptyParentPrefab;
    public GameObject WallPrefab;
    public BuildingTileHolder bth;


    private List<GameObject> gatheredTiles;

    private Transform selectionTransform;

    private Vector3 beforePortPosition;
    private GameObject lastApartment;

    void Update()
    {
      if (Input.GetKeyDown(KeyCode.R))
      {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 10f);
        if (Physics.Raycast(ray, out RaycastHit hit, 25))
        {
          var cellPos = hit.point + ray.direction * .01f; // elongate the rayhit through the "bulding walls"
          cellPos = new Vector3(Mathf.Ceil(cellPos.x), Mathf.Ceil(cellPos.y), Mathf.Ceil(cellPos.z)); // ceil
          cellPos -= new Vector3(0.5f, 0.5f, 0.5f); // cell center

          var tile = bth.GetTileAtPos(cellPos);
          if (tile)
          {
            selectionTransform = tile.transform;
            GatherTiles(tile.transform);
            EnterApartmentPreview();
            // SetupApartmentPreview(tile.transform);
            return;
          }
        }

      }
    }

    void GatherTiles(Transform t)
    {
      while (selectionScene.childCount > 0)
      {
        DestroyImmediate(selectionScene.GetChild(0).gameObject);
      }

      var gathered = new List<GameObject>();
      gathered.Add(t.gameObject);
      int rev = 0;
      GatherNeighbors(t.gameObject.name.Split("Tile")[0], t, gathered, ref rev);

      if (gathered.Count < 1)
      {
        Debug.Log("No apt foudn");
        return;
      }

      gatheredTiles = gathered;
    }

    void GatherNeighbors(string type, Transform t, List<GameObject> gathered, ref int rev)
    {
      rev += 1;
      if (rev > 1000)
      {
        return;
      }

      var dirs = new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back }; // Vector3.up, Vector3.down,
      foreach (var dir in dirs)
      {
        var tile = bth.GetTileAtPos(t.position + dir);
        if (tile && tile.name.Contains(type) && !gathered.Contains(tile.gameObject))
        {
          gathered.Add(tile.gameObject);
          GatherNeighbors(type, tile, gathered, ref rev);
        }
      }
    }

    void SetupApartmentPreview(Transform t)
    {
      var parent = Instantiate(EmptyParentPrefab);
      parent.transform.parent = selectionScene;

      foreach (var tile in gatheredTiles)
      {
        var copy = Instantiate(tile);
        copy.hideFlags = HideFlags.None;
        copy.SetActive(true);
        copy.transform.parent = parent.transform;
        copy.transform.localPosition -= t.localPosition;
      }

      var innerShell = Instantiate(EmptyParentPrefab);
      innerShell.transform.parent = parent.transform;
      BuildApartment(innerShell.transform);
      innerShell.transform.Rotate(new Vector3(0, 180f, 0));

      parent.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
      parent.transform.localPosition = new Vector3();

      foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
      {
        child.gameObject.layer = LayerMask.NameToLayer("ApartmentPreview");
      }

      transform.GetChild(1).gameObject.SetActive(true);
    }

    void BuildApartment(Transform parent)
    {
      var center = gatheredTiles[0].transform.position;

      foreach (var tile in gatheredTiles)
      {
        var block = Instantiate(EmptyParentPrefab);
        block.transform.parent = parent.transform;
        block.name = "Tile";

        var dirs = new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        foreach (var dir in dirs)
        {
          var hasNeighbor = GetTileAtPos(tile.transform.position + dir) != null;
          if (!hasNeighbor)
          {
            // place wall
            var wall = Instantiate(WallPrefab);
            wall.transform.parent = block.transform;
            wall.transform.localPosition -= dir * 0.5f;
            wall.transform.Rotate(new Vector3(dir.x, 1f, dir.z) * 90f);
          }
        }

        // top and bottom
        var top = Instantiate(WallPrefab);
        top.transform.parent = block.transform;
        top.transform.localPosition += Vector3.up * 0.5f;
        top.transform.Rotate(new Vector3(0, 0, 180f));

        var bottom = Instantiate(WallPrefab);
        bottom.transform.parent = block.transform;
        bottom.transform.localPosition += Vector3.down * 0.5f;
        bottom.transform.Rotate(new Vector3());

        block.transform.localPosition = (tile.transform.position - center) * -1f;
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
      var parent = Instantiate(EmptyParentPrefab);
      parent.transform.parent = transform;
      parent.name = "Apartment";

      BuildApartment(parent.transform);

      parent.transform.localRotation = new Quaternion();
      parent.transform.localPosition = selectionTransform.position;
      parent.transform.Rotate(new Vector3(0, 180f, 0));

      return parent;
    }

    public Transform GetTileAtPos(Vector3 pos)
    {
      foreach (GameObject child in gatheredTiles)
      {
        if (Vector3.SqrMagnitude(child.transform.position - pos) < 0.01f)
        {
          return child.transform;
        }
      }
      return null;
    }
  }
}
