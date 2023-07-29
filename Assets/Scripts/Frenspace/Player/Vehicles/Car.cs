using UnityEngine;

namespace Frenspace.Player
{
  /// <summary>
  /// Handles the interaction between Car and Player
  /// </summary>
  public class Car : MonoBehaviour
  {
    private GameObject player;

    private bool canEnter = false;
    private bool entered = false;

    void Update()
    {
      if (canEnter && Input.GetKeyDown("e"))
      {
        if (entered)
        {
          LeaveCar();
          player = null;
        }
        else
        {
          player = GameObject.FindWithTag("Player");
          EnterCar();
        }
      }
    }

    void EnterCar()
    {
      player.GetComponent<PlayerMovement>().enabled = false;
      player.GetComponent<PlayerMovementAnimation>().enabled = false;
      player.GetComponent<CharacterController>().enabled = false;

      player.transform.position = transform.TransformPoint(new Vector3(-0.51f, 0.176f, 0.697f));
      player.transform.parent = transform;
      player.transform.localRotation = new Quaternion();

      GetComponentInParent<CharacterController>().enabled = true;
      GetComponentInParent<CarMovement>().enabled = true;

      entered = true;
      this.Invoke(() =>
      {
        GameObject.FindGameObjectWithTag("Buttons").transform.Find("E").gameObject.SetActive(false);
      }, 0.1f);
    }

    void LeaveCar()
    {
      GetComponentInParent<CarMovement>().enabled = false;
      GetComponentInParent<CharacterController>().enabled = false;

      player.transform.parent = null;
      player.transform.position = transform.TransformPoint(new Vector3(-2.39f, 0.06f, 0.697f));

      player.GetComponent<CharacterController>().enabled = true;
      player.GetComponent<PlayerMovementAnimation>().enabled = true;
      player.GetComponent<PlayerMovement>().enabled = true;

      entered = false;
    }

    void OnTriggerEnter(Collider other)
    {
      canEnter = true;
      GameObject.FindGameObjectWithTag("Buttons").transform.Find("E").gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
      canEnter = false;
      GameObject.FindGameObjectWithTag("Buttons").transform.Find("E").gameObject.SetActive(false);
    }
  }

}
