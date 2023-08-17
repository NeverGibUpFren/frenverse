using System.Collections.Generic;
using GameEvents;
using GameStates;
using UnityEngine;

namespace Frenspace.Player {

  /// <summary>
  /// Base movement implementation
  /// </summary>

  public class Movement : MonoBehaviour {
    public LayerMask layerMask;
    public float speed = 1.0f;

    protected Camera cam;
    protected MovementAnimation anmtr;


    virtual protected void Start() {
      anmtr = gameObject.GetComponent<MovementAnimation>();
      cam = Camera.main;

      // TODO: handle chat input
    }

    protected void Update() {
      var keyChanged = HandleKeys();

      var (movement, camSnapped, collision) = CalculateMovement();
      movement = MovementModifier(movement);

      HandleNetwork(keyChanged || camSnapped || collision, movement);

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

    MovementState lastMovementState = MovementState.STOPPED;

    virtual protected void HandleNetwork(bool keyChanged, Vector3 movement) {
      if (!keyChanged) return;

      var ms = MovementState.STOPPED;

      switch (Mathf.Round(transform.eulerAngles.y)) {
        case 0: ms = MovementState.NORTH; break;
        case 180: ms = MovementState.SOUTH; break;
        case 270: ms = MovementState.WEST; break;
        case 90: ms = MovementState.EAST; break;
      }

      if (keysPressed.Count == 0 || movement.Equals(Vector3.zero)) ms = MovementState.STOPPED;

      if (ms == lastMovementState) return;
      lastMovementState = ms;

      Client.main?.Send(new byte[] { (byte)GameEvent.MOVE, (byte)ms });
      // Debug.Log(ms);
    }

    float lastCamSnapAngle = 0f;
    Vector3 lastCollision;

    protected (Vector3, bool, bool) CalculateMovement() {
      Vector3 lf = transform.forward;
      transform.forward = cam.transform.forward;
      float camSnapAngle = GetSnapAngle(90f);
      transform.forward = lf;

      Vector3 movement = new Vector3();
      float moveAngle = 0f;
      bool camSnapped = false;
      bool collision = false;
      Quaternion lastRot = transform.localRotation;

      if (keysPressed.Count > 0) {
        var rightClickView = Input.GetMouseButton(1);
        lastRot = transform.rotation;

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

        var center = transform.position + new Vector3(0, 0.2f, 0);
        Debug.DrawLine(center, center + transform.forward * 0.1f, Color.red);

        if (Physics.Raycast(center, transform.forward, out RaycastHit hitF, 0.1f, layerMask)) {
          transform.localRotation = lastRot;
          movement = new Vector3();

          if (hitF.point != lastCollision) {
            lastCollision = hitF.point;
            collision = true;
          }
        }
        else {
          lastCollision = default;
        }

      }

      return (movement, camSnapped, collision);
    }

    virtual protected Vector3 MovementModifier(Vector3 movement) {
      return movement;
    }

    protected void Move(Vector3 movement) {
      transform.position += (speed * movement) * Time.deltaTime;
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