using System;

namespace Orange.FbxImporter
{
	public class FbxObject
	{
		public IntPtr NativePtr { get; }

		public FbxObject(IntPtr ptr)
		{
			NativePtr = ptr;	
		}
	}
}
