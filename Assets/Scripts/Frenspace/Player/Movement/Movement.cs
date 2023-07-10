using System.Collections.Generic;
using UnityEngine;

namespace Frenspace.Player
{
  /// <summary>
  /// Base movement implementation
  /// </summary>
  [RequireComponent(typeof(CharacterController))]
  public class Movement : MonoBehaviour
  {
    public float speed = 1.0f;

    protected Camera cam;
    protected CharacterController ctrlr;
    protected MovementAnimation anmtr;

    private ChatInput ci;

    virtual protected void Start()
    {
      ctrlr = gameObject.GetComponent<CharacterController>();
      anmtr = gameObject.GetComponent<MovementAnimation>();
      cam = Camera.main;
      ci = GameObject.FindWithTag("Chat").GetComponent<ChatInput>();
    }

    protected void Update()
    {
      HandleKeys();

      var movement = CalculateMovement();

      Move(movement);

      anmtr?.Animate(movement);
    }

    protected KeyCode[] keys = new KeyCode[] { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D };
    protected List<KeyCode> keysPressed = new List<KeyCode>();
    protected void HandleKeys()
    {
      if (ci.active) { return; }

      for (int i = 0; i < keys.Length; i++)
      {
        KeyCode key = keys[i];
        if (Input.GetKeyDown(key))
        {
          keysPressed.Add(key);
          // start move
        }
        if (Input.GetKeyUp(key))
        {
          keysPressed.Remove(key);
          // stop move
        }
      }
    }

    protected float GetSnapAngle(float angleStep)
    {
      float yRotation = transform.localRotation.eulerAngles.y;
      yRotation = (float)Mathf.RoundToInt(yRotation / angleStep) * angleStep;
      return yRotation;
    }

    virtual protected Quaternion RotationModifier(Quaternion rotation)
    {
      return rotation;
    }

    float lastCamSnapAngle = 0f;

    protected Vector3 CalculateMovement()
    {
      Vector3 lf = transform.forward;
      transform.forward = cam.transform.forward;
      float camSnapAngle = GetSnapAngle(90f);
      transform.forward = lf;
      // transform.localRotation = Quaternion.AngleAxis(camSnapAngle, Vector3.up);

      Vector3 movement = new Vector3();
      float moveAngle = 0f;

      if (keysPressed.Count > 0)
      {
        var rightClickView = Input.GetMouseButton(1);
        // transform.forward = cam.transform.forward;
        transform.localRotation = RotationModifier(Quaternion.AngleAxis((rightClickView ? camSnapAngle : lastCamSnapAngle), Vector3.up));

        switch (keysPressed[keysPressed.Count - 1])
        {
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

        if (rightClickView)
        {
          lastCamSnapAngle = camSnapAngle;
        }
      }

      return movement;
    }

    virtual protected Vector3 MovementModifier(Vector3 movement)
    {
      return movement;
    }

    protected void Move(Vector3 movement)
    {
      ctrlr.Move((speed * MovementModifier(movement)) * Time.deltaTime);
    }

  }

}