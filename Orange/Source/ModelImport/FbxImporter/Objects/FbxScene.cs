using System;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class FbxScene : FbxObject
	{
		public FbxNode Root { get; }

		public FbxSceneAnimations Animations { get; }

		public FbxScene(IntPtr ptr) : base(ptr)
		{
			var r = FbxSceneGetRootNode(NativePtr);
			if (r == IntPtr.Zero) {
				throw new FbxImportException("An error has occured while parsing root node");
			}
			Root = new FbxNode(r);
			Animations = new FbxSceneAnimations(ptr);
		}

		#region PInvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxSceneGetRootNode(IntPtr scene);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FbxSceneDestroy(IntPtr scene);

		#endregion
	}
}
