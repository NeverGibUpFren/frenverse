using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frenspace
{
  public static class Utility
  {
    public static void Defer(this MonoBehaviour mb, System.Action f)
    {
      mb.Invoke(() => f(), 0.01f);
    }

    public static void Invoke(this MonoBehaviour mb, System.Action f, float seconds)
    {
      mb.StartCoroutine(InvokeRoutine(f, seconds));
    }

    private static IEnumerator InvokeRoutine(System.Action f, float seconds)
    {
      yield return new WaitForSeconds(seconds);
      f();
    }

  }

}