using Lime;

namespace Tangerine.Core
{
	public enum NodeVisibility
	{
		Default = 0,
		Hidden = 1,
		Shown = 2,
	}

	public class NodeEditorState
	{
		readonly Node node;
		public string ThumbnailData { get; set; }
		public NodeVisibility Visibility
		{
			get
			{
				if (node.GetTangerineFlag(TangerineFlags.Shown)) {
					return NodeVisibility.Shown;
				} else if (node.GetTangerineFlag(TangerineFlags.Hidden)) {
					return NodeVisibility.Hidden;
				} else {
					return NodeVisibility.Default;
				}
			}
			set
			{
				node.SetTangerineFlag(TangerineFlags.Shown, value == NodeVisibility.Shown);
				node.SetTangerineFlag(TangerineFlags.Hidden, value == NodeVisibility.Hidden);
			}
		}
		public bool Locked { get { return node.GetTangerineFlag(TangerineFlags.Locked); } set { node.SetTangerineFlag(TangerineFlags.Locked, value); } }
		public bool PropertiesExpanded { get { return node.GetTangerineFlag(TangerineFlags.PropertiesExpanded); } set { node.SetTangerineFlag(TangerineFlags.PropertiesExpanded, value); } }
		public bool ChildrenExpanded { get { return node.GetTangerineFlag(TangerineFlags.ChildrenExpanded); } set { node.SetTangerineFlag(TangerineFlags.ChildrenExpanded, value); } }

		public int ColorIndex
		{
			get
			{
				return
					(node.GetTangerineFlag(TangerineFlags.ColorBit1) ? 1 : 0) |
					(node.GetTangerineFlag(TangerineFlags.ColorBit2) ? 2 : 0) |
					(node.GetTangerineFlag(TangerineFlags.ColorBit3) ? 4 : 0);
			}
			set
			{
				node.SetTangerineFlag(TangerineFlags.ColorBit1, (value & 1) == 1);
				node.SetTangerineFlag(TangerineFlags.ColorBit2, ((value >>= 1) & 1) == 1);
				node.SetTangerineFlag(TangerineFlags.ColorBit3, ((value >>= 1) & 1) == 1);
			}
		}

		Folder rootFolder;
		public Folder RootFolder => rootFolder ?? (rootFolder = Folder.BuildTree(node));

		public NodeEditorState(Node node)
		{
			this.node = node;
		}
	}

	public static class NodeExtensions
	{
		public static NodeEditorState EditorState(this Node node)
		{
			if (node.UserData == null) {
				node.UserData = new NodeEditorState(node);
			}
			return (NodeEditorState)node.UserData;
		}

		public static int CollectionIndex(this Node node)
		{
			return node.Parent.Nodes.IndexOf(node);
		}

		public static Folder RootFolder(this Node node)
		{
			return EditorState(node).RootFolder;
		}

		public static void SyncFolderDescriptorsAndNodes(this Node node)
		{
			EditorState(node).RootFolder.SyncDescriptorsAndNodes(node);
		}

		public static bool IsCopyPasteAllowed(this Node node)
		{
			return NodeCompositionValidator.IsCopyPasteAllowed(node.GetType());
		}
	}
}
