using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
					instance = new Manager(CreateFbxManager());
				}
				return instance;
			}
		}

		private Manager(IntPtr ptr) : base(ptr)
		{ }

		public Scene LoadScene(string fileName) {
			return new Scene(LoadScene(NativePtr, new StringBuilder(fileName)));
		}

		#region PInvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr CreateFbxManager();

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr LoadScene(IntPtr manager, StringBuilder pFileName);

		#endregion
	}
}
