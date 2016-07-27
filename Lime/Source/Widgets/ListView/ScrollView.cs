using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	[Flags]
	public enum ScrollDirection
	{
		None = 0,
		Horizontal = 1,
		Vertical = 2,
		Both = 3,
	}

	public partial class ScrollView : IDisposable
	{
		public readonly Frame Frame;
		public readonly ScrollViewContentWidget Content;

		protected bool IsBeingRefreshed { get; set; }
		public bool CanScroll { get; set; }
		public bool RejectOrtogonalSwipes { get; set; }
		public Vector2? SwipeSensitivity { get; set; }
		public float BounceZoneThickness = 100;
		public float ScrollToItemVelocity = 800;
		public ScrollDirection ScrollDirection { get; private set; }
		private WheelScrollState wheelScrollState;
		public bool ScrollWhenContentFits = true;
		private float? lastFrameRotation = null;
		private Vector2? lastScrollAxis = null;
		private Vector2 ScrollAxis
		{
			get
			{
				if (lastFrameRotation != Frame.Rotation) {
					lastFrameRotation = Frame.Rotation;
					lastScrollAxis = ScrollDirection == ScrollDirection.Horizontal
						? new Vector2(1.0f, 0.0f)
						: new Vector2(0.0f, 1.0f);
					lastScrollAxis = Vector2.RotateDeg(lastScrollAxis.Value, Frame.Rotation);
				}
				return lastScrollAxis.Value;
			}
		}

		private float ProjectToScrollAxisWithFrameRotation(Vector2 v)
		{
			return Vector2.DotProduct(ScrollAxis, v);
		}

		public virtual bool IsDragging { get; protected set; }

		// TODO: Use WidgetInput instead
		private Input Input { get { return Window.Current.Input; } }

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

		public float MaxOverscroll
		{
			get { return ProjectToScrollAxis(Frame.Size * Vector2.Half); }
		}

		private Task scrollingTask;

		public ScrollView(Frame frame, ScrollDirection scrollDirection = ScrollDirection.Vertical, bool processChildrenFirst = false)
		{
			this.ScrollDirection = scrollDirection;
			RejectOrtogonalSwipes = true;
			Frame = frame;
			Frame.HitTestTarget = true;
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
#if MAC || MONOMAC || WIN
			Content.Tasks.Add(WheelScrollingTask());
#endif
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
			Content.UnlinkAndDispose();
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
			foreach (var t in Task.SinMotion(time, ScrollPosition, position)) {
				ScrollPosition = t;
				yield return null;
			}
			scrollingTask = null;
		}

		public bool IsScrolling()
		{
			return scrollingTask != null || wheelScrollState != WheelScrollState.Stop;
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

		private bool IsUnderMouse()
		{
			return Frame.IsMouseOverDescendant();
		}

		private IEnumerator<object> MainTask()
		{
			while (true) {
				// Wait until a user starts dragging the widget
				while (!Frame.Input.WasMousePressed() || !IsUnderMouse()) {
					Bounce();
					yield return null;
				}
				StopScrolling();
				Vector2 mousePos = Input.MousePosition;
				var velocityMeter = new VelocityMeter();
				velocityMeter.AddSample(ScrollPosition);
				if (RejectOrtogonalSwipes) {
					var r = new TaskResult<bool>();
					yield return !SwipeSensitivity.HasValue ? DetectSwipeAlongScrollAxisTask(r) : DetectSwipeUsingSensitivityTask(r);
					if (r.Value) {
						yield return HandleDragTask(velocityMeter, ProjectToScrollAxisWithFrameRotation(mousePos));
					}
				} else {
					yield return HandleDragTask(velocityMeter, ProjectToScrollAxisWithFrameRotation(mousePos));
				}
			}
		}

		private IEnumerator<object> WheelScrollingTask()
		{
			const int wheelScrollingSpeedFactor = 8;
			wheelScrollState = WheelScrollState.Stop;
			float totalScrollAmount = 0f;
			while (true) {
				yield return null;
				var IsScrollingByMouseWheel =
					!Frame.Input.IsMousePressed() &&
					(Frame.Input.WasKeyPressed(Key.MouseWheelDown) || Frame.Input.WasKeyPressed(Key.MouseWheelUp)) &&
					(CanScroll && IsUnderMouse());
				if (IsScrollingByMouseWheel) {
					var newWheelScrollState = (WheelScrollState)Math.Sign(Frame.Input.WheelScrollAmount);
					if (newWheelScrollState != wheelScrollState) {
						totalScrollAmount = 0f;
						wheelScrollState = newWheelScrollState;
					}
					totalScrollAmount -= Frame.Input.WheelScrollAmount;
				}

				if (totalScrollAmount.Abs() >= 1f && wheelScrollState != WheelScrollState.Stop) {
					StopScrolling();
					var stepPerFrame = totalScrollAmount * Task.Current.Delta * wheelScrollingSpeedFactor;
					var prevScrollPosition = ScrollPosition;
					ScrollPosition = Mathf.Clamp(ScrollPosition + stepPerFrame, MinScrollPosition, MaxScrollPosition);
					if (ScrollPosition == MinScrollPosition || ScrollPosition == MaxScrollPosition) {
						totalScrollAmount = 0f;
					} else {
						// If scroll stopped in the middle, we need to round to upper int if we move down
						// or to lower int if we move up.
						ScrollPosition = stepPerFrame > 0 ? ScrollPosition.Ceiling() : ScrollPosition.Floor();
						totalScrollAmount += (prevScrollPosition - ScrollPosition);
					}
				} else {
					wheelScrollState = WheelScrollState.Stop;
				}
			}
		}

		private IEnumerator<object> DetectSwipeAlongScrollAxisTask(TaskResult<bool> result)
		{
			Vector2 mousePos = Input.MousePosition;
			while (Input.IsMousePressed() && (Input.MousePosition - mousePos).Length < 10) {
				yield return null;
			}
			Vector2 d = Input.MousePosition - mousePos;
			var dt = Vector2.DotProduct(ScrollAxis, d);
			var dn = Vector2.DotProduct(new Vector2(ScrollAxis.Y, -ScrollAxis.X), d);
			result.Value = dt.Abs() > dn.Abs();
		}

		private IEnumerator<object> DetectSwipeUsingSensitivityTask(TaskResult<bool> result) {
			var mousePos = Input.MousePosition;
			while (Input.IsMousePressed()) {
				var deltaPosition = Input.MousePosition - mousePos;
				if (Mathf.Abs(deltaPosition.X) >= SwipeSensitivity.Value.X) {
					result.Value = false;
					yield break;
				}
				if (Mathf.Abs(deltaPosition.Y) >= SwipeSensitivity.Value.Y) {
					result.Value = true;
					yield break;
				}
				yield return null;
			}
			result.Value = false;
		}

		private IEnumerator<object> InertialScrollingTask(float velocity)
		{
			while (true) {
				var delta = Task.Current.Delta;
				float damping = ScrollPosition.InRange(MinScrollPosition, MaxScrollPosition) ? 2.0f : 20.0f;
				velocity -= velocity * damping * delta;
				if (velocity.Abs() < 40.0f) {
					break;
				}
				// Round scrolling position to prevent blurring
				ScrollPosition = Mathf.Clamp(
					value: (ScrollPosition + velocity * delta).Round(),
					min: MinScrollPosition - MaxOverscroll,
					max: MaxScrollPosition + MaxOverscroll
				);
				yield return null;
			}
			scrollingTask = null;
		}

		private IEnumerator<object> HandleDragTask(VelocityMeter velocityMeter, float mouseProjectedPosition)
		{
			if (!CanScroll || !ScrollWhenContentFits && MaxScrollPosition == 0)
				yield break;
			IsBeingRefreshed = false; // Do not block scrollview on refresh gesture
			IsDragging = true;
			Frame.Input.CaptureMouse();
			float realScrollPosition = ScrollPosition;
			wheelScrollState = WheelScrollState.Stop;
			do {
				if (IsItemDragInProgress()) {
					Frame.Input.ReleaseMouse();
					IsDragging = false;
					yield break;
				}
				realScrollPosition += mouseProjectedPosition - ProjectToScrollAxisWithFrameRotation(Input.MousePosition);
				// Round scrolling position to prevent blurring
				ScrollPosition = ClampScrollPositionWithinBounceZone(realScrollPosition).Round();
				mouseProjectedPosition = ProjectToScrollAxisWithFrameRotation(Input.MousePosition);
				velocityMeter.AddSample(realScrollPosition);
				yield return null;
			} while (Input.IsMousePressed());
			Frame.Input.ReleaseMouse();
			StartScrolling(InertialScrollingTask(velocityMeter.CalcVelocity()));
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
				if (PostPresenter != null) {
					chain.Add(this, PostPresenter);
				}
				if (ReverseOrderRendering) {
					AddToRenderChainReversed(chain);
				} else {
					AddToRenderChainDirect(chain);
				}
				if (Presenter != null) {
					chain.Add(this, Presenter);
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

		private enum WheelScrollState : int
		{
			Up = -1,
			Stop = 0,
			Down = 1
		}
	}
}

