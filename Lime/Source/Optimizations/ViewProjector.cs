namespace Lime.RenderOptimizer
{
	public abstract class ViewProjector
	{
		public abstract Node ProjectorNode { get; }

		public abstract ContentSize Project(Node node, ContentSize size);

		protected static Rectangle CalcAABBInContainer(Matrix32 containerWorldToLocal, Matrix32 contentLocalToWorld, Rectangle contentAABB)
		{
			return CalcAABBInContainer(contentLocalToWorld * containerWorldToLocal, contentAABB);
		}

		protected static Rectangle CalcAABBInContainer(Matrix32 contentToContainer, Rectangle contentAABB)
		{
			var result = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue)
				.IncludingPoint(contentToContainer * contentAABB.A)
				.IncludingPoint(contentToContainer * new Vector2(contentAABB.Right, contentAABB.Top))
				.IncludingPoint(contentToContainer * contentAABB.B)
				.IncludingPoint(contentToContainer * new Vector2(contentAABB.Left, contentAABB.Bottom));
			return result;
		}
	}

	public class ViewRectProjector : ViewProjector
	{
		private readonly Matrix32 viewWorldToLocal;

		public readonly Widget Widget;
		public readonly Rectangle Area;

		public override Node ProjectorNode => Widget;

		public ViewRectProjector(Widget widget, Rectangle area)
		{
			Widget = widget;
			Area = area;
			viewWorldToLocal = Widget.LocalToWorldTransform.CalcInversed();
		}

		public bool IsOnView(ContentSize size)
		{
			var aabb = ((ContentRectangle)size).Data;
			return Rectangle.Intersect(Area, aabb) != Rectangle.Empty;
		}

		public override ContentSize Project(Node node, ContentSize size)
		{
			var rect = size as ContentRectangle;
			if (rect == null) {
				throw new Exception($"{nameof(ViewRectProjector)} can project only {nameof(ContentRectangle)}");
			}

			var widget = (Widget)node;
			return new ContentRectangle(CalcAABBInContainer(viewWorldToLocal, widget.LocalToWorldTransform, rect.Data));
		}
	}

	public class Viewport3DProjector : ViewProjector
	{
		private readonly Vector3 vector2Dto3D = new Vector3(1, -1, 1);
		private readonly Vector2 halfSize;
		private Matrix44 viewProjection;

		public readonly Viewport3D Viewport;

		public override Node ProjectorNode => Viewport;

		public Viewport3DProjector(Viewport3D viewport)
		{
			Viewport = viewport;
			viewProjection = Viewport.Camera.ViewProjection;
			halfSize = Viewport.Size * Vector2.Half;
		}

		public override ContentSize Project(Node node, ContentSize size)
		{
			var plane = size as ContentPlane;
			var box = size as ContentBox;
			if (plane == null && box == null) {
				throw new Exception($"{nameof(Viewport3DProjector)} can project only {nameof(ContentPlane)} or {nameof(ContentBox)}");
			}

			Vector3[] points;
			if (plane != null) {
				points = plane.Data;
			} else {
				points = new[] {
					box.Data.A,
					new Vector3(box.Data.B.X, box.Data.A.Y, box.Data.A.Z),
					new Vector3(box.Data.B.X, box.Data.A.Y, box.Data.B.Z),
					new Vector3(box.Data.A.X, box.Data.A.Y, box.Data.B.Z),
					new Vector3(box.Data.A.X, box.Data.B.Y, box.Data.A.Z),
					new Vector3(box.Data.B.X, box.Data.B.Y, box.Data.A.Z),
					box.Data.B,
					new Vector3(box.Data.A.X, box.Data.B.Y, box.Data.B.Z)
				};
			}

			var node3D = (Node3D)node;
			var result = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
			for (var i = 0; i < points.Length; i++) {
				var worldPoint = points[i] * node3D.GlobalTransform;
				var pt = viewProjection.ProjectVector(worldPoint) * vector2Dto3D;
				var viewportPoint = ((Vector2)pt + Vector2.One) * halfSize;
				result = result.IncludingPoint(viewportPoint);
			}
			return new ContentRectangle(result);
		}
	}

	public class WidgetAdapter3DProjector : ViewProjector
	{
		public readonly WidgetAdapter3D WidgetAdapter;

		public override Node ProjectorNode => WidgetAdapter;

		public WidgetAdapter3DProjector(WidgetAdapter3D widgetAdapter)
		{
			WidgetAdapter = widgetAdapter;
		}

		public override ContentSize Project(Node node, ContentSize size)
		{
			var rect = (ContentRectangle)size;
			var widget = (Widget)node;
			var aabb = CalcAABBInContainer(widget.LocalToWorldTransform, rect.Data);
			return new ContentPlane(aabb);
		}
	}
}
