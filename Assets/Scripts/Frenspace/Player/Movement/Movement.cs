using System.Collections.Generic;
using GameEvents;
using GameStates;
using UnityEngine;
using UnityEditor;

namespace Frenspace.Player {

  /// <summary>
  /// Base movement implementation
  /// </summary>

  [RequireComponent(typeof(Rigidbody))]
  public class Movement : MonoBehaviour {
    public float speed = 1.0f;

    protected Camera cam;
    protected MovementAnimation anmtr;
    protected new Rigidbody rigidbody;

    virtual protected void Start() {
      anmtr = gameObject.GetComponent<MovementAnimation>();
      cam = Camera.main;
      rigidbody = GetComponent<Rigidbody>();
    }

    protected void Update() {
      HandleKeys();

      Move(MovementModifier(CalculateMovement()));
    }

    Vector3 lastPos = Vector3.zero;
    Vector3 lastVel = Vector3.zero;
    protected void FixedUpdate() {
      var pos = transform.position;
      var vel = rigidbody.velocity;
      if (!vel.Equals(lastVel) && !pos.Equals(lastPos)) {
        lastPos = pos;
        lastVel = vel;
        // Debug.Log(vel);
        HandleMovementUpdate(vel);
      }
    }

    protected void HandleMovementUpdate(Vector3 movement) {
      HandleNetwork(movement);
      anmtr?.Animate(movement);
    }

    protected KeyCode[] keys = new KeyCode[] { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D };
    protected List<KeyCode> keysPressed = new List<KeyCode>();
    protected void HandleKeys() {
      for (int i = 0; i < keys.Length; i++) {
        KeyCode key = keys[i];
        if (Input.GetKeyDown(key)) {
          keysPressed.Add(key);
        }
        if (Input.GetKeyUp(key)) {
          keysPressed.Remove(key);
        }
      }
    }

    protected float GetSnapAngle(float angleStep) {
      float yRotation = transform.localRotation.eulerAngles.y;
      yRotation = Mathf.RoundToInt(yRotation / angleStep) * angleStep;
      return yRotation;
    }


    MovementState lastMovementState = MovementState.STOPPED;

    virtual protected void HandleNetwork(Vector3 movement) {
      var ms = MovementState.STOPPED;

      switch (Mathf.Round(transform.eulerAngles.y)) {
        case 0: ms = MovementState.NORTH; break;
        case 180: ms = MovementState.SOUTH; break;
        case 270: ms = MovementState.WEST; break;
        case 90: ms = MovementState.EAST; break;
      }
      if (movement.y > 0) ms = MovementState.UP;
      if (movement.y < 0) ms = MovementState.DOWN;

      if (keysPressed.Count == 0 || movement.Equals(Vector3.zero)) ms = MovementState.STOPPED;

      if (ms == lastMovementState) return;
      lastMovementState = ms;

      Client.main?.Send(new byte[] { (byte)GameEvent.MOVE, (byte)ms });
      Debug.Log(ms);
    }

    float lastCamSnapAngle = 0f;

    protected Vector3 CalculateMovement() {
      Vector3 lf = transform.forward;
      transform.forward = cam.transform.forward;
      float camSnapAngle = GetSnapAngle(90f);
      transform.forward = lf;

      Vector3 movement = new Vector3();
      float moveAngle = 0f;

      if (keysPressed.Count > 0) {
        var rightClickView = Input.GetMouseButton(1);

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
          lastCamSnapAngle = camSnapAngle;
        }
      }

      return movement;
    }

    virtual protected Quaternion RotationModifier(Quaternion rotation) {
      return rotation;
    }

    virtual protected Vector3 MovementModifier(Vector3 movement) {
      return movement;
    }

    protected void Move(Vector3 movement) {
      // transform.position += (speed * movement) * Time.deltaTime;
      rigidbody.velocity = speed * movement;
    }

    public void Port(Vector3 to, bool stopMotion = true) {
      if (stopMotion)
        rigidbody.velocity = Vector3.zero;

      rigidbody.MovePosition(to);
    }

    void OnCollisionEnter(Collision collision) {
      if (collision.gameObject.tag == "Player") return;
      HandleMovementUpdate(Vector3.zero);
    }

  }

}