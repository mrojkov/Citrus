using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine.UI.Timeline
{
	public class Toolbar
	{
		public Widget RootWidget { get; private set; }
		
		public Toolbar()
		{
			RootWidget = new Widget {
				MinMaxHeight = Metrics.ToolbarHeight,
				MinWidth = Metrics.ToolbarMinWidth,
				Presenter = new DelegatePresenter<Widget>(Render),
				Layout = new HBoxLayout(),
				Nodes = {
					new Widget(),
					CreateEyeButton(),
					CreateLockButton()
				}
			};
		}
		
		void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawVerticalGradientRect(Vector2.Zero, widget.Size, Colors.Toolbar); 
		}

		BitmapButton CreateEyeButton()
		{
			var button = new BitmapButton(IconPool.GetTexture("Timeline.Eye"), Metrics.IconSize) {
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			button.Clicked += () => {
				var visibility = NodeVisibility.Hide;
				if (Document.Current.Container.Nodes.All(i => i.EditorState().Visibility == NodeVisibility.Hide)) {
					visibility = NodeVisibility.Show;
				} else if (Document.Current.Container.Nodes.All(i => i.EditorState().Visibility == NodeVisibility.Show)) {
					visibility = NodeVisibility.Default;
				}
				foreach (var node in Document.Current.Container.Nodes) {
					Core.Operations.SetGenericProperty<NodeVisibility>.Perform(
						() => node.EditorState().Visibility, value => node.EditorState().Visibility = value,
						visibility
					);
				}
			};
			return button;
		}

		BitmapButton CreateLockButton()
		{
			var button = new BitmapButton(IconPool.GetTexture("Timeline.Lock"), Metrics.IconSize) {
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			button.Clicked += () => {
				var locked = Document.Current.Container.Nodes.All(i => !i.EditorState().Locked);
				foreach (var node in Document.Current.Container.Nodes) {
					Core.Operations.SetGenericProperty<bool>.Perform(
						() => node.EditorState().Locked, value => node.EditorState().Locked = value,
						locked
					);
				}
			};
			return button;
		}
	}
}

