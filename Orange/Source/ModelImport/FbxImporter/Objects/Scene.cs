using System;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class Scene : FbxObject
	{
		public Scene(IntPtr ptr) : base(ptr)
		{
			var r = FbxSceneGetRootNode (NativePtr);
			root = new Node(r);
			Animations = new Animation(ptr);
		}

		private Node root;
		public Node Root
		{
			get 
			{
				return root;
			}
		}

		public Animation Animations { get; set; } 

		#region PInvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxSceneGetRootNode(IntPtr scene);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FbxSceneDestroy(IntPtr scene);

		#endregion

	}
}
