using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class AnchorsPropertyEditor : CommonPropertyEditor<Anchors>
	{
		private readonly ToolbarButton firstButton;
		private readonly Widget group;

		public AnchorsPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			group = new Widget { Layout = new HBoxLayout { DefaultCell = new DefaultLayoutCell(Alignment.Center), Spacing = 4 } };
			EditorContainer.AddNode(group);
			firstButton = AddButton(Anchors.Left, "Anchor to the left");
			AddButton(Anchors.Right, "Anchor to the right");
			AddButton(Anchors.Top, "Anchor to the top");
			AddButton(Anchors.Bottom, "Anchor to the bottom");
			AddButton(Anchors.CenterH, "Anchor to the center horizontally");
			AddButton(Anchors.CenterV, "Anchor to the center vertically");
			group.AddNode(Spacer.HStretch());
		}

		private ToolbarButton AddButton(Anchors anchor, string tip)
		{
			var tb = new AnchorButton { LayoutCell = new LayoutCell(Alignment.Center), Tip = tip };
			group.AddNode(tb);
			var current = CoalescedPropertyValue();
			tb.CompoundPresenter.Insert(0, new SyncDelegatePresenter<Widget>(w => DrawIcon(w, anchor)));
			tb.Clicked += () => {
				tb.Checked = !tb.Checked;
				SetProperty(tb.Checked ? current.GetValue().Value | anchor : current.GetValue().Value & ~anchor);
			};
			tb.AddChangeWatcher(current, v => tb.Checked = (v.Value & anchor) != 0);
			return tb;
		}

		private readonly float[] a = { 0, 0, 1, 0, 0, 0, 0, 1, 0.5f, 0, 0, 0.5f };
		private readonly float[] b = { 0, 1, 1, 1, 1, 0, 1, 1, 0.5f, 1, 1, 0.5f };

		private void DrawIcon(Widget button, Anchors anchor)
		{
			button.PrepareRendererState();
			int t = -1;
			while (anchor != Anchors.None) {
				anchor = (Anchors)((int)anchor >> 1);
				t++;
			}
			var w = button.Width;
			var h = button.Height;
			Renderer.DrawLine(
				Scale(a[t * 2], w), Scale(a[t * 2 + 1], h),
				Scale(b[t * 2], w), Scale(b[t * 2 + 1], h),
				Theme.Colors.BlackText
			);
		}

		private static float Scale(float x, float s)
		{
			x *= s;
			if (x == 0) x += 4;
			if (x == s) x -= 4;
			return x;
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			group.Enabled = Enabled;
		}

		private class AnchorButton : ToolbarButton
		{
			protected override void GetColors(State state, out Color4 bgColor, out Color4 borderColor)
			{
				base.GetColors(state, out bgColor, out borderColor);
				if (state == State.Default && !Checked) {
					bgColor = GloballyEnabled ? Theme.Colors.WhiteBackground : Theme.Colors.DisabledBackground;
					borderColor = Theme.Colors.ControlBorder;
				}
			}
		}
	}
}
