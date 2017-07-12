using Yuzu;

namespace Lime.RenderOptimizer
{
	public class ContentSizeComponent : NodeComponent
	{
		[YuzuRequired]
		public ContentSize Size;

		public bool IsEmpty => Size == null;
	}

	public class ContentSize
	{
		public virtual ContentSize ForProjection(Node node)
		{
			return this;
		}
	}

	public class ContentRectangle : ContentSize
	{
		[YuzuRequired]
		public Rectangle Data;

		public ContentRectangle()
		{
			Data = new Rectangle(Vector2.Zero, Vector2.One);
		}

		public ContentRectangle(Rectangle aabb, Vector2 size)
		{
			Data = new Rectangle(aabb.A * size, aabb.B * size);
		}

		public ContentRectangle(Rectangle aabb)
		{
			Data = aabb;
		}

		public override ContentSize ForProjection(Node node)
		{
			return new ContentRectangle(Data, ((Widget)node).Size);
		}
	}

	public class ContentPlane : ContentSize
	{
		private static readonly Matrix44 matrix2DTo3D = Matrix44.CreateScale(new Vector3(1, -1, 1));

		[YuzuRequired]
		public Vector3[] Data = new Vector3[4];

		public ContentPlane(Rectangle contentAABB)
		{
			Data[0] = GetPoint(contentAABB.A);
			Data[1] = GetPoint(new Vector2(contentAABB.Right, contentAABB.Top));
			Data[2] = GetPoint(contentAABB.B);
			Data[3] = GetPoint(new Vector2(contentAABB.Left, contentAABB.Bottom));
		}

		private static Vector3 GetPoint(Vector2 position)
		{
			return (Vector3)position * matrix2DTo3D;
		}
	}

	// TODO: Content size for 3D objects
	public class ContentBox : ContentSize
	{
		[YuzuRequired]
		public Bounds Data;
	}
}
