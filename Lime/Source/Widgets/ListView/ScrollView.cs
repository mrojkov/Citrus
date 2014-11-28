using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public enum ScrollDirection
	{
		Horizontal,
		Vertical
	}

	public partial class ScrollView : IDisposable
	{
		public readonly Frame Frame;
		public readonly ScrollViewContentWidget Content;

		protected bool IsBeingRefreshed { get; set; }
		public bool CanScroll { get; set; }
		public bool RejectOrtogonalSwipes { get; set; }
		public float BounceZoneThickness = 100;
		public float ScrollToItemVelocity = 800;
		public ScrollDirection ScrollDirection { get; private set; }
		protected virtual bool IsDragging { get; set; }
		
		public float ContentLength
		{ 
			get { return ProjectToScrollAxis(Content.Size); }
			set { SetProjectedSize(Content, value); }
		}

		public float ScrollPosition
		{
			get { return ProjectToScrollAxis(-Content.Position); }
			set { SetScrollPosition(value); }
		}

		public readonly float MinScrollPosition = 0;

		public float MaxScrollPosition
		{
			get { return Mathf.Max(0, ProjectToScrollAxis(Content.Size - Frame.Size).Round()); }
		}

		private Task scrollingTask;

		public ScrollView(Frame frame, ScrollDirection scrollDirection = ScrollDirection.Vertical, bool processChildrenFirst = false)
		{
			this.ScrollDirection = scrollDirection;
			RejectOrtogonalSwipes = true;
			Frame = frame;
			Frame.ClipChildren = ClipMethod.ScissorTest;
			CanScroll = true;
			Content = new ScrollViewContentWidget() { ScrollDirection = ScrollDirection };
			if (ScrollDirection == ScrollDirection.Vertical) {
				Content.Width = Frame.Width;
				Content.Height = 0;
			} else {
				Content.Width = 0;
				Content.Height = Frame.Height;
			}
			Content.Anchors = (ScrollDirection == ScrollDirection.Vertical) ? Anchors.LeftRight : Anchors.TopBottom;
			Content.PushToNode(Frame);
			if (processChildrenFirst) {
				Content.LateTasks.Add(MainTask());
			} else {
				Content.Tasks.Add(MainTask());
			}
		}

		public bool IsItemOnscreen(Widget item)
		{
			return Content.IsItemOnscreen(item);
		}

		public bool IsItemFullyOnscreen(Widget item)
		{
			return Content.IsItemFullyOnscreen(item);
		}

		public float ProjectToScrollAxis(Vector2 vector)
		{
			return (ScrollDirection == ScrollDirection.Horizontal) ? vector.X : vector.Y;
		}

		public void SetProjectedPosition(Widget w, float position)
		{
			if (ScrollDirection == ScrollDirection.Vertical) {
				w.Y = position;
			}
			else {
				w.X = position;
			}
		}

		public void SetProjectedSize(Widget w, float position)
		{
			if (ScrollDirection == ScrollDirection.Vertical) {
				w.Height = position;
			}
			else {
				w.Width = position;
			}
		}

		public virtual void Dispose()
		{
			Content.Unlink();
		}

		/// <summary>
		/// Include whole widget into frame. If the widget is too large, display top part.
		/// </summary>
		public float PositionToViewFully(Widget w)
		{
			var p = ProjectToScrollAxis(w.CalcPositionInSpaceOf(Content));
			if (p < ScrollPosition)
				return p;
			return Mathf.Clamp(
				p + ProjectToScrollAxis(w.Size) - ProjectToScrollAxis(Frame.Size), ScrollPosition, p);
		}

		protected virtual void SetScrollPosition(float value)
		{
			if (!IsBeingRefreshed) {
				SetProjectedPosition(Content, -value);
			}
		}

		public void ScrollTo(float position, bool instantly = false)
		{
			var p = position.Clamp(MinScrollPosition, MaxScrollPosition);
			if (instantly) {
				ScrollPosition = p;
			}
			else {
				StartScrolling(ScrollToTask(p));
			}
		}

		private IEnumerator<object> ScrollToTask(float position)
		{
			float time = (position - ScrollPosition).Abs() / ScrollToItemVelocity;
			foreach (var t in TaskList.SinMotion(time, ScrollPosition, position)) {
				ScrollPosition = t;
				yield return 0;
			}
			scrollingTask = null;
		}

		public bool IsScrolling()
		{
			return scrollingTask != null;
		}

		public void StopScrolling()
		{
			if (scrollingTask != null) {
				scrollingTask.Dispose();
				scrollingTask = null;
			}
		}

		public void AssimilateChildren()
		{
			foreach (var ch in Frame.Nodes.ToArray()) {
				if (ch == Content) continue;
				ch.Unlink();
				Content.AddNode(ch);
				var w = ch.AsWidget;
				if (w == null) continue;
				var end = ProjectToScrollAxis(w.Position + w.Size) * (1 - ProjectToScrollAxis(w.Pivot));
				if (end > Content.Height)
					Content.Height = end;
			}
		}

		private void StartScrolling(IEnumerator<object> newScrollingTask)
		{
			StopScrolling();
			scrollingTask = Content.Tasks.Add(newScrollingTask);
		}

		private void Bounce()
		{
			if (IsScrolling())
				return;
			if (ScrollPosition < MinScrollPosition)
				ScrollTo(MinScrollPosition);
			else if (ScrollPosition > MaxScrollPosition)
				ScrollTo(MaxScrollPosition);
		}

		private IEnumerator<object> MainTask()
		{
			while (true) {
				// Wait until a user starts dragging the widget
				while (!Frame.Input.WasMousePressed() || !Frame.IsMouseOver()) {
					Bounce();
					yield return 0;
				}
				StopScrolling();
				Vector2 mousePos = Input.MousePosition;
				var velocityMeter = new VelocityMeter();
				velocityMeter.AddSample(ScrollPosition);
				if (RejectOrtogonalSwipes) {
					var r = new TaskResult<bool>();
					yield return DetectSwipeAlongScrollAxisTask(r);
					if (r.Value) {
						yield return HandleDragTask(velocityMeter, ProjectToScrollAxis(mousePos));
					}
				} else {
					yield return HandleDragTask(velocityMeter, ProjectToScrollAxis(mousePos));
				}
			}
		}

		private IEnumerator<object> DetectSwipeAlongScrollAxisTask(TaskResult<bool> result)
		{
			Vector2 mousePos = Input.MousePosition;
			while (Input.IsMousePressed() && (Input.MousePosition - mousePos).Length < 10) {
				yield return 0;
			}
			Vector2 d = Input.MousePosition - mousePos;
			if (ScrollDirection == ScrollDirection.Vertical) {
				result.Value = d.Y.Abs() > d.X.Abs();
			} else {
				result.Value = d.X.Abs() > d.Y.Abs();
			}
		}

		private IEnumerator<object> IntertialScrollingTask(float velocity)
		{
			while (true) {
				var delta = TaskList.Current.Delta;
				float damping = ScrollPosition.InRange(MinScrollPosition, MaxScrollPosition) ? 2.0f : 20.0f;
				velocity -= velocity * damping * delta;
				if (velocity.Abs() < 40.0f) {
					break;
				}
				// Round scrolling position to prevent blurring
				ScrollPosition = (ScrollPosition + velocity * delta).Round();
				yield return 0;
			}
			scrollingTask = null;
		}

		private IEnumerator<object> HandleDragTask(VelocityMeter velocityMeter, float mouseProjectedPosition)
		{
			if (!CanScroll)
				yield break;
			IsDragging = true;
			Frame.Input.CaptureMouse();
			float realScrollPosition = ScrollPosition;
			do {
				if (IsItemDragInProgress()) {
					Frame.Input.ReleaseMouse();
					IsDragging = false;
					yield break;
				}
				realScrollPosition += mouseProjectedPosition - ProjectToScrollAxis(Input.MousePosition);
				// Round scrolling position to prevent blurring
				ScrollPosition = ClampScrollPositionWithinBounceZone(realScrollPosition).Round();
				mouseProjectedPosition = ProjectToScrollAxis(Input.MousePosition);
				velocityMeter.AddSample(realScrollPosition);
				yield return 0;
			} while (Input.IsMousePressed());
			Frame.Input.ReleaseMouse();
			StartScrolling(IntertialScrollingTask(velocityMeter.CalcVelocity()));
			IsDragging = false;
		}

		private float ClampScrollPositionWithinBounceZone(float scrollPosition)
		{
			float resistance = 1.0f / 3;
			if (scrollPosition < 0) {
				return (scrollPosition * resistance).Clamp(-BounceZoneThickness, 0);
			} else if (scrollPosition > MaxScrollPosition) {
				return ((scrollPosition - MaxScrollPosition) * resistance).Clamp(0, BounceZoneThickness) + MaxScrollPosition;
			} else {
				return scrollPosition;
			}
		}

		protected virtual bool IsItemDragInProgress()
		{
			return false;
		}

		public class ScrollViewContentWidget : Widget
		{
			public bool ReverseOrderRendering;
			public ScrollDirection ScrollDirection;

			public override void AddToRenderChain(RenderChain chain)
			{
				if (ReverseOrderRendering) {
					AddToRenderChainReversed(chain);
				} else {
					AddToRenderChainDirect(chain);
				}
			}

			private void AddToRenderChainDirect(RenderChain chain)
			{
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					if (IsItemOnscreen(node.AsWidget)) {
						node.AddToRenderChain(chain);
					}
				}
			}

			private void AddToRenderChainReversed(RenderChain chain)
			{
				for (int i = Nodes.Count - 1; i >= 0; i--) {
					var item = Nodes[i].AsWidget;
					if (IsItemOnscreen(item)) {
						item.AddToRenderChain(chain);
					}
				}
			}

			public bool IsItemOnscreen(Widget item)
			{
				var frame = Parent.AsWidget;
				if (ScrollDirection == ScrollDirection.Vertical) {
					var y0 = item.Y + Y;
					var y1 = y0 + item.Height;
					return y1 >= 0 && y0 < frame.Height;
				} else {
					var x0 = item.X + X;
					var x1 = x0 + item.Width;
					return x1 >= 0 && x0 < frame.Width;
				}
			}

			public bool IsItemFullyOnscreen(Widget item)
			{
				var frame = Parent.AsWidget;
				if (ScrollDirection == ScrollDirection.Vertical) {
					var y0 = item.Y + Y;
					var y1 = y0 + item.Height;
					return y0 >= 0 && y1 < frame.Height;
				} else {
					var x0 = item.X + X;
					var x1 = x0 + item.Width;
					return x0 >= 0 && x1 < frame.Width;
				}
			}
		}
	}
}
