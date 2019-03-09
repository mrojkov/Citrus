using Lime;
using System.Collections.Generic;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline.Components
{
	public class RollAnimationTrackView : IRollRowView
	{
		readonly Row row;
		readonly SimpleText label;
		readonly Widget editBoxContainer;
		readonly Image propIcon;
		readonly AnimationTrackRow trackRow;
		readonly Widget spacer;
		readonly Widget widget;
		readonly ToolbarButton eyeButton;
		readonly ToolbarButton lockButton;
		readonly RollNodeView.ObjectIdInplaceEditor trackIdEditor;

		AnimationTrack Track => trackRow.Track;

		public Widget Widget => widget;
		public AwakeBehavior AwakeBehavior => widget.Components.Get<AwakeBehavior>();
		public float Indentation { set { spacer.MinMaxWidth = value; } }

		public RollAnimationTrackView(Row row)
		{
			this.row = row;
			trackRow = row.Components.Get<AnimationTrackRow>();
			label = new ThemedSimpleText {
				ForceUncutText = false,
				VAlignment = VAlignment.Center,
				OverflowMode = TextOverflowMode.Ellipsis,
				LayoutCell = new LayoutCell(Alignment.LeftCenter, float.MaxValue)
			};
			editBoxContainer = new Widget {
				Visible = false,
				Layout = new HBoxLayout(),
				LayoutCell = new LayoutCell(Alignment.LeftCenter, float.MaxValue),
			};
			propIcon = new Image {
				LayoutCell = new LayoutCell(Alignment.Center),
				Texture = IconPool.GetTexture("Nodes.Unknown"),
				MinMaxSize = new Vector2(16, 16)
			};
			eyeButton = CreateEyeButton();
			lockButton = CreateLockButton();
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
					editBoxContainer,
					Spacer.HSpacer(Theme.Metrics.DefaultToolbarButtonSize.X),
					lockButton
				},
			};
			trackIdEditor = new RollNodeView.ObjectIdInplaceEditor(row, Track, label, editBoxContainer);
			widget.Gestures.Add(new ClickGesture(1, ShowTrackContextMenu));
			widget.Gestures.Add(new DoubleClickGesture(() => {
				Document.Current.History.DoTransaction(() => {
					var labelExtent = label.MeasureUncutText();
					if (label.LocalMousePosition().X < labelExtent.X) {
						trackIdEditor.Rename();
					}
				});
			}));
			label.AddChangeWatcher(() => Track.Id, s => label.Text = s);
			widget.Components.Add(new AwakeBehavior());
			widget.CompoundPresenter.Push(new SyncDelegatePresenter<Widget>(RenderBackground));
			widget.Gestures.Add(new ClickGesture(1, ShowTrackContextMenu));
		}

		public void Rename() => trackIdEditor.Rename();

		ToolbarButton CreateEyeButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(() => trackRow.Visibility, i => {
				var texture = "Timeline.Dot";
				if (i == AnimationTrackVisibility.Shown) {
					texture = "Timeline.Eye";
				} else if (i == AnimationTrackVisibility.Hidden) {
					texture = "Timeline.Cross";
				}
				button.Texture = IconPool.GetTexture(texture);
			});
			button.AddTransactionClickHandler(
				() => Core.Operations.SetProperty.Perform(trackRow, nameof(AnimationTrackRow.Visibility), (AnimationTrackVisibility)(((int)trackRow.Visibility + 1) % 3))
			);
			return button;
		}

		ToolbarButton CreateLockButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(
				() => trackRow.Locked,
				i => button.Texture = IconPool.GetTexture(i ? "Timeline.Lock" : "Timeline.Dot")
			);
			button.AddTransactionClickHandler(() => Core.Operations.SetProperty.Perform(trackRow, nameof(AnimationTrackRow.Locked), !trackRow.Locked));
			return button;
		}

		void ShowTrackContextMenu()
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

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(
				Vector2.Zero, widget.Size,
				row.Selected ? Theme.Colors.SelectedBackground : Theme.Colors.WhiteBackground);
		}
	}
}
