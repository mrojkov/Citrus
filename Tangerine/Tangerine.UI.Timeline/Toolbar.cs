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
				Padding = new Thickness(2, 0),
				MinMaxHeight = Metrics.ToolbarHeight,
				MinWidth = TimelineMetrics.ToolbarMinWidth,
				Presenter = new WidgetFlatFillPresenter(ColorTheme.Current.Toolbar.Background),
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center) },
				Nodes = {
					CreateAnimationModeButton(),
					CreateAutoKeyframesButton(),
					CreateNewFolderButton(),
					CreateCurveEditorButton(),
					CreateAnimationIndicator(),
					new Widget(),
					CreateExitButton(),
					CreateLockAnimationButton(),
					CreateEyeButton(),
					CreateLockButton()
				}
			};
		}

		TimelineUserPreferences UserPreferences => TimelineUserPreferences.Instance;
		CoreUserPreferences CoreUserPreferences => Core.CoreUserPreferences.Instance;

		ToolbarButton CreateAnimationModeButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.AnimationMode")) { Tip = "Animation mode" };
			button.AddChangeWatcher(() => CoreUserPreferences.AnimationMode, i => button.Checked = i);
			button.Clicked += () => CoreUserPreferences.AnimationMode = !CoreUserPreferences.AnimationMode;
			return button;
		}

		ToolbarButton CreateCurveEditorButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.Curve")) { Tip = "Edit curves" };
			button.AddChangeWatcher(() => UserPreferences.EditCurves, i => button.Checked = i);
			button.Clicked += () => UserPreferences.EditCurves = !UserPreferences.EditCurves;
			return button;
		}

		Widget CreateAnimationIndicator()
		{
			var t = new ThemedSimpleText { Padding = new Thickness(4, 0), MinWidth = 100 };
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
			button.AddChangeWatcher(() => CoreUserPreferences.AutoKeyframes, i => button.Checked = i);
			button.Clicked += () => CoreUserPreferences.AutoKeyframes = !CoreUserPreferences.AutoKeyframes;
			return button;
		}

		ToolbarButton CreateNewFolderButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Tools.NewFolder")) { Tip = "Create folder" };
			button.AddTransactionClickHandler(() => {
				var newFolder = new Folder { Id = "Folder" };
				Core.Operations.InsertFolderItem.Perform(newFolder);
				Core.Operations.ClearRowSelection.Perform();
				Core.Operations.SelectRow.Perform(Document.Current.GetRowForObject(newFolder));
			});
			return button;
		}

		ToolbarButton CreateExitButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.ExitContainer")) { Tip = "Exit current container (backspace)" };
			button.AddTransactionClickHandler(Core.Operations.LeaveNode.Perform);
			button.Updating += _ => button.Enabled = Core.Operations.LeaveNode.IsAllowed();
			return button;
		}

		ToolbarButton CreateEyeButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.Eye")) { Tip = "Show widgets" };
			button.AddTransactionClickHandler(() => {
				var nodes = !RootWidget.Input.IsKeyPressed(Key.Shift) ? Document.Current.Container.Nodes.ToList() : Document.Current.SelectedNodes().ToList();
				var visibility = NodeVisibility.Hidden;
				if (nodes.All(i => i.EditorState().Visibility == NodeVisibility.Hidden)) {
					visibility = NodeVisibility.Shown;
				} else if (nodes.All(i => i.EditorState().Visibility == NodeVisibility.Shown)) {
					visibility = NodeVisibility.Default;
				}
				foreach (var node in nodes) {
					Core.Operations.SetProperty.Perform(node.EditorState(), nameof(NodeEditorState.Visibility), visibility);
				}
			});
			return button;
		}

		ToolbarButton CreateLockButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.Lock")) { Tip = "Lock widgets" };
			button.AddTransactionClickHandler(() => {
				var nodes = !RootWidget.Input.IsKeyPressed(Key.Shift) ? Document.Current.Container.Nodes.ToList() : Document.Current.SelectedNodes().ToList();
				var locked = nodes.All(i => !i.EditorState().Locked);
				foreach (var node in nodes) {
					Core.Operations.SetProperty.Perform(node.EditorState(), nameof(NodeEditorState.Locked), locked);
				}
			});
			return button;
		}

		ToolbarButton CreateLockAnimationButton()
		{
			var button = new ToolbarButton(IconPool.GetTexture("Timeline.AnimationEnabled")) { Tip = "Lock animation" };
			button.AddTransactionClickHandler(() => {
				var nodes = !RootWidget.Input.IsKeyPressed(Key.Shift) ? Document.Current.Container.Nodes.ToList() : Document.Current.SelectedNodes().ToList();
				var enable = !nodes.All(IsAnimationEnabled);
				foreach (var node in nodes) {
					foreach (var animator in node.Animators) {
						Core.Operations.SetProperty.Perform(animator, nameof(IAnimator.Enabled), enable);
					}
				}
			});
			return button;
		}

		static bool IsAnimationEnabled(IAnimable animable)
		{
			foreach (var a in animable.Animators) {
				if (!a.Enabled)
					return false;
			}
			return true;
		}
	}
}

