using Lime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class ExpandableListItem : Widget
	{
		public readonly Widget Header;
		public readonly ListView ListView;
		public bool IsExpanded { get; private set; }
		public bool IsAnimating { get; private set; }
		public bool PinHeader { get; set; }
		public NodeList ContentItems { get { return subContainer.Nodes; } }

		private readonly Frame subContainer;

		public ExpandableListItem(Widget header, ListView listView)
		{
			Header = header;
			ListView = listView;
			Width = listView.Frame.Width;
			Height = header.Height;
			Anchors = Anchors.LeftRight;
			PinHeader = true;
			Tasks.Add(AutoLayoutTask());
			subContainer = new Frame() { Width = listView.Frame.Width, Anchors = Anchors.LeftRight };
			AddNode(header);
			AddNode(subContainer);
			SetExpanded(false, animated: false);
			AlignTop(header);
		}

		public void AddContentItem(Widget item) {
			AlignTop(item);
			subContainer.Nodes.Add(item);
		}

		private void AlignTop(Widget w)
		{
			w.Position = Vector2.Zero;
			w.Pivot = Vector2.Zero;
			w.Width = Width;
		}

		private IEnumerator<object> AutoLayoutTask()
		{
			while (true) {
				if (!IsAnimating) {
					StackWidgetsVertically(subContainer);
					StackWidgetsVertically(this);
					if (IsExpanded && PinHeader) {
						DoPinHeader();
					}
				}
				yield return 0;
			}
		}

		protected static void StackWidgetsVertically(Widget container)
		{
			float y = 0;
			for (var node = container.Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				var widget = node.AsWidget;
				if (widget != null && widget.Visible) {
					widget.Y = y;
					y += widget.Height;
				}
			}
			container.Height = y;
		}

		private void DoPinHeader()
		{
			float y = -Header.CalcPositionInSpaceOf(ListView.Frame).Y;
			Header.Y = y.Clamp(0, subContainer.Height);
		}

		private IEnumerator<object> ExpandAnimatedTask(Action onAnimationFinished)
		{
			subContainer.Visible = true;
			subContainer.ClipChildren = ClipMethod.ScissorTest;
			StackWidgetsVertically(subContainer);
			IsAnimating = true;
			foreach (var t in TaskList.SinMotion(0.25f, 0, subContainer.Height)) {
				subContainer.Height = t;
				StackWidgetsVertically(this);
				yield return 0;
			}
			IsAnimating = false;
			subContainer.ClipChildren = ClipMethod.None;
			IsExpanded = true;
			float y = Header.CalcPositionInSpaceOf(ListView.Content).Y;
			ListView.ScrollTo(y);
			if (onAnimationFinished != null) {
				onAnimationFinished();
			}
		}

		private IEnumerator<object> CollapseAnimatedTask(Action onAnimationFinished)
		{
			subContainer.ClipChildren = ClipMethod.ScissorTest;
			StackWidgetsVertically(subContainer);
			IsAnimating = true;
			var subContainerPos = subContainer.CalcPositionInSpaceOf(ListView.Frame);
			var headerPos = Header.CalcPositionInSpaceOf(ListView.Frame);
			var minHeight = (headerPos.Y + Header.Height - subContainerPos.Y).Max(0);
			var bottomPadding = new Widget() { Height = minHeight };
			ListView.Add(bottomPadding);
			foreach (var t in TaskList.SinMotion(0.25f, subContainer.Height, minHeight)) {
				subContainer.Height = t;
				StackWidgetsVertically(this);
				DoPinHeader();
				yield return 0;
			}
			subContainer.Height = 0;
			IsAnimating = false;
			subContainer.ClipChildren = ClipMethod.None;
			subContainer.Visible = false;
			IsExpanded = false;
			ListView.Remove(bottomPadding);
			StackWidgetsVertically(this);
			ListView.ScrollPosition -= minHeight;
			ListView.Frame.Update(0);
			if (onAnimationFinished != null) {
				onAnimationFinished();
			}
		}

		public void DisposeContent()
		{
			foreach (var w in subContainer.Nodes)
				w.AsWidget.UnlinkAndDispose();
		}

		public void SetExpanded(bool value, bool animated, Action onAnimationFinished = null)
		{
			if (!animated) {
				subContainer.Visible = value;
				StackWidgetsVertically(subContainer);
				StackWidgetsVertically(this);
				DisposeContent();
			} else if (value) {
				Tasks.Add(ExpandAnimatedTask(onAnimationFinished));
			} else {
				Tasks.Add(CollapseAnimatedTask(onAnimationFinished));
			}
		}
	}
}
