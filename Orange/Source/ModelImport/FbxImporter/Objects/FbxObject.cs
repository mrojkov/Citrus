using System;

namespace Orange.FbxImporter
{
	public class FbxObject
	{
		public IntPtr NativePtr { get; private set; }

		public FbxObject(IntPtr ptr)
		{
			NativePtr = ptr;	
		}
	}
}
