using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	[TangerineClass(allowChildren: true)]
	public class Tab : Widget
	{
		private bool active;
		private Widget closeButton;
		private TextPresentersFeeder textPresentersFeeder;

		public event Action Closing;

		public override string Text { get; set; }
		public bool Closable { get; set; }

		public Tab()
		{
			HitTestTarget = true;
			Theme.Current.Apply(this);
		}

		public bool Active
		{
			get { return active; }
			set
			{
				if (active != value) {
					active = value;
					RefreshPresentation();
				}
			}
		}

		private void RefreshPresentation()
		{
			TryRunAnimation(active ? "Active" : "Normal");
		}

		protected override void Awake()
		{
			textPresentersFeeder = new TextPresentersFeeder(this);
			closeButton = TryFind<Widget>("CloseButton");
			if (closeButton != null) {
				closeButton.Clicked += () => {
					if (Closing != null) {
						Closing();
					}
				};
			}
			RefreshPresentation();
		}

		public override void Update(float delta)
		{
			base.Update(delta);
			if (closeButton != null) {
				closeButton.Visible = Closable;
			}
			if (Input.WasMousePressed() && IsMouseOver()) {
				if (Clicked != null) {
					Clicked();
				}
			}
			textPresentersFeeder.Update();
		}
	}

	public class TabBar : Widget
	{
		public TabBar()
		{
			Theme.Current.Apply(this);
		}

		public void ActivateTab(Tab tab)
		{
			foreach (var t in Nodes.OfType<Tab>()) {
				t.Active = t == tab;
			}
		}
	}
}