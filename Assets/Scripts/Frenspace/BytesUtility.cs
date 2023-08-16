using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStates;
using UnityEngine;

public static class BytesUtility
{
  public static byte[] PackFren(FrenHandler.Fren fren)
  {
    return new byte[0]
    .Concat(BitConverter.GetBytes(Convert.ToUInt16(fren.ID)))
    .Concat(FromVector3(fren.go.transform.position))
    .Concat(BitConverter.GetBytes(Convert.ToUInt16(fren.instanceId)))
    .Concat(new byte[] { (byte)fren.instance })
    .Concat(new byte[] { (byte)fren.movement })
    .Concat(new byte[] { (byte)fren.vehicle })
    .ToArray();
  }

  public static FrenHandler.Fren UnpackFren(byte[] bytes)
  {
    var fren = new FrenHandler.Fren() { go = new GameObject() };

    fren.ID = BitConverter.ToUInt16(new ReadOnlySpan<byte>(bytes, 0, 2));

    fren.go.transform.position = ToVector3(new ReadOnlySpan<byte>(bytes, 2, 12).ToArray());

    fren.instanceId = BitConverter.ToUInt16(new ReadOnlySpan<byte>(bytes, 14, 2));
    fren.instance = (InstanceState)bytes[16];

    fren.movement = (MovementState)bytes[17];
    fren.vehicle = (VehicleState)bytes[18];

    return fren;
  }

  public static Vector3 ToVector3(byte[] bytes)
  {
    // TODO: not sure if span is the most efficient way of doing this
    return new Vector3(
        BitConverter.ToSingle(new ReadOnlySpan<byte>(bytes, 0, 4)),
        BitConverter.ToSingle(new ReadOnlySpan<byte>(bytes, 4, 4)),
        BitConverter.ToSingle(new ReadOnlySpan<byte>(bytes, 8, 4))
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

  public static void LogBytes(ReadOnlySpan<byte> bytes)
  {
    char[] c = new char[bytes.Length * 2];

    byte b;

    for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
    {
      b = ((byte)(bytes[bx] >> 4));
      c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

      b = ((byte)(bytes[bx] & 0x0F));
      c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
    }

    Debug.Log(new string(c));
  }

}
