using UnityEngine;

[ExecuteInEditMode]
public class SkyboxHandler : MonoBehaviour
{
  public float rotationSpeed = 1f;


  void Update()
  {
    RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
  }

}