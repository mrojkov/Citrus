using System.Collections.Generic;
using System.Linq;
using Lime;
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
		readonly ToolbarButton eyeButton;
		readonly ToolbarButton lockButton;
		readonly RollNodeView.ObjectIdInplaceEditor trackIdEditor;

        public Widget Widget { get; }
		private AnimationTrack Track => trackRow.Track;
		public AwakeBehavior AwakeBehavior => Widget.Components.Get<AwakeBehavior>();
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
			Widget = new Widget {
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
			Widget.Gestures.Add(new ClickGesture(1, ShowTrackContextMenu));
			Widget.Gestures.Add(new DoubleClickGesture(() => {
				Document.Current.History.DoTransaction(() => {
					var labelExtent = label.MeasureUncutText();
					if (label.LocalMousePosition().X < labelExtent.X) {
						trackIdEditor.Rename();
					}
				});
			}));
			label.AddChangeWatcher(() => Track.Id, s => label.Text = s);
			Widget.Components.Add(new AwakeBehavior());
			Widget.CompoundPresenter.Push(new SyncDelegatePresenter<Widget>(RenderBackground));
			Widget.Gestures.Add(new ClickGesture(1, ShowTrackContextMenu));
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
				new Command("Add", () => AddAnimationTrack(row.Index + 1)),
				Command.MenuSeparator,
				Command.Cut,
				Command.Copy,
				Command.Paste,
				Command.Delete
			};
			menu.Popup();
		}

		public static void AddAnimationTrack(int index = -1)
		{
			if (index < 0) {
				index = Document.Current.Animation.Tracks.Count;
			}
			Document.Current.History.DoTransaction(() => {
				var track = new AnimationTrack { Id = GenerateTrackId() };
				Core.Operations.InsertIntoList<AnimationTrackList, AnimationTrack>.Perform(Document.Current.Animation.Tracks, index, track);
				Core.Operations.ClearRowSelection.Perform();
				Core.Operations.SelectRow.Perform(Document.Current.GetRowForObject(track));
			});

			string GenerateTrackId()
			{
				for (int i = 1; ; i++) {
					var id = "Track" + ((i > 1) ? i.ToString() : "");
					if (!Document.Current.Animation.Tracks.Any(t => t.Id == id)) {
						return id;
					}
				}
				throw new System.Exception();
			}
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
