using UnityEngine;

/// <summary>
/// Handles the interaction between Car and Player
/// </summary>
public class Car : MonoBehaviour
{
  private GameObject player;

  private bool entered = false;

  void Update()
  {
    if (player && Input.GetKeyDown("e"))
    {
      if (entered)
      {
        LeaveCar();
      }
      else
      {
        EnterCar();
      }
    }
  }

  void EnterCar()
  {
    player.GetComponent<Movement>().enabled = false;
    player.transform.position = transform.TransformPoint(new Vector3(-0.51f, 0.176f, 0.697f));
    player.transform.parent = transform;
    player.transform.localRotation = new Quaternion();

    entered = true;
  }

  void LeaveCar()
  {
    player.transform.parent = null;
    player.transform.position = transform.TransformPoint(new Vector3(-2.39f, 0.06f, 0.697f));

    entered = false;
    Invoke("ReactivateMovement", 0.1f); // ugly and weird, there should be a better way
  }

  void ReactivateMovement()
  {
    player.GetComponent<Movement>().enabled = true;
  }

  void OnTriggerEnter(Collider other)
  {
    if (other.GetComponent<Movement>())
    {
      player = other.gameObject;
    }
  }

  void OnTriggerExit(Collider other)
  {
    player = null;
  }
}
