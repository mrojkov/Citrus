using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class RollPropertyView : IRollWidget
	{
		readonly Row row;
		readonly SimpleText label;
		readonly Image propIcon;
		readonly Widget widget;
		readonly PropertyRow propRow;

		public RollPropertyView(Row row, int identation)
		{
			this.row = row;
			propRow = row.Components.Get<PropertyRow>();
			label = new SimpleText { AutoSizeConstraints = false, LayoutCell = new LayoutCell(Alignment.Center), Text = propRow.Animator.TargetProperty };
			propIcon = new Image {
				LayoutCell = new LayoutCell(Alignment.Center),
				Texture = IconPool.GetTexture("Nodes.Unknown"),
				MinMaxSize = Metrics.IconSize
			};
			widget = new Widget {
				Padding = new Thickness { Left = 4, Right = 2 },
				MinHeight = Metrics.TimelineDefaultRowHeight,
				HitTestTarget = true,
				Layout = new HBoxLayout(),
				Nodes = {
					new HSpacer(identation * Metrics.TimelineRollIndentation),
					CreateExpandButton(),
					propIcon,
					new HSpacer(3),
					label,
				},
			};
			widget.CompoundPresenter.Push(new DelegatePresenter<Widget>(RenderBackground));
		}

		CustomCheckbox CreateExpandButton()
		{
			var button = new CustomCheckbox(IconPool.GetTexture("Timeline.Expanded"), IconPool.GetTexture("Timeline.Collapsed")) {
				LayoutCell = new LayoutCell(Alignment.Center),
			};
			var s = propRow.Animator.EditorState();
			button.Updated += delta => button.Checked = s.CurvesShown;
			button.Clicked += () => {
				Document.Current.History.Execute(new Core.Operations.SetGenericProperty<bool>(() => s.CurvesShown, value => s.CurvesShown = value, !s.CurvesShown));
			};
			return button;
		}

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, Timeline.Instance.SelectedRows.Contains(row) ? Colors.SelectedBackground : Colors.WhiteBackground);
		}

		Widget IRollWidget.Widget => widget;
	}	
}