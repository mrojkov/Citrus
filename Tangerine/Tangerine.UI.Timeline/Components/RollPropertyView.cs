using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline.Components
{
	public class RollPropertyView : IRollRowView
	{
		readonly Row row;
		readonly SimpleText label;
		readonly Image propIcon;
		readonly Widget widget;
		readonly PropertyRow propRow;
		readonly Widget spacer;

		public RollPropertyView(Row row)
		{
			this.row = row;
			propRow = row.Components.Get<PropertyRow>();
			label = new ThemedSimpleText { Text = propRow.Animator.TargetPropertyPath };
			propIcon = new Image {
				LayoutCell = new LayoutCell(Alignment.Center),
				Texture = IconPool.GetTexture("Nodes.Unknown"),
				MinMaxSize = new Vector2(16, 16)
			};
			widget = new Widget {
				Padding = new Thickness { Left = 4, Right = 2 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Layout = new HBoxLayout { DefaultCell = new DefaultLayoutCell(Alignment.Center) },
				HitTestTarget = true,
				Nodes = {
					(spacer = new Widget()),
					Spacer.HSpacer(6),
					propIcon,
					Spacer.HSpacer(3),
					label,
					new Widget(),
					CreateLockAnimationButton(),
					Spacer.HSpacer(Theme.Metrics.DefaultToolbarButtonSize.X),
					Spacer.HSpacer(Theme.Metrics.DefaultToolbarButtonSize.X)
				},
			};
			widget.Components.Add(new AwakeBehavior());
			widget.CompoundPresenter.Push(new SyncDelegatePresenter<Widget>(RenderBackground));
			widget.Gestures.Add(new ClickGesture(1, ShowPropertyContextMenu));
		}

		void ShowPropertyContextMenu()
		{
			Document.Current.History.DoTransaction(() => {
				if (!row.Selected) {
					Core.Operations.ClearRowSelection.Perform();
					Core.Operations.SelectRow.Perform(row);
				}
			});
			var menu = new Menu {
				Command.Cut,
				Command.Copy,
				Command.Paste,
				Command.Delete
			};
			menu.Popup();
		}

		ToolbarButton CreateExpandButton()
		{
			var button = new ToolbarButton { LayoutCell = new LayoutCell(Alignment.Center) };
			var s = propRow.Animator.EditorState();
			button.AddChangeWatcher(() => s.CurvesShown,
				i => button.Texture = IconPool.GetTexture(i ? "Timeline.Expanded" : "Timeline.Collapsed"));
			button.Clicked += () => {
				Core.Operations.SetProperty.Perform(s, nameof(AnimatorEditorState.CurvesShown), !s.CurvesShown);
				if (s.CurvesShown) {
					Timeline.Instance.EnsureRowChildsVisible(row);
				}
			};
			return button;
		}

		ToolbarButton CreateLockAnimationButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(
				() => propRow.Animator.Enabled,
				i => button.Texture = IconPool.GetTexture(i ? "Timeline.AnimationEnabled" : "Timeline.AnimationDisabled")
			);
			button.AddTransactionClickHandler(() => {
				Core.Operations.SetProperty.Perform(propRow.Animator, nameof(IAnimator.Enabled), !propRow.Animator.Enabled);
			});
			return button;
		}

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(
				Vector2.Zero, widget.Size,
				row.Selected ? Theme.Colors.SelectedBackground : Theme.Colors.WhiteBackground);
		}

		public void Rename() { }

		public Widget Widget => widget;
		public AwakeBehavior AwakeBehavior => widget.Components.Get<AwakeBehavior>();
		public float Indentation { set { spacer.MinMaxWidth = value; } }
	}
}
