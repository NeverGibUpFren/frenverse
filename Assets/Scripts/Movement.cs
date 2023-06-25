using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Movement : MonoBehaviour
{
  public float speed = 1.0F;

  public Camera cam;

  private CharacterController ctrlr;
  private Animator anmtr;

  void Start()
  {
    ctrlr = gameObject.GetComponent<CharacterController>();
    anmtr = gameObject.GetComponent<Animator>();
  }

  public static Vector3 SnapTo(Vector3 v3, float snapAngle, Vector3 customUpAxis)
  {
    float angle = Vector3.Angle(v3, customUpAxis);
    if (angle < snapAngle / 2.0f)          // Cannot do cross product
      return customUpAxis * v3.magnitude;  //   with angles 0 & 180
    if (angle > 180.0f - snapAngle / 2.0f)
      return -customUpAxis * v3.magnitude;

    float t = Mathf.Round(angle / snapAngle);
    float deltaAngle = (t * snapAngle) - angle;

    Vector3 axis = Vector3.Cross(customUpAxis, v3);
    Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
    return q * v3;
  }

  private float GetSnapAngle(float angleStep)
  {
    float yRotation = transform.localRotation.eulerAngles.y;
    yRotation = (float)Mathf.RoundToInt(yRotation / angleStep) * angleStep;
    return yRotation;
  }

  private char[] keys = new char[] { 'w', 's', 'a', 'd' };
  private List<char> keysPressed = new List<char>();

  void Update()
  {
    Vector3 lf = transform.forward;
    transform.forward = cam.transform.forward;
    float camSnapAngle = GetSnapAngle(90f);
    transform.forward = lf;
    // transform.localRotation = Quaternion.AngleAxis(camSnapAngle, Vector3.up);

    for (int i = 0; i < keys.Length; i++)
    {
      char key = keys[i];
      if (Input.GetKeyDown(key.ToString()))
      {
        keysPressed.Add(key);
        // start move
      }
      if (Input.GetKeyUp(key.ToString()))
      {
        keysPressed.Remove(key);
        // stop move
      }
    }


    Vector3 movement = new Vector3();
    float moveAngle = 0f;

    if (keysPressed.Count > 0)
    {
      // transform.forward = cam.transform.forward;
      transform.localRotation = Quaternion.AngleAxis(camSnapAngle, Vector3.up);

      switch (keysPressed[keysPressed.Count - 1])
      {
        case 'w':
          moveAngle = 0;
          movement = transform.TransformDirection(Vector3.forward);
          break;
        case 's':
          moveAngle = 180;
          movement = transform.TransformDirection(Vector3.back);
          break;
        case 'a':
          moveAngle = -90;
          movement = transform.TransformDirection(Vector3.left);
          break;
        case 'd':
          moveAngle = 90;
          movement = transform.TransformDirection(Vector3.right);
          break;
      }

      transform.localRotation = Quaternion.AngleAxis(camSnapAngle + moveAngle, Vector3.up);
    }

    ctrlr.SimpleMove(speed * movement);

    if (movement.x == 0f && movement.z == 0f)
    {
      anmtr.Play("Idle");
    }
    else
    {
      anmtr.Play("Run");
    }

  }
}
