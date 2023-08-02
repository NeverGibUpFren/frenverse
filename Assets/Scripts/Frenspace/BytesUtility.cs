using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class BytesUtility
{

  public static Vector3 ToVector3(byte[] bytes)
  {
    // TODO: not sure if span is the most efficient way of doing this
    return new Vector3(
        BitConverter.ToSingle(new ReadOnlySpan<byte>(bytes, 0, 2)),
        BitConverter.ToSingle(new ReadOnlySpan<byte>(bytes, 2, 4)),
        BitConverter.ToSingle(new ReadOnlySpan<byte>(bytes, 4, 6))
    );
  }

  public static byte[] FromVector3(Vector3 v)
  {
    var x = BitConverter.GetBytes(v.x);
    var y = BitConverter.GetBytes(v.y);
    var z = BitConverter.GetBytes(v.z);
    return x.Concat(y).Concat(z).ToArray();
  }

  public static string ToString(byte[] bytes)
  {
    return Encoding.UTF8.GetString(bytes);
  }

  public static byte[] FromString(string s)
  {
    return Encoding.UTF8.GetBytes(s);
  }

  public static void ForEachChunk(ReadOnlySpan<byte> bytes, int chunkSize, Action<byte[], int> fn)
  {
    var idx = 0;
    var i = 0;
    var byteArr = bytes.ToArray();
    while (i < bytes.Length)
    {
      fn(new ReadOnlySpan<byte>(byteArr, i, chunkSize).ToArray(), idx);
      i += chunkSize;
      idx++;
    }
  }
}