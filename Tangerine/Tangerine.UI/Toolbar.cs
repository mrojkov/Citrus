using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;
using Tangerine.UI;

namespace Tangerine
{
	public class Toolbar : VersionedCollection<ICommand>
	{
		readonly Widget widget;

		public Toolbar(Widget container)
		{
			widget = new Widget();
			container.Nodes.Add(widget);
			DecorateToolbar(widget);
			container.Tasks.Add(new Property<int>(() => Version).DistinctUntilChanged().Consume(_ => Rebuild()));
			widget.Tasks.AddLoop(() => {
				for (int i = 0; i < Count; i++) {
					var b = (ToolbarButton)widget.Nodes[i];
					var c = this[i];
					b.Enabled = c.Enabled;
					b.Tip = c.Text;
				}
			});
		}

		void Rebuild()
		{
			widget.Nodes.Clear();
			foreach (var c in this) {
				var b = new ToolbarButton(c.Icon ?? new SerializableTexture());
				b.Clicked += c.Execute;
				widget.Nodes.Add(b);
			}
		}

		static void DecorateToolbar(Widget widget)
		{
			widget.Padding = new Thickness(4, 0);
			widget.LayoutCell = new LayoutCell { StretchX = 0 };
			widget.Layout = new HBoxLayout { Spacing = 2 };
			widget.CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				if (w.Width > 0) {
					Renderer.DrawVerticalGradientRect(new Vector2(2, 0), w.Size - new Vector2(2, 0), ToolbarColors.Background);
					Renderer.DrawRectOutline(new Vector2(2, 0), w.Size - new Vector2(2, 0), ToolbarColors.Border);
				}
			}));
		}
	}
}