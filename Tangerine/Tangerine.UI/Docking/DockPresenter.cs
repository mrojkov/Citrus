using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Docking
{
	public class DockingPresenter : SyncDelegatePresenter<Widget>
	{
		public DockingPresenter() : base(Render) { }

		private static void Render(Widget widget)
		{
			var comp = widget.Components.Get<RequestedDockingComponent>();
			if (!comp.Bounds.HasValue) return;
			widget.PrepareRendererState();
			Renderer.DrawRectOutline(comp.Bounds.Value.A + Vector2.One, comp.Bounds.Value.B - Vector2.One, ColorTheme.Current.Docking.DragRectagleOutline, 2);
			Renderer.DrawRect(comp.Bounds.Value.A + Vector2.One, comp.Bounds.Value.B - Vector2.One, ColorTheme.Current.Docking.DragRectagleOutline.Transparentify(0.8f));
		}
	}
}
