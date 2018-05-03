using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core.Components;

namespace Tangerine.Core
{
	public struct RowLocation
	{
		public Row ParentRow;
		public int Index;

		public RowLocation(Row parentRow, int index)
		{
			ParentRow = parentRow;
			Index = index;
		}
	}

	public class Row
	{
		public int Index { get; set; }
		public bool Selected => SelectCounter != 0;
		public int SelectCounter { get; set; }
		public bool CanHaveChildren { get; set; }
		public Row Parent { get; set; }
		public readonly List<Row> Rows = new List<Row>();
		public readonly ComponentCollection<Component> Components = new ComponentCollection<Component>();

		public bool IsCopyPasteAllowed()
		{
			return Components.Get<NodeRow>()?.Node?.IsCopyPasteAllowed() ?? true;
		}

		public static FolderItemLocation GetFolderItemLocation(Row r)
		{
			var fi = GetFolderItem(r);
			return fi == null ? new FolderItemLocation(null, -1) : Document.Current.Container.RootFolder().Find(fi);
		}

		public static IFolderItem GetFolderItem(Row r)
		{
			return (r.Components.Get<NodeRow>()?.Node as IFolderItem) ?? r.Components.Get<FolderRow>()?.Folder;
		}
	}
}
