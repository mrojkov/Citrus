using Lime;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;

namespace Tangerine
{
	public static class DisplayResolutions
	{
		public static IList<ResolutionInfo> Items { get; }
		static DisplayResolutions()
		{
			Items = new List<ResolutionInfo>();
			Items.Add(new ResolutionInfo("iPhone5 Portrait", "@Portrait", new Vector2(640, 1136)));
			Items.Add(new ResolutionInfo("iPhone5 Landscape", "@Landscape", new Vector2(1136, 640)));
			Items.Add(new ResolutionInfo("iPad Portrait", "@iPadPortrait", new Vector2(768, 1024)));
			Items.Add(new ResolutionInfo("iPad Landscape", "@iPadLandscape", new Vector2(1024, 768)));
		}

		private static void SetMarker(Node rootNode, string markerId)
		{
			foreach (var node in rootNode.Nodes) {
				var marker = node.Markers.FirstOrDefault(m => m.Id == markerId);
				if (marker != null) {
					UI.Timeline.Operations.SetCurrentColumn.Perform(marker.Frame, node);
				}
				SetMarker(node, markerId);
			}
		}

		public static void SetResolution(ResolutionInfo resolution)
		{
			Document.Current.History.BeginTransaction();
			Core.Operations.SetProperty.Perform(Document.Current.RootNode, nameof(Widget.Size), resolution.Size);
			SetMarker(Document.Current.RootNode, resolution.MarkerId);
			Document.Current.History.EndTransaction();
		}
	}

	public class ResolutionInfo
	{
		public string Name { get; set; }
		public string MarkerId { get; set; }
		public Vector2 Size { get; set; }

		public ResolutionInfo(string name, string markerId, Vector2 size)
		{
			Name = name;
			MarkerId = markerId;
			Size = size;
		}
	}
}