using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Orange.FbxImporter
{
	public class Manager : FbxObject
	{
		private List<Scene> createdScenes = new List<Scene>();

		public static Manager Create()
		{
			return new Manager(FbxCreateManager());
		}

		public Manager(IntPtr ptr) : base(ptr)
		{ }

		[HandleProcessCorruptedStateExceptions]
		public Scene LoadScene(string fileName)
		{
			try {
				var scene = new Scene(FbxManagerLoadScene(NativePtr, new StringBuilder(fileName)));
				createdScenes.Add(scene);
				return scene;
			} catch(Exception e) {
				throw new Exception(e.Message, e);
			}
		}

		~Manager()
		{
			FbxManagerDestroy(NativePtr);
		}

		#region PInvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxCreateManager();

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxManagerLoadScene(IntPtr manager, StringBuilder pFileName);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FbxManagerDestroy(IntPtr manager);

		#endregion
	}
}
