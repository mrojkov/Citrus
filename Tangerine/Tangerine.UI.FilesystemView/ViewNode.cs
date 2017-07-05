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
		[YuzuRequired]
		public List<ViewNode> Children { get; private set; } = new List<ViewNode>();

		public ViewNode Parent;

		public Widget Widget;
	}

	public class SplitterNode : ViewNode
	{
		[YuzuRequired]
		public SplitterType Type;

		[YuzuRequired]
		public List<float> Stretches = new List<float>();
	}

	public class FSViewNode : ViewNode
	{
		[YuzuRequired]
		public string Path = Project.Current?.AssetsDirectory ?? Directory.GetCurrentDirectory();

		[YuzuRequired]
		public bool ShowCookingRulesEditor = false;

		[YuzuRequired]
		public bool ShowSelectionPreview = true;

		[YuzuRequired]
		public List<float> CookingRulesSplitterStretches = new List<float>();

		[YuzuRequired]
		public List<float> SelectionPreviewSplitterStretches = new List<float>();
	}
}