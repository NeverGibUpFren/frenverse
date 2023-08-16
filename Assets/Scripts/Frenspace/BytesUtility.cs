using System;
using System.Linq;
using System.Text;
using Frenspace.Fren;
using GameStates;
using UnityEngine;

public static class BytesUtility {
  public static byte[] PackFren(PositionedFren fren) {
    return new byte[0]
    .Concat(BitConverter.GetBytes(Convert.ToUInt16(fren.ID)))

    .Concat(FromVector3(fren.position))

    .Concat(BitConverter.GetBytes(Convert.ToUInt16(fren.instanceId)))
    .Concat(new byte[] { (byte)fren.instance })

    .Concat(new byte[] { (byte)fren.movement })
    .Concat(new byte[] { (byte)fren.vehicle })

    .ToArray();
  }

  public static PositionedFren UnpackFren(byte[] bytes) {
    return new PositionedFren() {
      ID = BitConverter.ToUInt16(new ReadOnlySpan<byte>(bytes, 0, 2)),

      position = ToVector3(new ReadOnlySpan<byte>(bytes, 2, 12).ToArray()),

      instanceId = BitConverter.ToUInt16(new ReadOnlySpan<byte>(bytes, 14, 2)),
      instance = (InstanceState)bytes[16],

      movement = (MovementState)bytes[17],
      vehicle = (VehicleState)bytes[18]
    };
  }

  public static Vector3 ToVector3(byte[] bytes) {
    // TODO: not sure if span is the most efficient way of doing this
    return new Vector3(
        BitConverter.ToSingle(new ReadOnlySpan<byte>(bytes, 0, 4)),
        BitConverter.ToSingle(new ReadOnlySpan<byte>(bytes, 4, 4)),
        BitConverter.ToSingle(new ReadOnlySpan<byte>(bytes, 8, 4))
    );
  }

  public static byte[] FromVector3(Vector3 v) {
    var x = BitConverter.GetBytes(v.x);
    var y = BitConverter.GetBytes(v.y);
    var z = BitConverter.GetBytes(v.z);
    return x.Concat(y).Concat(z).ToArray();
  }

  public static string ToString(byte[] bytes) {
    return Encoding.UTF8.GetString(bytes);
  }

  public static byte[] FromString(string s) {
    return Encoding.UTF8.GetBytes(s);
  }

  public static void LogBytes(ReadOnlySpan<byte> bytes) {
    char[] c = new char[bytes.Length * 2];

    byte b;

    for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx) {
      b = ((byte)(bytes[bx] >> 4));
      c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

      b = ((byte)(bytes[bx] & 0x0F));
      c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
    }

    Debug.Log(new string(c));
  }

}
