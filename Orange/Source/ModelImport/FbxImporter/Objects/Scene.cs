using System;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class Scene : FbxObject
	{
		public Node Root { get; }

		public SceneAnimations Animations { get; }

		public Scene(IntPtr ptr) : base(ptr)
		{
			var r = FbxSceneGetRootNode(NativePtr);
			if (r == IntPtr.Zero) {
				throw new FbxImportException("An error has occured while parsing root node");
			}
			Root = new Node(r);
			Animations = new SceneAnimations(ptr);
		}

		#region PInvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxSceneGetRootNode(IntPtr scene);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FbxSceneDestroy(IntPtr scene);

		#endregion
	}
}
