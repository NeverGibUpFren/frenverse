using System.Collections.Generic;
using GameEvents;
using GameStates;
using UnityEngine;

namespace Frenspace.Player {

  /// <summary>
  /// Base movement implementation
  /// </summary>

  [RequireComponent(typeof(BoxCollider))]
  public class Movement : MonoBehaviour {
    public float speed = 1.0f;

    protected Camera cam;
    protected CharacterController ctrlr;
    protected MovementAnimation anmtr;


    virtual protected void Start() {
      ctrlr = gameObject.GetComponent<CharacterController>();
      anmtr = gameObject.GetComponent<MovementAnimation>();
      cam = Camera.main;

      // TODO: handle chat input
    }

    bool isCurrentlyColliding;

    void OnCollisionEnter(Collision col) {
      Debug.Log(col.gameObject.name);
      isCurrentlyColliding = true;
    }

    void OnCollisionExit(Collision col) {
      isCurrentlyColliding = false;
    }

    protected void Update() {
      var keyChanged = HandleKeys();

      var (movement, camSnapped, collision) = CalculateMovement();
      movement = MovementModifier(movement);

      HandleNetwork(keyChanged || camSnapped, movement);

      Move(movement);

      anmtr?.Animate(movement);
    }

    protected KeyCode[] keys = new KeyCode[] { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D };
    protected List<KeyCode> keysPressed = new List<KeyCode>();
    protected bool HandleKeys() {
      var changed = false;
      for (int i = 0; i < keys.Length; i++) {
        KeyCode key = keys[i];
        if (Input.GetKeyDown(key)) {
          keysPressed.Add(key);
          changed = true;
        }
        if (Input.GetKeyUp(key)) {
          keysPressed.Remove(key);
          changed = true;
        }
      }
      return changed;
    }

    protected float GetSnapAngle(float angleStep) {
      float yRotation = transform.localRotation.eulerAngles.y;
      yRotation = (float)Mathf.RoundToInt(yRotation / angleStep) * angleStep;
      return yRotation;
    }

    virtual protected Quaternion RotationModifier(Quaternion rotation) {
      return rotation;
    }

    virtual protected void HandleNetwork(bool keyChanged, Vector3 movement) {
      if (!keyChanged) return;

      var ms = MovementState.STOPPED;

      switch (Mathf.Round(transform.eulerAngles.y)) {
        case 0: ms = MovementState.NORTH; break;
        case 180: ms = MovementState.SOUTH; break;
        case 270: ms = MovementState.WEST; break;
        case 90: ms = MovementState.EAST; break;
      }

      if (keysPressed.Count == 0) ms = MovementState.STOPPED;

      Client.main?.Send(new byte[] { (byte)GameEvent.MOVE, (byte)ms });
    }

    float lastCamSnapAngle = 0f;

    protected (Vector3, bool, bool) CalculateMovement() {
      Vector3 lf = transform.forward;
      transform.forward = cam.transform.forward;
      float camSnapAngle = GetSnapAngle(90f);
      transform.forward = lf;
      // transform.localRotation = Quaternion.AngleAxis(camSnapAngle, Vector3.up);

      Vector3 movement = new Vector3();
      float moveAngle = 0f;

      bool camSnapped = false;

      if (keysPressed.Count > 0) {
        var rightClickView = Input.GetMouseButton(1);
        // transform.forward = cam.transform.forward;
        transform.localRotation = RotationModifier(Quaternion.AngleAxis((rightClickView ? camSnapAngle : lastCamSnapAngle), Vector3.up));

        switch (keysPressed[keysPressed.Count - 1]) {
          case KeyCode.W:
            moveAngle = 0;
            movement = transform.TransformDirection(Vector3.forward);
            break;
          case KeyCode.S:
            moveAngle = 180;
            movement = transform.TransformDirection(Vector3.back);
            break;
          case KeyCode.A:
            moveAngle = -90;
            movement = transform.TransformDirection(Vector3.left);
            break;
          case KeyCode.D:
            moveAngle = 90;
            movement = transform.TransformDirection(Vector3.right);
            break;
        }

        transform.localRotation = RotationModifier(Quaternion.AngleAxis((rightClickView ? camSnapAngle : lastCamSnapAngle) + moveAngle, Vector3.up));

        if (rightClickView) {
          camSnapped = lastCamSnapAngle != camSnapAngle;
          lastCamSnapAngle = camSnapAngle;
        }
      }

      return (movement, camSnapped, false);
    }

    virtual protected Vector3 MovementModifier(Vector3 movement) {
      return movement;
    }

    protected void Move(Vector3 movement) {
      ctrlr.Move((speed * movement) * Time.deltaTime);
    }

    public void Port(Vector3 to) {
      this.enabled = false;

      this.Defer(() => {
        transform.position = to;

        this.Defer(() => {
          this.enabled = true;
        });
      });
    }

  }

}