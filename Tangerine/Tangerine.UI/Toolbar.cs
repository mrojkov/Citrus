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

		public Toolbar(Widget widget)
		{
			this.widget = widget;
			DecorateToolbar(widget);
			widget.Tasks.Add(new Property<int>(() => Version).DistinctUntilChanged().Consume(_ => Rebuild()));
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
			widget.Layout = new FlowLayout { Spacing = 2 };
			widget.CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				Renderer.DrawVerticalGradientRect(Vector2.Zero, w.Size, ToolbarColors.Background);
			}));
		}
	}
}