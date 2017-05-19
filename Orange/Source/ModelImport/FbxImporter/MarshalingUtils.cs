using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Orange.FbxImporter
{

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

	[StructLayout(LayoutKind.Sequential)]
	public class Camera
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public string name;

		[MarshalAs(UnmanagedType.R4)]
		public float fieldOfView;

		[MarshalAs(UnmanagedType.R4)]
		public float ClipPlaneFar;

		[MarshalAs(UnmanagedType.R4)]
		public float ClipPlaneNear;

		[MarshalAs(UnmanagedType.R4)]
		public float AspectRatio;
	}
}
