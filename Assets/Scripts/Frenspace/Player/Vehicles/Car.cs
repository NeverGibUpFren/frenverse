using UnityEngine;

namespace Frenspace.Player {
  /// <summary>
  /// Handles the interaction between Car and Player
  /// </summary>
  public class Car : MonoBehaviour {
    private GameObject player;

    private bool canEnter = false;
    private bool entered = false;

    void Update() {
      if (canEnter && Input.GetKeyDown("e")) {
        if (entered) {
          LeaveCar();
          player = null;
        }
        else {
          player = GameObject.FindWithTag("Player");
          EnterCar();
        }
      }
    }

    // void EnterCar() {
    //   player.GetComponent<PlayerMovement>().enabled = false;
    //   player.GetComponent<PlayerMovementAnimation>().enabled = false;

    //   player.transform.position = transform.TransformPoint(new Vector3(-0.51f, 0.176f, 0.697f));
    //   player.transform.parent = transform;
    //   player.transform.localRotation = new Quaternion();

    //   GetComponentInParent<CarMovement>().enabled = true;

    //   entered = true;
    // }

    // void LeaveCar() {
    //   GetComponentInParent<CarMovement>().enabled = false;

    //   player.transform.parent = null;
    //   player.transform.position = transform.TransformPoint(new Vector3(-2.39f, 0.06f, 0.697f));

    //   player.GetComponent<PlayerMovementAnimation>().enabled = true;
    //   player.GetComponent<PlayerMovement>().enabled = true;

    //   entered = false;
    // }

    void EnterCar() {
      GetComponentInChildren<BoxCollider>().enabled = false;

      player.GetComponent<Rigidbody>().detectCollisions = false;
      player.GetComponent<Rigidbody>().isKinematic = true;
      player.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
      player.GetComponent<PlayerMovement>().enabled = false;
      player.GetComponent<PlayerMovementAnimation>().Animate(Vector3.zero);

      this.Defer(() => {
        player.transform.parent = transform;
        player.transform.position = transform.TransformPoint(new Vector3(-0.51f, 0.176f, 0.697f));
        player.transform.localRotation = new Quaternion();

        GetComponentInParent<CarMovement>().enabled = true;
        GetComponentInParent<Rigidbody>().isKinematic = false;

        entered = true;
      });
    }

    void LeaveCar() {
      GetComponentInParent<CarMovement>().enabled = false;
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
