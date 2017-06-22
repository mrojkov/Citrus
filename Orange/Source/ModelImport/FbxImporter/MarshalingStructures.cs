using Lime;
using System.Linq;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	[StructLayout(LayoutKind.Sequential)]
	public class Vec2
	{
		[MarshalAs(UnmanagedType.R4)]
		public float V1;

		[MarshalAs(UnmanagedType.R4)]
		public float V2;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class Vec3 : Vec2
	{
		[MarshalAs(UnmanagedType.R4)]
		public float V3;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class Vec4 : Vec3
	{
		[MarshalAs(UnmanagedType.R4)]
		public float V4;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class Mat4x4
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public double[] Data;

		public Matrix44 ToLime()
		{
			var values = Data.Select(v => (float)v).ToArray();
			return new Matrix44(
				values[0], values[1], values[2], values[3],
				values[4], values[5], values[6], values[7],
				values[8], values[9], values[10], values[11],
				values[12], values[13], values[14], values[15]
			);
		}
	}

	public static class VectorExtensions
	{
		public static Quaternion toLimeQuaternion(this Vec4 vector)
		{
			return new Quaternion(vector.V1, vector.V2, vector.V3, vector.V4);
		}

		public static Color4 toLimeColor(this Vec4 vector)
		{
			return Color4.FromFloats(vector.V1, vector.V2, vector.V3, vector.V4);
		}

		public static Vector4 toLime(this Vec4 vector)
		{
			return new Vector4(vector.V1, vector.V2, vector.V3, vector.V4);
		}

		public static Vector3 toLime(this Vec3 vector)
		{
			return new Vector3(vector.V1, vector.V2, vector.V3);
		}

		public static Vector2 toLime(this Vec2 vector)
		{
			return new Vector2(vector.V1, vector.V2);
		}
	}
}
