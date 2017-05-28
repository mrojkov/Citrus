using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Orange.FbxImporter
{
	public class Manager : FbxObject
	{
		public static Manager instance;
		public static Manager Instance
		{
			get 
			{
				if (instance == null) {
					instance = new Manager(FbxCreateManager());
				}
				return instance;
			}
		}

		private Manager(IntPtr ptr) : base(ptr)
		{ }

		[HandleProcessCorruptedStateExceptions]
		public Scene LoadScene(string fileName)
		{
			try {
				return new Scene(FbxManagerLoadScene(NativePtr, new StringBuilder(fileName)));
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
