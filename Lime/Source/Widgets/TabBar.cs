using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	[TangerineAllowedChildrenTypes(typeof(Node))]
	public class Tab : Widget, IUpdatableNode
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
			Gestures.Add(new ClickGesture(2, () => Closing?.Invoke()));
			Awoke += Awake;
			Components.Add(new UpdatableNodeBehavior());
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

		private static void Awake(Node owner)
		{
			var t = (Tab)owner;
			t.textPresentersFeeder = new TextPresentersFeeder(t);
			t.closeButton = t.TryFind<Widget>("CloseButton");
			if (t.closeButton != null) {
				t.closeButton.Clicked += () => {
					t.Closing?.Invoke();
				};
			}
			t.RefreshPresentation();
		}

		public virtual void OnUpdate(float delta)
		{
			if (closeButton != null) {
				closeButton.Visible = Closable;
			}
			if (Input.WasMousePressed() && IsMouseOver()) {
				Clicked?.Invoke();
			}
			textPresentersFeeder.Update();
		}
	}

	[YuzuDontGenerateDeserializer]
	public class TabBar : Widget
	{
		private readonly DragGesture gesture;

		public bool AllowReordering { get; set; } = false;

		/// <summary>
		/// Occurs when position of a tab has changed.
		/// </summary>
		public event Action<ReorderEventArgs> OnReordering;

		/// <summary>
		/// Occurs when position of a tab changed and users has finished dragging.
		/// </summary>
		public event Action<ReorderEventArgs> OnReordered;

		public void ActivateTab(Tab tab)
		{
			foreach (var t in Nodes.OfType<Tab>()) {
				t.Active = t == tab;
			}
		}

		public TabBar()
		{
			gesture = new DragGesture(direction: DragDirection.Horizontal);
			Gestures.Add(gesture);
			Tasks.Add(DragTabProcessor());
		}

		private IEnumerator<object> DragTabProcessor()
		{
			while (true) {
				if (AllowReordering) {
					if (TryGetTabUnderMouse(out var tab) && gesture.WasRecognized()) {
						ActivateTab(tab);
						var indexFrom = Nodes.IndexOf(tab);
						var indexTo = indexFrom;
						while (!gesture.WasEnded()) {
							if (TryGetTabUnderMouse(out var tabUnderMouse)) {
								var currentIndex = Nodes.IndexOf(tab);
								indexTo = Nodes.IndexOf(tabUnderMouse);
								if (currentIndex != indexTo) {
									Nodes.Move(currentIndex, indexTo);
									OnReordering?.Invoke(new ReorderEventArgs {
										IndexFrom = currentIndex,
										IndexTo = indexTo
									});
								}
							}
							yield return null;
						}
						OnReordered?.Invoke(new ReorderEventArgs {
							IndexFrom = indexFrom,
							IndexTo = indexTo
						});
					}
				}
				yield return null;
			}
		}

		private bool TryGetTabUnderMouse(out Tab tab)
		{
			foreach (var node in Nodes) {
				if (node.AsWidget.BoundingRectHitTest(Input.MousePosition)) {
					tab = node as Tab;
					return true;
				}
			}
			tab = null;
			return false;
		}

		public class ReorderEventArgs
		{
			public int IndexFrom { get; set; }
			public int IndexTo { get; set; }
		}
	}
}
