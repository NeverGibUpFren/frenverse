using UnityEngine;
using System.Collections;

struct Cloud
{
  public Transform t;
  public float opacity;
  public Vector3 orgPos;
  public float speed;
  public float offset;
}

public class CloudGenerator : MonoBehaviour
{

  public int amount = 100;

  public GameObject cloudPrefab;

  public float width = 1000.0f;

  public float depth = 1000.0f;

  public float minScale = 20f;
  public float maxScale = 40f;

  public float minSpeed = 0.1f;
  public float maxSpeed = 0.3f;

  public float heightDiff = 20f;

  private Cloud[] refs = new Cloud[0];

  Camera mainCamera;
  void Start()
  {
    mainCamera = Camera.main;
  }
  void LateUpdate()
  {
    Vector3 newRotation = mainCamera.transform.eulerAngles;
    newRotation.x = 90;
    newRotation.z = 0;
    for (int i = 0; i < refs.Length; i++)
    {
      Cloud c = refs[i];

      c.t.eulerAngles = newRotation;

      float mod = 0.5f + Mathf.Sin(Time.time + c.offset) * (.005f * c.speed);

      c.t.position = new Vector3(c.t.position.x, c.orgPos.y * mod * 2f, c.t.position.z + c.speed * 0.01f);

      if (c.t.position.z > depth)
      {
        // if (c.r.color.a > 0)
        // {
        //   // fade out
        //   c.r.color -= new Color(0, 0, 0, 0.001f);
        // }

        // destroy
        Destroy(c.t.gameObject);
        refs[i] = GenerateCloud(true);
      }
      else
      {
        // if (c.r.color.a < c.opacity)
        // {
        //   // fade in
        //   c.r.color += new Color(0, 0, 0, 0.001f);
        // }
      }
    }
  }

  void Awake()
  {
    while (transform.childCount > 0)
    {
      DestroyImmediate(transform.GetChild(0).gameObject);
    }

    refs = new Cloud[amount];
    for (int i = 0; i < amount; i++)
    {
      refs[i] = GenerateCloud(false);
    }
  }

  [ContextMenu("Simulate")]
  void Simulate()
  {
    Awake();
  }

  Cloud GenerateCloud(bool atStart)
  {
    GameObject cloud = GameObject.Instantiate(
        cloudPrefab,
        new Vector3(Random.Range(0, width), Random.Range(transform.position.y - heightDiff, transform.position.y + heightDiff), atStart ? 0 : Random.Range(0, depth)),
        cloudPrefab.transform.rotation
    ) as GameObject;
    float scale = Random.Range(minScale, maxScale);
    cloud.transform.localScale = new Vector3(scale, 0.1f, scale);

    cloud.transform.SetParent(this.transform);
    cloud.hideFlags = HideFlags.HideInHierarchy;

    Cloud c = new Cloud();
    c.t = cloud.transform;
    // c.r = cloud.GetComponent<SpriteRenderer>();
    c.orgPos = cloud.transform.position + new Vector3();
    c.speed = Random.Range(minSpeed, maxSpeed);
    // c.r.flipX = Random.Range(0, 10) > 5f ? true : false;
    c.offset = Random.Range(1, 1000);
    c.opacity = Random.Range(0.6f, 1);
    if (atStart)
    {
      // c.r.color = new Color(1, 1, 1, 0);
    }
    else
    {
      // c.r.color = new Color(1, 1, 1, c.opacity);
    }

    return c;
  }
}