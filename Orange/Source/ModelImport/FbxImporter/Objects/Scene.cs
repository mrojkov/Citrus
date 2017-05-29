using System;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class Scene : FbxObject
	{
		public Scene(IntPtr ptr) : base(ptr)
		{
			try {
				root = new Node(FbxSceneGetRootNode(NativePtr));
				Animations = new Animation(ptr);
			} catch(Exception e) {
				Console.WriteLine(e.Message);
			}
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

		public override string ToString()
		{
			return "Scene: \n" + Root.ToString(0);
		}

		#region PInvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxSceneGetRootNode(IntPtr scene);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FbxSceneDestroy(IntPtr scene);

		#endregion

	}
}
