using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;

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
				MinMaxSize = new Vector2(16, 16)
			};
			widget = new Widget {
				Padding = new Thickness { Left = 4, Right = 2 },
				MinHeight = Metrics.DefaultRowHeight,
				HitTestTarget = true,
				Layout = new HBoxLayout(),
				Nodes = {
					new HSpacer(identation * Metrics.RollIndentation),
					CreateExpandButton(),
					propIcon,
					new HSpacer(3),
					label,
				},
			};
			widget.CompoundPresenter.Push(new DelegatePresenter<Widget>(RenderBackground));
		}

		ToolbarButton CreateExpandButton()
		{
			var button = new ToolbarButton { LayoutCell = new LayoutCell(Alignment.Center) };
			var s = propRow.Animator.EditorState();
			button.Tasks.Add(new Property<bool>(() => s.CurvesShown).DistinctUntilChanged().Consume(i => {
				button.Texture = IconPool.GetTexture(i ? "Timeline.Expanded" : "Timeline.Collapsed");
			}));
			button.Clicked += () => {
				Core.Operations.SetGenericProperty<bool>.Perform(() => s.CurvesShown, value => s.CurvesShown = value, !s.CurvesShown);
			};
			return button;
		}

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, Document.Current.SelectedRows.Contains(row) ? Colors.SelectedBackground : Colors.WhiteBackground);
		}

		Widget IRollWidget.Widget => widget;
	}	
}