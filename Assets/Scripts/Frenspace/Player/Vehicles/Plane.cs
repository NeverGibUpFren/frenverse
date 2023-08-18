using UnityEngine;

namespace Frenspace.Player {
  /// <summary>
  /// Handles the interaction between Car and Player
  /// </summary>
  public class Plane : MonoBehaviour {
    private GameObject player;

    private bool canEnter = false;
    private bool entered = false;

    void Update() {
      if (canEnter && Input.GetKeyDown("e")) {
        if (entered) {
          if (IsGrounded()) {
            LeavePlane();
          }
        }
        else {
          player = GameObject.FindWithTag("Player");
          EnterPlane();
        }
      }
    }

    bool IsGrounded() {
      Debug.DrawLine(transform.parent.position, transform.parent.position + Vector3.down * 0.03f, Color.red, 10f);
      return Physics.Raycast(transform.parent.position, Vector3.down, 0.03f);
    }

    void EnterPlane() {
      GetComponentInChildren<BoxCollider>().enabled = false;

      player.GetComponent<Rigidbody>().detectCollisions = false;
      player.GetComponent<Rigidbody>().isKinematic = true;
      player.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
      player.GetComponent<PlayerMovement>().enabled = false;
      player.GetComponent<PlayerMovementAnimation>().Animate(Vector3.zero);

      this.Defer(() => {
        player.transform.parent = transform;
        player.transform.position = transform.TransformPoint(new Vector3(-0.5f, 0.444f, 0.196f));
        player.transform.localRotation = new Quaternion();

        GetComponentInParent<PlaneMovement>().enabled = true;
        GetComponentInParent<Rigidbody>().isKinematic = false;

        entered = true;
      });
    }

    void LeavePlane() {
      GetComponentInParent<PlaneMovement>().enabled = false;
      GetComponentInParent<Rigidbody>().isKinematic = true;

      player.transform.position = transform.TransformPoint(new Vector3(-2.345f, -0.02466f, 0.195f));
      player.transform.parent = null;

      this.Defer(() => {
        player.GetComponent<Rigidbody>().detectCollisions = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        player.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        player.GetComponent<PlayerMovement>().enabled = true;

        GetComponentInChildren<BoxCollider>().enabled = true;
        entered = false;
      });
    }

    void OnTriggerEnter(Collider other) {
      canEnter = true;
    }

    void OnTriggerExit(Collider other) {
      canEnter = false;
    }
  }

}
