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

		private Manager(IntPtr ptr) : base(ptr)
		{
			if (ptr == IntPtr.Zero) {
				throw new FbxImportException("An error has occured while initializing FbxSdk manager");
			}
		}

		[HandleProcessCorruptedStateExceptions]
		public Scene LoadScene(string fileName)
		{
			var native = FbxManagerLoadScene(NativePtr, new StringBuilder(fileName));
			if (native == IntPtr.Zero) {
				throw new FbxImportException("An error has occured while loading scene");
			}
			var scene = new Scene(native);
			createdScenes.Add(scene);
			return scene;
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
