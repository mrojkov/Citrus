using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Информация о трансформации. Аналогично матрице, но представляет данные в более удобном виде
	/// </summary>
	public struct Transform
	{
		public Vector2 Position;
		public Vector2 Scale;
		public float Rotation;
		public Vector2 U;
		public Vector2 V;
	}

	public struct WidgetHull
	{
		public Vector2 V1;
		public Vector2 V2;
		public Vector2 V3;
		public Vector2 V4;
	}

	public partial class Widget : Node
	{
		/// <summary>
		/// Renders widget's descendants to texture. If you want to render widget itself
		/// then add it to provided renderChain before calling RenderToTexture. Be sure
		/// not to do this inside Render() call.
		/// </summary>
		/// <param name="texture">Texture to render to. Must be RenderTexture or SerializableTexture</param>
		/// <param name="renderChain">Render queue (order relation of elements with tree structure and layers)</param>
		public void RenderToTexture(ITexture texture, RenderChain renderChain)
		{
			if (Width > 0 && Height > 0) {
				var scissorTest = Renderer.ScissorTestEnabled;
				if (scissorTest) {
					Renderer.ScissorTestEnabled = false;
				}
				texture.SetAsRenderTarget();
				var savedViewport = Renderer.Viewport;
				Renderer.Viewport = new WindowRect { X = 0, Y = 0, Width = texture.ImageSize.Width, Height = texture.ImageSize.Height };
				Renderer.ClearRenderTarget(0, 0, 0, 0);
				Renderer.PushProjectionMatrix();
				Renderer.SetOrthogonalProjection(0, 0, Width, Height);
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.AddToRenderChain(renderChain);
				}
				renderChain.RenderAndClear();
				texture.RestoreRenderTarget();
				Renderer.Viewport = savedViewport;
				Renderer.PopProjectionMatrix();
				if (scissorTest) {
					Renderer.ScissorTestEnabled = true;
				}
			}
		}

		/// <summary>
		/// Возвращает true, если два виджета пересекаются. Результат не зависит от свойств HitTestMethod.
		/// Проверяется только пересечение границ виджетов
		/// </summary>
		public static bool AreWidgetsIntersected(Widget a, Widget b)
		{
			Vector2[] rect = new Vector2[4] {
				new Vector2(0, 0), new Vector2(1, 0),
				new Vector2(1, 1), new Vector2(0, 1)
			};
			var sizes = new Vector2[2] { a.Size, b.Size };
			var matrices = new Matrix32[2] { a.LocalToWorldTransform, b.LocalToWorldTransform };
			var det = new float[2] { matrices[0].CalcDeterminant(), matrices[1].CalcDeterminant() };
			if (det[0] == 0 || det[1] == 0) {
				return false;
			}
			for (int k = 0; k < 2; k++)
				for (int i = 0; i < 4; i++) {
					var ptA = matrices[k] * (rect[i] * sizes[k]);
					var ptB = matrices[k] * (rect[(i + 1) % 4] * sizes[k]);
					var isOutside = true;
					for (int j = 0; j < 4; j++) {
						var pt = matrices[1 - k] * (rect[j] * sizes[1 - k]);
						isOutside = isOutside && Geometry.CalcPointHalfPlane(pt, ptA, ptB) * det[k] < 0;
					}
					if (isOutside)
						return false;
				}
			return true;
		}

		public void CenterOnParent()
		{
			if (Parent == null) {
				throw new Lime.Exception("Parent must not be null");
			}
			Position = Parent.AsWidget.Size * 0.5f;
			Pivot = Vector2.Half;
		}

		public Matrix32 CalcTransitionToSpaceOf(Widget container)
		{
			Matrix32 mtx1 = container.LocalToWorldTransform.CalcInversed();
			Matrix32 mtx2 = LocalToWorldTransform;
			return mtx2 * mtx1;
		}

		public void CalcHullInSpaceOf(out WidgetHull hull, Widget container)
		{
			var transform = CalcTransformInSpaceOf(container);
			hull.V1 = transform.Position - transform.U * Size.X * Pivot.X - transform.V * Size.Y * Pivot.Y;
			hull.V2 = hull.V1 + transform.U * Size.X;
			hull.V3 = hull.V1 + transform.U * Size.X + transform.V * Size.Y;
			hull.V4 = hull.V1 + transform.V * Size.Y;
		}

		/// <summary>
		/// Возвращает ограничивающий прямоугольник этого виджета, если бы он находился в этой же экранной координате, но в другом контейнере
		/// </summary>
		public Rectangle CalcAABBInSpaceOf(Widget container)
		{
			WidgetHull hull;
			CalcHullInSpaceOf(out hull, container);
			var aabb = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
			aabb.IncludePoint(hull.V1);
			aabb.IncludePoint(hull.V2);
			aabb.IncludePoint(hull.V3);
			aabb.IncludePoint(hull.V4);
			return aabb;
		}

		public Transform CalcTransformFromMatrix(Matrix32 matrix)
		{
			var v1 = new Vector2(1, 0);
			var v2 = new Vector2(0, 1);
			var v3 = new Vector2(0, 0);
			v1 = matrix.TransformVector(v1);
			v2 = matrix.TransformVector(v2);
			v3 = matrix.TransformVector(v3);
			v1 = v1 - v3;
			v2 = v2 - v3;
			Transform transform;
			transform.Position = matrix.TransformVector(Pivot * Size);
			transform.Scale = new Vector2(v1.Length, v2.Length);
			transform.Rotation = v1.Atan2Deg;
			transform.U = v1;
			transform.V = v2;
			return transform;
		}

		/// <summary>
		/// Возвращает информацию о трансформации этого виджета, если бы он находился в этой же экранной координате, но в другом контейнере
		/// </summary>
		public Transform CalcTransformInSpaceOf(Widget container)
		{
			Matrix32 matrix = CalcTransitionToSpaceOf(container);
			return CalcTransformFromMatrix(matrix);
		}

		/// <summary>
		/// Возвращает позицию этого виджета, если бы он находился в этой же экранной координате, но в другом контейнере
		/// </summary>
		public Vector2 CalcPositionInSpaceOf(Widget container)
		{
			Matrix32 matrix = CalcTransitionToSpaceOf(container);
			return matrix.TransformVector(Pivot * Size);
		}

		/// <summary>
		/// Отладочный метод, позволяющий узнать причины, по которым виджет не отрисовывается
		/// </summary>
		public virtual IEnumerable<string> GetVisibilityIssues()
		{
			var world = World.Instance;
			if (!ChildOf(world) && (this != world)) {
				yield return "The widget is not included to the world hierarchy";
			}
			if (!Visible) {
				yield return "The flag 'Visible' is not set";
			} else if (Opacity == 0) {
				yield return "It is fully transparent! Check up 'Opacity' property!";
			} else if (Opacity < 0.1f) {
				yield return "It is almost transparent! Check up 'Opacity' property!";
			} else if (!GloballyVisible) {
				yield return "One of its parent has 'Visible' flag not set";
			} else if (GlobalColor.A < 10) {
				yield return "One of its parent has 'Opacity' close to zero";
			}
			var basis = CalcTransformInSpaceOf(world);
			if ((basis.Scale.X * Size.X).Abs() < 1 || (basis.Scale.Y * Size.Y).Abs() < 1) {
				yield return string.Format("The widget is probably too small");
			}
			bool passedHitTest = false;
			var hitTestLT = world.Position;
			var hitTestRB = hitTestLT + world.Size;
			for (float y = hitTestLT.Y; y < hitTestRB.Y && !passedHitTest; y++) {
				for (float x = hitTestLT.X; x < hitTestRB.X && !passedHitTest; x++) {
					if (this.SelfHitTest(new Vector2(x, y))) {
						passedHitTest = true;
					}
				}
			}
			if (!passedHitTest) {
				yield return string.Format("SelfHitTest() returns false in range [{0}] x [{1}].", hitTestLT, hitTestRB);
			}
			if (!(this is Image) && (this.Nodes.Count == 0)) {
				yield return "The widget doesn't contain any drawable node";
			}
		}
	}
}
