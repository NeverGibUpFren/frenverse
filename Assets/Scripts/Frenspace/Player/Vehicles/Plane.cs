using UnityEngine;

namespace Frenspace.Player
{
  /// <summary>
  /// Handles the interaction between Car and Player
  /// </summary>
  public class Plane : MonoBehaviour
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
          if (IsGrounded())
          {
            LeavePlane();
            player = null;
          }
        }
        else
        {
          player = GameObject.FindWithTag("Player");
          EnterPlane();
        }
      }
    }

    bool IsGrounded()
    {
      return Physics.Raycast(transform.parent.position, -Vector3.up, 0.003f);
    }

    void EnterPlane()
    {
      player.GetComponent<PlayerMovement>().enabled = false;
      player.GetComponent<PlayerMovementAnimation>().enabled = false;
      player.GetComponent<CharacterController>().enabled = false;

      player.transform.position = transform.TransformPoint(new Vector3(-0.5f, 0.444f, 0.196f));
      player.transform.parent = transform;
      player.transform.localRotation = new Quaternion();

      GetComponentInParent<CharacterController>().enabled = true;
      GetComponentInParent<PlaneMovement>().enabled = true;

      entered = true;
    }

    void LeavePlane()
    {
      GetComponentInParent<PlaneMovement>().enabled = false;
      GetComponentInParent<CharacterController>().enabled = false;

      player.transform.parent = null;
      player.transform.position = transform.TransformPoint(new Vector3(-2.345f, 0.06f, 0.195f));

      player.GetComponent<CharacterController>().enabled = true;
      player.GetComponent<PlayerMovementAnimation>().enabled = true;
      player.GetComponent<PlayerMovement>().enabled = true;

      entered = false;
    }

    void OnTriggerEnter(Collider other)
    {
      canEnter = true;
    }

    void OnTriggerExit(Collider other)
    {
      canEnter = false;
    }
  }

}
