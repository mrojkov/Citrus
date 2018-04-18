using Lime;
using Tangerine.Core;
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
			container.AddChangeWatcher(() => Version, _ => Rebuild());
		}

		public void Rebuild()
		{
			widget.Nodes.Clear();
			foreach (var c in this) {
				var b = new ToolbarButton(c.Icon ?? new SerializableTexture());
				b.Clicked += () => CommandQueue.Instance.Add((Command)c);
				b.Updating += _ => {
					b.Enabled = c.Enabled;
					b.Tip = c.Text;
				};
				widget.Nodes.Add(b);
			}
		}

		static void DecorateToolbar(Widget widget)
		{
			widget.MinMaxHeight = Metrics.ToolbarHeight;
			widget.LayoutCell = new LayoutCell { StretchX = 0 };
			widget.Layout = new HBoxLayout { Spacing = 1, CellDefaults = new LayoutCell(Alignment.Center) };
			widget.CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
				if (w.Width > 0) {
					w.PrepareRendererState();
					Renderer.DrawRect(Vector2.Zero, w.Size, ColorTheme.Current.Toolbar.Background);
				}
			}));
		}
	}
}
