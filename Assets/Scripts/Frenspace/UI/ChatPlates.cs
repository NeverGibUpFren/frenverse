using UnityEngine;

public class ChatPlates : MonoBehaviour
{
  public GameObject chatPlate;

  public float offsetY = 1f;

  GameObject[] frens;
  GameObject[] plates;

  void Start()
  {
    frens = GameObject.FindGameObjectsWithTag("Fren");

    plates = new GameObject[frens.Length];
    for (int i = 0; i < frens.Length; i++)
    {
      var plate = Instantiate(chatPlate);
      plate.transform.SetParent(transform);
      plate.transform.localScale = new Vector3(1, 1, 1);
      // plate.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 44);
      plates[i] = plate;
    }
  }

  void Update()
  {
    for (int i = 0; i < frens.Length; i++)
    {
      var point = Camera.main.WorldToScreenPoint(frens[i].transform.GetChild(0).position);
      if (point.z < 0f)
      {
        point = new Vector3(-100f, -100f, -100f);
      }
      plates[i].transform.position = point;
    }

  }
}
