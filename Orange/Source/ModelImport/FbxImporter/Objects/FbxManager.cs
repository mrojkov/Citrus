using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Orange.FbxImporter
{
	public class FbxManager : FbxObject
	{
		private List<FbxScene> createdScenes = new List<FbxScene>();

		public static FbxManager Create()
		{
			return new FbxManager(FbxCreateManager());
		}

		private FbxManager(IntPtr ptr) : base(ptr)
		{
			if (ptr == IntPtr.Zero) {
				throw new FbxImportException("An error has occured while initializing FbxSdk manager");
			}
		}

		[HandleProcessCorruptedStateExceptions]
		public FbxScene LoadScene(FbxImportOptions options)
		{
			var native = FbxManagerLoadScene(NativePtr, new StringBuilder(options.Path));
			if (native == IntPtr.Zero) {
				throw new FbxImportException("An error has occured while loading scene");
			}
			var scene = new FbxScene(native);
			createdScenes.Add(scene);
			return scene;
		}

		public void Destroy()
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
