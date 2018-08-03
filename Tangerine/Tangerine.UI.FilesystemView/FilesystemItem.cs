using System.IO;
using Lime;

namespace Tangerine.UI.FilesystemView
{
	public class FilesystemItem : Widget
	{
		public string FilesystemPath;
		public const float IconSize = 16;
		public const float ItemPadding = 2.0f;
		public const float ItemWidth = 200.0f;
		public const float Spacing = 2.0f;
		public FilesystemItem(string path, bool visibalFill = false)
		{
			FilesystemPath = path;
			this.Input.AcceptMouseThroughDescendants = true;
			SimpleText text = null;
			MinMaxSize = new Vector2(ItemWidth, IconSize);
			Layout = new HBoxLayout { Spacing = Spacing };
			Padding = new Thickness(2);
			HitTestTarget = true;
			var isRoot = false;
			if (new DirectoryInfo(FilesystemPath).Parent == null) {
				isRoot = true;
			}

			Nodes.AddRange(
				new Image {
					LayoutCell = new LayoutCell {
						Stretch = Vector2.Zero,
						Alignment = new Alignment { X = HAlignment.Right, Y = VAlignment.Center }
					},
					MinMaxSize = new Vector2(IconSize, IconSize),
					Texture = SystemIconTextureProvider.Instance.GetTexture(FilesystemPath),
				},
				(text = new ThemedSimpleText {
					ForceUncutText = false,
					OverflowMode = TextOverflowMode.Ellipsis,
					Text = isRoot ?
						FilesystemPath.Remove(FilesystemPath.Length - 1) :
						Path.GetFileName(FilesystemPath),
					LayoutCell = new LayoutCell {
						Alignment = new Alignment { X = HAlignment.Right, Y = VAlignment.Bottom }
					}
				}),
				new Widget {
					LayoutCell = new LayoutCell {
						StretchX = float.MaxValue
					},
					MinWidth = 0,
					HitTestTarget = true
				}
			);
			text.Width = text.MinMaxWidth = Mathf.Min(ItemWidth - (IconSize + ItemPadding * 2 + Spacing + 2), text.MeasureUncutText().X);

			if (visibalFill) {
				CompoundPresenter.Add(new DelegatePresenter<Widget>(_ => {
					if (IsMouseOverThisOrDescendant()) {
						PrepareRendererState();
						Renderer.DrawRect(Vector2.Zero, Size, Theme.Colors.HoveredBackground);
						if (Input.WasMousePressed()) {
							Renderer.DrawRectOutline(Vector2.Zero, Size, Theme.Colors.SelectedBorder);
						}
					}
				}));
			}
		}
	}
}
