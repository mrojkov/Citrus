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
				Padding = new Thickness { Left = 4 },
				MinMaxHeight = Metrics.ToolbarHeight,
				MinWidth = TimelineMetrics.ToolbarMinWidth,
				Presenter = new DelegatePresenter<Widget>(Render),
				Layout = new HBoxLayout { Spacing = 2, CellDefaults = new LayoutCell(Alignment.Center) },
				Nodes = {
					CreateAnimationModeButton(),
					CreateAutoKeyframesButton(),
					CreateNewFolderButton(),
					CreateAnimationIndicator(),
					new Widget(),
					CreateExitButton(),
					CreateEyeButton(),
					CreateLockButton()
				}
			};
		}
		
		void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, ColorTheme.Current.Toolbar.Background); 
		}

		ToolbarButton CreateAnimationModeButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.AnimationMode")) { Tip = "Animation mode" };
			button.AddChangeWatcher(() => UserPreferences.Instance.AnimationMode, i => button.Checked = i);
			button.Clicked += () => {
				UserPreferences.Instance.AnimationMode = !UserPreferences.Instance.AnimationMode;
			};
			return button;
		}

		Widget CreateAnimationIndicator()
		{
			var t = new SimpleText { Padding = new Thickness(4, 0) };
			Action f = () => {
				var distance = Timeline.Instance.Ruler.MeasuredFrameDistance;
				t.Text = (distance == 0) ? 
					$"Col : {Document.Current.Container.AnimationFrame}" :
					$"Col : {Document.Current.Container.AnimationFrame} {Timeline.Instance.Ruler.MeasuredFrameDistance:+#;-#;0}";
			};
			t.AddChangeWatcher(() => Document.Current.Container.AnimationFrame, _ => f());
			t.AddChangeWatcher(() => Timeline.Instance.Ruler.MeasuredFrameDistance, _ => f());
			return t;
		}

		ToolbarButton CreateAutoKeyframesButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.Key")) { Tip = "Automatic keyframes" };
			button.AddChangeWatcher(() => UserPreferences.Instance.AutoKeyframes, i => button.Checked = i);
			button.Clicked += () => {
				UserPreferences.Instance.AutoKeyframes = !UserPreferences.Instance.AutoKeyframes;
			};
			return button;
		}

		ToolbarButton CreateNewFolderButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Tools.NewFolder")) { Tip = "Create folder" };
			button.Clicked += () => {
				var doc = Document.Current;
				var newFolder = new Folder { Id = "Folder" };
				Core.Operations.InsertFolderItem.Perform(newFolder);
				Core.Operations.ClearRowSelection.Perform();
				Core.Operations.SelectRow.Perform(Document.Current.GetRowForObject(newFolder));
			};
			return button;
		}

		ToolbarButton CreateExitButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.ExitContainer")) { Tip = "Exit current container (backspace)" };
			button.Clicked += Core.Operations.LeaveNode.Perform;
			button.Updating += _ => button.Enabled = Core.Operations.LeaveNode.IsAllowed();
			return button;
		}

		ToolbarButton CreateEyeButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.Eye")) { Tip = "Show widgets" };
			button.Clicked += () => {
				var visibility = NodeVisibility.Hidden;
				if (Document.Current.Container.Nodes.All(i => i.EditorState().Visibility == NodeVisibility.Hidden)) {
					visibility = NodeVisibility.Shown;
				} else if (Document.Current.Container.Nodes.All(i => i.EditorState().Visibility == NodeVisibility.Shown)) {
					visibility = NodeVisibility.Default;
				}
				foreach (var node in Document.Current.Container.Nodes) {
					Core.Operations.SetProperty.Perform(node.EditorState(), nameof(NodeEditorState.Visibility), visibility);
				}
			};
			return button;
		}

		ToolbarButton CreateLockButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.Lock")) { Tip = "Lock widgets" };
			button.Clicked += () => {
				var locked = Document.Current.Container.Nodes.All(i => !i.EditorState().Locked);
				foreach (var node in Document.Current.Container.Nodes) {
					Core.Operations.SetProperty.Perform(node.EditorState(), nameof(NodeEditorState.Locked), locked);
				}
			};
			return button;
		}
	}
}

