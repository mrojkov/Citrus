using System;

namespace Lime
{
	public class FolderBegin : Node
	{
		public bool Expanded { get; set; }

		public FolderBegin()
		{
			Expanded = true;
		}
	}

	public class FolderEnd : Node
	{
	}
}
