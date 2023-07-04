using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class TV : MonoBehaviour
{

  private VideoPlayer vp;

  void Start()
  {
    vp = GetComponent<VideoPlayer>();
    vp.url = "https://rr5---sn-4g5e6ns6.googlevideo.com/videoplayback?expire=1688229115&ei=mwCgZIL9Bs-M6dsP2umDiAk&ip=2a02%3A3037%3A210%3A2faa%3A5472%3Af678%3A4f0a%3A261d&id=o-AJRi7j1gfck_xnQwDCHtXU-j0Mx6aGZ3oAYuDl2U7UVa&itag=242&aitags=133%2C134%2C135%2C136%2C137%2C160%2C242%2C243%2C244%2C247%2C248%2C278%2C394%2C395%2C396%2C397%2C398%2C399&source=youtube&requiressl=yes&mh=Md&mm=31%2C29&mn=sn-4g5e6ns6%2Csn-4g5edndk&ms=au%2Crdu&mv=m&mvi=5&pl=39&initcwndbps=1188750&spc=Ul2Sq0BCfEZhNEy9wyF1AUQbXVc1fd5XYXMtvkOuQw&vprv=1&svpuc=1&mime=video%2Fwebm&ns=YXOLmjj52EpW7pQbpWfU51IO&gir=yes&clen=1317459&dur=81.314&lmt=1528904455016228&mt=1688207135&fvip=2&keepalive=yes&fexp=24007246%2C24363393&beids=24350017&c=WEB&n=0QQ2zpq6_I_kDz4&sparams=expire%2Cei%2Cip%2Cid%2Caitags%2Csource%2Crequiressl%2Cspc%2Cvprv%2Csvpuc%2Cmime%2Cns%2Cgir%2Cclen%2Cdur%2Clmt&sig=AOq0QJ8wRQIhANSmwBoEkF6a4TdlTm6kCvbGiXjn9-4LiJ8Yb_MQ6xOiAiA_7U7Bu5sXa0yUXK8asfef_lUsIiatG9aFKTAz0AFe0A%3D%3D&lsparams=mh%2Cmm%2Cmn%2Cms%2Cmv%2Cmvi%2Cpl%2Cinitcwndbps&lsig=AG3C_xAwRQIhAOIwW8NXNAEIUqlRM16uxwKkkx7EfoY4FJgTZIVDbG_dAiBuOEs5zI9NYybRVQPTTKUOxE18bUzvybco6JQ6-fZOOA%3D%3D";
    vp.audioOutputMode = VideoAudioOutputMode.Direct;
    // vp.EnableAudioTrack(0, true);
    vp.SetDirectAudioVolume(0, 0.1f);
    // vp.Prepare();
  }

}
