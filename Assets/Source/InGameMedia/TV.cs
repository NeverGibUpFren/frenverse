using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class TV : MonoBehaviour
{

  private VideoPlayer vp;

  void Start()
  {
    vp = GetComponent<VideoPlayer>();
    vp.url = "";
    vp.Prepare();
  }

}
