using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orange.FbxImporter
{
	public static class StringExtension 
	{
		public static string ToLevel(this string str, int level)
		{
			return string.Empty.PadRight(level * 4) + str;
		}
	}

	public class FbxObject
	{
		public IntPtr NativePtr { get; private set; }

		public FbxObject(IntPtr ptr)
		{
			NativePtr = ptr;	
		}

		public virtual string ToString(int level)
		{
			return nameof(FbxObject);
		}
	}
}
