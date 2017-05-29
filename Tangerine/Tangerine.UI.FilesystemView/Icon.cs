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
			MinSize = MaxSize = new Vector2(200, 16);
			Layout = new HBoxLayout();
			Nodes.Add(new Image
			{
				LayoutCell = new LayoutCell
				{
					Stretch = Vector2.Zero
				},
				MinSize = MaxSize = new Vector2(16, 16),
				Texture = SystemIconTextureProvider.Instance.GetTexture(path),
			});
			Nodes.Add(new SimpleText { Text = Path.GetFileName(path) });
			Padding = new Thickness(5.0f);
			HitTestTarget = true;
		}
	}
}