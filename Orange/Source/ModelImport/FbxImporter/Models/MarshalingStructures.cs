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
}
