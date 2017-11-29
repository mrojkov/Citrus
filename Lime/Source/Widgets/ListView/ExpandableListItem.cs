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
			Size = listView.Frame.Size;
			ListView.SetProjectedSize(this, ProjectedSize(Header));
			PinHeader = true;
			Tasks.Add(AutoLayoutTask());
			subContainer = new Frame() {
				Size = listView.Frame.Size,
				Anchors = ListView.ScrollDirection == ScrollDirection.Vertical ? Anchors.LeftRight : Anchors.TopBottom,
			};
			AddNode(header);
			AddNode(subContainer);
			SetExpanded(false, animated: false);
			AlignTop(header);
		}

		public void AddContentItem(Widget item) 
		{
			AlignTop(item);
			subContainer.Nodes.Add(item);
		}

		public void InsertContentItem(int index, Widget item)
		{
			AlignTop(item);
			subContainer.Nodes.Insert(0, item);
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
					StackWidgets(subContainer);
					StackWidgets(this);
					if (IsExpanded && PinHeader) {
						DoPinHeader();
					}
				}
				yield return null;
			}
		}

		private void StackWidgets(Widget container)
		{
			float pos = 0;
			for (var node = container.FirstChild; node != null; node = node.NextSibling) {
				var widget = node.AsWidget;
				if (widget != null && widget.Visible) {
					ListView.SetProjectedPosition(widget, pos);
					pos += ProjectedSize(widget);
				}
			}
			ListView.SetProjectedSize(container, pos);
		}

		private float FramePos(Widget w)
		{
			return ListView.ProjectToScrollAxis(w.CalcPositionInSpaceOf(ListView.Frame));
		}

		private float ProjectedSize(Widget w)
		{
			return ListView.ProjectToScrollAxis(w.Size);
		}

		private void DoPinHeader()
		{
			float p = -FramePos(Header);
			ListView.SetProjectedPosition(Header, p.Clamp(0, ProjectedSize(subContainer)));
		}

		private IEnumerator<object> ResizeSubContainerTask(float from, float to, Action onStep)
		{
			subContainer.ClipChildren = ClipMethod.ScissorTest;
			IsAnimating = true;
			foreach (var t in Task.SinMotion(0.25f, from, to)) {
				ListView.SetProjectedSize(subContainer, t);
				StackWidgets(this);
				onStep();
				yield return null;
			}
			IsAnimating = false;
			IsExpanded = !IsExpanded;
			subContainer.Visible = IsExpanded;
			subContainer.ClipChildren = ClipMethod.None;
		}

		private IEnumerator<object> ExpandAnimatedTask(Action onAnimationFinished)
		{
			subContainer.Visible = true;
			StackWidgets(subContainer);
			yield return ResizeSubContainerTask(
				0, ProjectedSize(subContainer),
				() => ListView.ScrollTo(ListView.PositionToViewFully(this)));
			if (onAnimationFinished != null) {
				onAnimationFinished();
			}
		}

		private IEnumerator<object> CollapseAnimatedTask(Action onAnimationFinished)
		{
			StackWidgets(subContainer);
			var minSize = (FramePos(Header) + ProjectedSize(Header) - FramePos(subContainer)).Max(0);
			var bottomPadding = new Widget();
			ListView.SetProjectedSize(bottomPadding, minSize);
			ListView.Add(bottomPadding);
			yield return ResizeSubContainerTask(ProjectedSize(subContainer), minSize, DoPinHeader);
			ListView.SetProjectedSize(subContainer, 0);
			ListView.Remove(bottomPadding);
			StackWidgets(this);
			ListView.ScrollPosition -= minSize;
			ListView.Frame.Update(0);
			if (onAnimationFinished != null) {
				onAnimationFinished();
			}
		}

		public void DisposeContent()
		{
			foreach (var w in subContainer.Nodes.ToList())
				w.AsWidget.UnlinkAndDispose();
		}

		public void SetExpanded(bool value, bool animated, Action onAnimationFinished = null)
		{
			if (!animated) {
				subContainer.Visible = value;
				StackWidgets(subContainer);
				StackWidgets(this);
				IsExpanded = value;
			} else if (value) {
				Tasks.Add(ExpandAnimatedTask(onAnimationFinished));
			} else {
				Tasks.Add(CollapseAnimatedTask(onAnimationFinished));
			}
		}
	}
}
