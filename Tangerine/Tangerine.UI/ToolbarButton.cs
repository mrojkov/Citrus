using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Lime;
using ProtoBuf;

namespace Tangerine.UI
{
	public class ToolbarButton : Button
	{
		private bool @checked;
		private ITexture texture;

		public bool Checked
		{
			get { return @checked; }
			set
			{
				if (@checked != value) {
					@checked = value;
					Window.Current.Invalidate();
				}
			}
		}

		public override ITexture Texture
		{
			get { return texture; }
			set
			{
				if (texture != value) {
					texture = value;
					Window.Current.Invalidate();
				}
			}
		}

		public ToolbarButton()
		{
			Nodes.Clear();
			Padding = new Thickness(3);
			Size = MinMaxSize = new Vector2(22, 22);
			CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				if (Checked) {
					Renderer.DrawRect(Vector2.One, Size - 2 * Vector2.One, Colors.ToolbarButtonCheckedBackground);
				}
				Renderer.DrawSprite(Texture, GlobalColor, ContentPosition, ContentSize, Vector2.Zero, Vector2.One);
				if (Checked) {
					Renderer.DrawRectOutline(Vector2.One, Size - 2 * Vector2.One, Colors.ToolbarButtonCheckedBorder, 1);
				}
			}));
		}

		public ToolbarButton(ITexture texture) : this()
		{
			Texture = texture;
		}
	}	
}