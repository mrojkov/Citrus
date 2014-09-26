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
		public bool Expanded { get; private set; }
		public bool Animating { get; private set; }
		public Widget Content
		{
			get { return content; }
			set
			{
				DisposeContent();
				content = value;
				if (content != null) {
					content.Position = Vector2.Zero;
					content.Pivot = Vector2.Zero;
					content.Width = Width;
					subContainer.AddNode(content);
				}
			}
		}

		private Widget content;
		private Frame subContainer;

		public ExpandableListItem(Widget header, ListView listView)
		{
			Header = header;
			ListView = listView;
			Width = listView.Frame.Width;
			Height = header.Height;
			Anchors = Anchors.LeftRight;
			Tasks.Add(AutoLayoutTask());
			subContainer = new Frame() { Width = listView.Frame.Width, Anchors = Anchors.LeftRight };
			AddNode(header);
			AddNode(subContainer);
			SetExpanded(false, animated: false);
			header.Position = Vector2.Zero;
			header.Pivot = Vector2.Zero;
			header.Width = Width;
		}

		private IEnumerator<object> AutoLayoutTask()
		{
			while (true) {
				if (!Animating) {
					StackWidgetsVertically(subContainer);
					StackWidgetsVertically(this);
					if (Expanded) {
						PinHeader();
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

		private void PinHeader()
		{
			float y = -Header.CalcPositionInSpaceOf(ListView.Frame).Y;
			Header.Y = y.Clamp(0, subContainer.Height);
		}

		private IEnumerator<object> ExpandAnimatedTask(Action onAnimationFinished)
		{
			subContainer.Visible = true;
			subContainer.ClipChildren = ClipMethod.ScissorTest;
			StackWidgetsVertically(subContainer);
			Animating = true;
			foreach (var t in TaskList.SinMotion(0.25f, 0, subContainer.Height)) {
				subContainer.Height = t;
				StackWidgetsVertically(this);
				yield return 0;
			}
			Animating = false;
			subContainer.ClipChildren = ClipMethod.None;
			Expanded = true;
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
			Animating = true;
			var subContainerPos = subContainer.CalcPositionInSpaceOf(ListView.Frame);
			var headerPos = Header.CalcPositionInSpaceOf(ListView.Frame);
			var minHeight = (headerPos.Y + Header.Height - subContainerPos.Y).Max(0);
			var bottomPadding = new Widget() { Height = minHeight };
			ListView.Add(bottomPadding);
			foreach (var t in TaskList.SinMotion(0.25f, subContainer.Height, minHeight)) {
				subContainer.Height = t;
				StackWidgetsVertically(this);
				PinHeader();
				yield return 0;
			}
			subContainer.Height = 0;
			Animating = false;
			subContainer.ClipChildren = ClipMethod.None;
			subContainer.Visible = false;
			Expanded = false;
			ListView.Remove(bottomPadding);
			StackWidgetsVertically(this);
			ListView.ScrollPosition -= minHeight;
			ListView.Frame.Update(0);
			if (onAnimationFinished != null) {
				onAnimationFinished();
			}
		}

		private void DisposeContent()
		{
			if (content != null) {
				content.UnlinkAndDispose();
				content = null;
			}
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
