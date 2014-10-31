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

		private float velocity;
		private Task interialScrollingTask;

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

		public bool IsScrolling()
		{
			return velocity != 0;
		}

		public void StopScrolling()
		{
			velocity = 0;
		}

		private void StartIntertialScrolling()
		{
			interialScrollingTask = Content.Tasks.Add(IntertialScrollingTask());
		}

		private IEnumerator<object> MainTask()
		{
			while (true) {
				// Wait until a user starts dragging the widget
				while (!Frame.Input.WasMousePressed() || !Frame.IsMouseOver()) {
					yield return 0;
				}
				if (interialScrollingTask != null) {
					interialScrollingTask.Dispose();
				}
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
				StartIntertialScrolling();
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

		private IEnumerator<object> IntertialScrollingTask()
		{
			while (true) {
				DoIntertialScrolling(TaskList.Current.Delta);
				yield return 0;
			}
		}

		private void DoIntertialScrolling(float delta)
		{
			if (ScrollPosition < MinScrollPosition) {
				BounceFromTop(delta);
			} else if (ScrollPosition > MaxScrollPosition) {
				BounceFromBottom(delta);
			} else {
				float damping = velocity * 2.0f;
				var prevVelocity = velocity;
				velocity -= damping * delta;
				if (velocity.Sign() != prevVelocity.Sign()) {
					velocity = 0;
				}
				AdvanceScrollPosition(delta);
			}
		}

		private void BounceFromBottom(float delta)
		{
			velocity = -velocity;
			ScrollPosition = MaxScrollPosition - ScrollPosition;
			BounceFromTop(delta);
			velocity = -velocity;
			ScrollPosition = MaxScrollPosition - ScrollPosition;
		}

		private void BounceFromTop(float delta)
		{
			if (velocity < 0) {
				velocity += 50000 * delta;
				velocity = velocity.Clamp(-10000, 0);
				AdvanceScrollPosition(delta);
			} else {
				velocity = (-ScrollPosition * 10).Clamp(300, 5000);
				AdvanceScrollPosition(delta);
				if (ScrollPosition > -float.Epsilon) {
					ScrollPosition = 0;
					velocity = 0;
				}
			}
		}

		private void AdvanceScrollPosition(float delta)
		{
			// Round scrolling position to prevent blurring
			ScrollPosition = (ScrollPosition + velocity * delta).Round();
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
			velocity = velocityMeter.CalcVelocity();
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

			public override void Update(float delta)
			{
				RaiseUpdating(delta);
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					var widget = node.AsWidget;
					if (IsItemOnscreen(widget)) {
						widget.Update(delta);
					} else {
						widget.RaiseUpdating(delta);
						widget.RaiseUpdated(delta);
					}
				}
				RaiseUpdated(delta);
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
