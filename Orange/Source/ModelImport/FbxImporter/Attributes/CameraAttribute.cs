using System;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class CameraAttribute : NodeAttribute
	{
		public override FbxNodeType Type { get; } = FbxNodeType.Camera;

		public string Name { get; }

		public float FieldOfView { get; }

		public float AspectRatio { get; }

		public float NearClipPlane { get; }

		public float FarClipPlane { get; }

		public CameraAttribute(IntPtr ptr) : base(ptr)
		{
			var native = FbxNodeGetCameraAttribute(ptr);
			if (native == IntPtr.Zero) {
				throw new FbxAtributeImportException(Type);
			}
			var cam = native.ToStruct<Camera>();
			Name = cam.name;
			FieldOfView = cam.fieldOfView;
			AspectRatio = cam.aspectRatio;
			NearClipPlane = cam.clipPlaneNear;
			FarClipPlane = cam.clipPlaneFar;
		}

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxNodeGetCameraAttribute(IntPtr node);

		#endregion

		#region MarshalingStructures

		[StructLayout(LayoutKind.Sequential, CharSet = ImportConfig.Charset)]
		private class Camera
		{
			public string name;

			[MarshalAs(UnmanagedType.R4)]
			public float fieldOfView;

			[MarshalAs(UnmanagedType.R4)]
			public float clipPlaneFar;

			[MarshalAs(UnmanagedType.R4)]
			public float clipPlaneNear;

			[MarshalAs(UnmanagedType.R4)]
			public float aspectRatio;
		}

		#endregion
	}
}