using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI.SceneView
{

	class NineGridLinePresenter
	{
		public NineGridLinePresenter(SceneView sceneView)
		{
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(Render));
		}

		private void Render(Widget canvas)
		{
			var grids = Core.Document.Current.SelectedNodes().Editable().OfType<NineGrid>();
			foreach (var grid in grids) {
				foreach (var line in NineGridLine.GetForNineGrid(grid)) {
					line.Render(canvas);
				}
			}
		}
	}

	class NineGridLine
	{
		public NineGrid Owner { get; private set; }
		public string PropertyName => PropertyNames[index];
		public float Value => PropertyGetters[index](Owner);
		public float TextureSize => TextureSizeGetters[index % 2](Owner);
		public float GridSize => NineGridSizeGetters[index % 2](Owner);
		public float MaxValue => GridSize / TextureSize;

		private readonly int index;
		private int IndexA => Indexes[index].Item1;
		private int IndexB => Indexes[index].Item2;
		private Vector2 A => Owner.Parts[IndexA].Rect.A;
		private Vector2 B => Owner.Parts[IndexB].Rect.B;

		private readonly Tuple<int, int>[] Indexes = {
			Tuple.Create(5, 2), Tuple.Create(7, 1),
			Tuple.Create(1, 6), Tuple.Create(2, 8)
		};

		private readonly string[] PropertyNames = {
			nameof(NineGrid.LeftOffset),  nameof(NineGrid.TopOffset),
			nameof(NineGrid.RightOffset), nameof(NineGrid.BottomOffset)
		};

		private readonly Func<NineGrid, float>[] PropertyGetters = {
			g => g.LeftOffset, g => g.TopOffset,
			g => g.RightOffset, g => g.BottomOffset
		};

		private readonly Func<NineGrid, float>[] TextureSizeGetters = {
			g => g.Texture.ImageSize.Width,
			g => g.Texture.ImageSize.Height
		};

		private readonly Func<NineGrid, float>[] NineGridSizeGetters = {
			g => g.Size.X,
			g => g.Size.Y
		};

		private readonly Vector2[] Directions = {
			new Vector2(1, 0), new Vector2(0, 1),
			new Vector2(-1, 0), new Vector2(0, -1)
		};

		public NineGridLine(int index, NineGrid nineGrid)
		{
			this.index = index;
			Owner = nineGrid;
		}

		public void Render(Widget canvas)
		{
			var matrix = Owner.CalcTransitionToSpaceOf(canvas);
			var A = matrix.TransformVector(this.A);
			var B = matrix.TransformVector(this.B);
			Renderer.DrawLine(A, B, Color4.Red, 2);
		}

		public bool HitTest(Vector2 point, Widget canvas, float radius = 10)
		{
			var matrix = Owner.CalcTransitionToSpaceOf(canvas);
			var A = matrix.TransformVector(this.A);
			var B = matrix.TransformVector(this.B);
			return DistanceFromPointToLine(A, B, point) <= radius;
		}

		public Vector2 GetDirection(Widget canvas)
		{
			var matrix = Owner.CalcTransitionToSpaceOf(canvas);
			return (matrix * Directions[index] - matrix * Vector2.Zero).Normalized;
		}

		private static float DistanceFromPointToLine(Vector2 A, Vector2 B, Vector2 P)
		{
			var a = A.Y - B.Y;
			var b = B.X - A.X;
			var c = B.Y * A.X - A.Y * B.X;
			return Mathf.Abs(P.X * a + P.Y * b + c) / Mathf.Sqrt(a * a + b * b);
		}

		public static IEnumerable<NineGridLine> GetForNineGrid(NineGrid nineGrid)
		{
			for (int i = 0; i < 4; ++i) {
				yield return new NineGridLine(i, nineGrid);
			}
		}
	}
}
