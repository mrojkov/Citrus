using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Orange.FbxImporter
{
	public class Scene : FbxObject
	{
		#region PInvokes
		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetRootNode(IntPtr scene);
		#endregion

		public Scene(IntPtr ptr) : base(ptr)
		{
			try {
				root = new Node(GetRootNode(NativePtr));
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

		public override string ToString()
		{
			return "Scene: \n" + Root.ToString(0);
		}
	}
}
