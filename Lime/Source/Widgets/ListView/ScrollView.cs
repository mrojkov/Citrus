using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public enum ScrollDirection
	{
		Any,
		Horizontal,
		Vertical,
	}

	public partial class ScrollView : IDisposable
	{
		public readonly Frame Frame;
		public readonly ScrollViewContentWidget Content;

		protected bool IsBeingRefreshed { get; set; }
		public bool CanScroll { get; set; }
		public bool ScrollBySlider { get; set; }
		public bool RejectOrtogonalSwipes { get; set; }
		public float BounceZoneThickness = 100;
		public float ScrollToItemVelocity = 800;
		public float InertialScrollingStopVelocity = 40;
		public float InertialScrollingDamping = 2;
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
					lastScrollAxis = Vector2.RotateDegRough(lastScrollAxis.Value, Frame.Rotation);
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

		public float MinScrollPosition = 0;

		public float MaxScrollPosition
		{
			get { return
					Mathf.Max(0, ProjectToScrollAxis(Content.Size - Frame.Size).Round()) + MinScrollPosition; }
		}

		public float MaxOverscroll
		{
			get { return ProjectToScrollAxis(Frame.Size * Vector2.Half); }
		}

		private Task scrollingTask;

		public ScrollView(Frame frame, ScrollDirection scrollDirection = ScrollDirection.Vertical, bool processChildrenFirst = false)
		{
			ScrollDirection = scrollDirection;
			RejectOrtogonalSwipes = true;
			Frame = frame;
			Frame.Input.AcceptMouseBeyondWidget = false;
			Frame.Input.AcceptMouseThroughDescendants = true;
			Frame.HitTestTarget = true;
			Frame.ClipChildren = ClipMethod.ScissorTest;
			CanScroll = true;
			Content = CreateContentWidget();
			Content.ScrollDirection = ScrollDirection;
			if (ScrollDirection == ScrollDirection.Vertical) {
				Content.Width = Frame.Width;
				Content.Height = 0;
			} else {
				Content.Width = 0;
				Content.Height = Frame.Height;
			}
			Content.PushToNode(Frame);
			if (processChildrenFirst) {
				Content.LateTasks.Add(MainTask());
			} else {
				Content.Tasks.Add(MainTask());
			}
#if MAC || WIN
			Content.Tasks.Add(WheelScrollingTask());
#endif
			Frame.Layout = new Layout(scrollDirection, Content);
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

		public float PositionToView(float pos, float paddingBefore = 0, float paddingAfter = 0)
		{
			return ScrollPosition.Clamp(pos - ProjectToScrollAxis(Frame.Size) + paddingAfter, pos - paddingBefore);
		}

		protected virtual void SetScrollPosition(float value)
		{
			if (!IsBeingRefreshed) {
				SetProjectedPosition(Content, -value);
			}
		}

		protected virtual ScrollViewContentWidget CreateContentWidget()
		{
			return new ScrollViewContentWidget();
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
				SetProjectedSize(Content, Mathf.Max(ProjectToScrollAxis(Content.Size), end));
			}
		}

		private void StartScrolling(IEnumerator<object> newScrollingTask)
		{
			StopScrolling();
			scrollingTask = Content.Tasks.Add(newScrollingTask);
		}

		private void Bounce()
		{
			if (IsScrolling() || ScrollBySlider)
				return;
			if (ScrollPosition < MinScrollPosition)
				ScrollTo(MinScrollPosition);
			else if (ScrollPosition > MaxScrollPosition)
				ScrollTo(MaxScrollPosition);
		}

		private IEnumerator<object> MainTask()
		{
			var dragRecognizer = new DragRecognizer(0, (DragDirection)ScrollDirection);
			Frame.GestureRecognizers.Add(dragRecognizer);
			while (true) {
				if (dragRecognizer.WasBegan()) {
					StopScrolling();
					Vector2 mousePos = Input.MousePosition;
					var velocityMeter = new VelocityMeter();
					velocityMeter.AddSample(ScrollPosition);
					yield return HandleDragTask(velocityMeter, ProjectToScrollAxisWithFrameRotation(mousePos), dragRecognizer);
				}
				Bounce();
				yield return null;
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
					(Frame.Input.WasKeyPressed(Key.MouseWheelDown) || Frame.Input.WasKeyPressed(Key.MouseWheelUp)) && CanScroll;
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

		private IEnumerator<object> InertialScrollingTask(float velocity)
		{
			while (true) {
				var delta = Task.Current.Delta;
				float damping = InertialScrollingDamping * (ScrollPosition.InRange(MinScrollPosition, MaxScrollPosition) ? 1 : 10);
				velocity -= velocity * damping * delta;
				if (velocity.Abs() < InertialScrollingStopVelocity) {
					break;
				}
				// Round scrolling position to prevent blurring
				ScrollPosition = Mathf.Clamp(value: (ScrollPosition + velocity * delta).Round(), min: MinScrollPosition - MaxOverscroll, max: MaxScrollPosition + MaxOverscroll);
				yield return null;
			}
			scrollingTask = null;
		}

		private IEnumerator<object> HandleDragTask(VelocityMeter velocityMeter, float mouseProjectedPosition, DragRecognizer dragRecognizer)
		{
			if (!CanScroll || !ScrollWhenContentFits && MaxScrollPosition == 0 || ScrollBySlider)
				yield break;
			IsBeingRefreshed = false; // Do not block scrollview on refresh gesture
			IsDragging = true;
			float realScrollPosition = ScrollPosition;
			wheelScrollState = WheelScrollState.Stop;
			while (dragRecognizer.IsDragging()) {
				realScrollPosition += mouseProjectedPosition - ProjectToScrollAxisWithFrameRotation(Input.MousePosition);
				// Round scrolling position to prevent blurring
				ScrollPosition = ClampScrollPositionWithinBounceZone(realScrollPosition)
					.Round();
				mouseProjectedPosition = ProjectToScrollAxisWithFrameRotation(Input.MousePosition);
				velocityMeter.AddSample(realScrollPosition);
				yield return null;
			}
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

		public class ScrollViewContentWidget : Widget
		{
			public bool ReverseOrderRendering;
			public ScrollDirection ScrollDirection;

			internal protected override void AddToRenderChain(RenderChain chain)
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
						node.RenderChainBuilder?.AddToRenderChain(node, chain);
					}
				}
			}

			private void AddToRenderChainReversed(RenderChain chain)
			{
				for (int i = Nodes.Count - 1; i >= 0; i--) {
					var item = Nodes[i].AsWidget;
					if (IsItemOnscreen(item)) {
						item.RenderChainBuilder?.AddToRenderChain(item, chain);
					}
				}
			}

			public virtual bool IsItemOnscreen(Widget item)
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

			public virtual bool IsItemFullyOnscreen(Widget item)
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

		protected class Layout : CommonLayout, ILayout
		{
			readonly Widget content;
			protected readonly ScrollDirection direction;

			public Layout(ScrollDirection direction, Widget content)
			{
				this.direction = direction;
				this.content = content;
			}

			public override void MeasureSizeConstraints(Widget widget)
			{
				ConstraintsValid = true;
				widget.MeasuredMinSize = Vector2.Zero;
				widget.MeasuredMaxSize = Vector2.PositiveInfinity;
			}

			public override void ArrangeChildren(Widget widget)
			{
				ArrangementValid = true;
				var p = widget.ContentPosition;
				var s = widget.ContentSize;
				// Check content.Layout for the compatibility with the existing code.
				var contentMinSize = (content.Layout is AnchorLayout) ? content.Size : content.EffectiveMinSize;
				if (direction == ScrollDirection.Vertical) {
					p.Y = content.Y;
					s.Y = contentMinSize.Y;
				} else {
					p.X = content.X;
					s.X = contentMinSize.X;
				}
				LayoutWidgetWithinCell(content, p, s, Alignment.LeftTop);
			}
		}
	}
}

