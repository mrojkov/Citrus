#if WIN
using static OculusWrap.OVRTypes;

namespace Lime.Oculus
{
	public static class OvrExtensions
	{
		public static OvrProvider.EyePose ToLime(this Posef pose)
		{
			return new OvrProvider.EyePose {
				Position = pose.Position.ToLime(),
				Orientation = pose.Orientation.ToLime()
			};
		}
		public static Quaternion ToLime(this Quaternionf q)
		{
			return new Quaternion(q.X, q.Y, q.Z, q.W);
		}

		public static Vector3 ToLime(this Vector3f v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}

		public static IntVector2 ToLime(this Sizei v)
		{
			return new IntVector2(v.Width, v.Height);
		}

		public static Matrix44 ToLime(this Matrix4f m)
		{
			return new Matrix44 {
				M11 = m.M11,
				M12 = m.M12,
				M13 = m.M13,
				M14 = m.M14,
				M21 = m.M21,
				M22 = m.M22,
				M23 = m.M23,
				M24 = m.M24,
				M31 = m.M31,
				M32 = m.M32,
				M33 = m.M33,
				M34 = m.M34,
				M41 = m.M41,
				M42 = m.M42,
				M43 = m.M43,
				M44 = m.M44,
			};
		}
	}
}
#endif
