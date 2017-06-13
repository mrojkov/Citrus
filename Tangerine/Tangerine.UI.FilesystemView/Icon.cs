using System.IO;
using Lime;

namespace Tangerine.UI.FilesystemView
{
	public class Icon : Widget
	{
		public string FilesystemPath;
		public Icon(string path)
		{
			FilesystemPath = path;
			MinMaxSize = new Vector2(200, 16);
			Layout = new HBoxLayout();
			Nodes.Add(new Image {
				LayoutCell = new LayoutCell
				{
					Stretch = Vector2.Zero,
					Alignment = new Alignment { X = HAlignment.Right, Y = VAlignment.Center }
				},
				MinMaxSize = new Vector2(16, 16),
				Texture = SystemIconTextureProvider.Instance.GetTexture(path),
			});
			Nodes.Add(new SimpleText {
				Text = Path.GetFileName(path),
				LayoutCell = new LayoutCell {
					Alignment = new Alignment { X = HAlignment.Right, Y = VAlignment.Bottom }
				}
			});
			Padding = new Thickness(5.0f);
			HitTestTarget = true;
		}
	}
}