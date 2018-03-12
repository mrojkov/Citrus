using Yuzu;

namespace Lime
{
	public enum SplineInterpolation
	{
		Linear,
		Bezier
	}

	[AllowedChildrenTypes(typeof(SplinePoint3D))]
	public class Spline3D : Node3D
	{
		[YuzuMember]
		public bool Closed { get; set; }

		public Spline3D()
		{
			RenderChainBuilder = null;
		}

		public float CalcLengthRough()
		{
			var length = 0f;
			var segmentCount = GetSegmentCount();
			var scale = GlobalTransform.GetScale(false);
			for (int i = 0; i < segmentCount; i++) {
				length += ((GetPoint(i + 1).Position - GetPoint(i).Position) * scale).Length;
			}
			return length;
		}

		public float CalcLengthAccurate(int approximateCount)
		{
			var length = 0f;
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				length += CalcSegmentLengthAccurate(GetPoint(i), GetPoint(i + 1), approximateCount);
			}
			return length;
		}

		private float CalcSegmentLengthAccurate(SplinePoint3D a, SplinePoint3D b, int approximateCount)
		{
			var length = 0f;
			var transform = GlobalTransform;
			var prevPosition = transform.TransformVector(a.Position);
			for (int i = 1; i < approximateCount; i++) {
				var currentPosition = Interpolate((float)i / approximateCount, a, b).Translation;
				length += (currentPosition - prevPosition).Length;
				prevPosition = currentPosition;
			}
			length += (transform.TransformVector(b.Position) - prevPosition).Length;
			return length;
		}

		public Matrix44 CalcPointTransformInSpaceOf(float amount, Node3D node)
		{
			return CalcPointTransform(amount) * node.GlobalTransform.CalcInverted();
		}

		public Matrix44 CalcPointTransform(float amount)
		{
			var length = 0f;
			var segmentCount = GetSegmentCount();
			var scale = GlobalTransform.GetScale(false);
			for (int i = 0; i < segmentCount; i++) {
				var point1 = GetPoint(i);
				var point2 = GetPoint(i + 1);
				var segmentLength = ((point2.Position - point1.Position) * scale).Length;
				if (length <= amount && amount < length + segmentLength) {
					return Interpolate((amount - length) / segmentLength, point1, point2);
				}
				length += segmentLength;
			}
			if (Nodes.Count > 1) {
				return Interpolate(1.0f, GetPoint(segmentCount - 1), GetPoint(segmentCount));
			}
			if (Nodes.Count == 1) {
				return Matrix44.CreateTranslation(((SplinePoint3D)Nodes[0]).Position);
			}
			return Matrix44.Identity;
		}

		private Matrix44 Interpolate(float amount, SplinePoint3D point1, SplinePoint3D point2)
		{
			var transform = GlobalTransform;
			var position1 = transform.TransformVector(point1.Position);
			var position2 = transform.TransformVector(point2.Position);
			Vector3 position, direction;
			if (point1.Interpolation == SplineInterpolation.Linear) {
				position = Mathf.Lerp(amount, position1, position2);
				direction = position2 - position1;
			} else {
				var tangent1 = position1 + transform.TransformNormal(point1.TangentA);
				var tangent2 = position2 + transform.TransformNormal(point2.TangentB);
				position = Mathf.BezierSpline(amount, position1, tangent1, tangent2, position2);
				direction = Mathf.BezierTangent(amount, position1, tangent1, tangent2, position2);
			}
			return CalcRotationMatrix(direction, Vector3.UnitY) * Matrix44.CreateTranslation(position);
		}

		public int GetSegmentCount()
		{
			if (Nodes.Count > 1) {
				return Closed ? Nodes.Count : Nodes.Count - 1;
			}
			return 0;
		}

		private SplinePoint3D GetPoint(int index)
		{
			return (SplinePoint3D)Nodes[index % Nodes.Count];
		}

		private static Matrix44 CalcRotationMatrix(Vector3 direction, Vector3 up)
		{
			var forward = direction.Normalized;
			var right = Vector3.CrossProduct(forward, up).Normalized;
			var normal = Vector3.CrossProduct(right, forward).Normalized;
			var matrix = Matrix44.Identity;
			matrix.Forward = forward;
			matrix.Right = right;
			matrix.Up = normal;
			return matrix;
		}
	}

	[AllowedParentTypes(typeof(Spline3D))]
	public class SplinePoint3D : Node, Viewport3D.IZSorterParams
	{
		[YuzuMember]
		public SplineInterpolation Interpolation { get; set; }

		[YuzuMember]
		public Vector3 Position { get; set; }

		[YuzuMember]
		public Vector3 TangentA { get; set; }

		[YuzuMember]
		public Vector3 TangentB { get; set; }

		public SplinePoint3D()
		{
			RenderChainBuilder = null;
			Interpolation = SplineInterpolation.Bezier;
		}

		public Vector3 CalcGlobalPosition()
		{
			var transform = (Parent as Spline3D).GlobalTransform;
			return transform.TransformVector(Position);
		}

		bool Viewport3D.IZSorterParams.Opaque => false;

		float Viewport3D.IZSorterParams.CalcDistanceToCamera(Camera3D camera)
		{
			return camera.View.TransformVector(CalcGlobalPosition()).Z;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
		}
	}
}