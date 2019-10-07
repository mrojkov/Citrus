using System.Collections.Generic;
using System.IO;
using Lime;
using Tangerine.Core;
using Yuzu;

namespace Tangerine.UI.FilesystemView
{
	public enum SplitterType
	{
		Horizontal,
		Vertical
	}

	public class ViewNodeComponent : Lime.NodeComponent
	{
		public ViewNode ViewNode;
	}

	public class ViewNode
	{
		[YuzuOptional]
		public List<ViewNode> Children { get; private set; } = new List<ViewNode>();

		public ViewNode Parent;

		public Widget Widget;
	}

	public class SplitterNode : ViewNode
	{
		[YuzuOptional]
		public SplitterType Type;

		[YuzuOptional]
		public List<float> Stretches = new List<float>();
	}

	public class FSViewNode : ViewNode
	{
		[YuzuOptional]
		public string Path = Project.Current.AssetsDirectory ?? Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());

		[YuzuOptional]
		public bool ShowCookingRulesEditor = false;

		[YuzuOptional]
		public bool ShowSelectionPreview = true;

		[YuzuOptional]
		public List<float> CookingRulesSplitterStretches = new List<float>();

		[YuzuOptional]
		public List<float> SelectionPreviewSplitterStretches = new List<float>();

		[YuzuOptional]
		public SortType SortType;
	}
}
