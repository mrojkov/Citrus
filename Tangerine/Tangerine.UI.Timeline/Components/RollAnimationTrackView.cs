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
					Spacer.HSpacer(Theme.Metrics.DefaultToolbarButtonSize.X)
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
