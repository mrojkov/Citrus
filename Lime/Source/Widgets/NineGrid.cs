using System;
using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(Order = 13)]
	public class NineGrid : Widget
	{
		[YuzuMember]
		[TangerineKeyframeColor(14)]
		public override ITexture Texture { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(15)]
		public float LeftOffset { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(16)]
		public float RightOffset { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(17)]
		public float TopOffset { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(18)]
		public float BottomOffset { get; set; }

		public struct Part
		{
			public Rectangle Rect;
			public Rectangle UV;
		};

		Part[] layout = new Part[9];

		public Part[] Parts => layout;
		public NineGridLine[] Lines { get; } = new NineGridLine[4];

		public NineGrid()
		{
			Presenter = DefaultPresenter.Instance;
			HitTestMethod = HitTestMethod.Contents;
			Texture = new SerializableTexture();
			Lines[0] = new NineGridLine(5, 2, this);
			Lines[1] = new NineGridLine(1 ,6, this);
			Lines[2] = new NineGridLine(7, 1, this);
			Lines[3] = new NineGridLine(2, 8, this);
		}

		void BuildLayout(Part[] layout)
		{
			Vector2 textureSize = (Vector2)Texture.ImageSize;

			float leftPart = LeftOffset * textureSize.X;
			float topPart = TopOffset * textureSize.Y;
			float rightPart = RightOffset * textureSize.X;
			float bottomPart = BottomOffset * textureSize.Y;

			float tx0 = 0;
			float tx1 = LeftOffset;
			float tx2 = 1 - RightOffset;
			float tx3 = 1;

			float ty0 = 0;
			float ty1 = TopOffset;
			float ty2 = 1 - BottomOffset;
			float ty3 = 1;

			Vector2 gridSize = Size;
			bool flipX = false;
			bool flipY = false;
			if (gridSize.X < 0) {
				gridSize.X = -gridSize.X;
				flipX = true;
			}
			if (gridSize.Y < 0) {
				gridSize.Y = -gridSize.Y;
				flipY = true;
			}
			// If grid width less than texture width, then uniform scale texture by width.
			if (gridSize.X < textureSize.X) {
				leftPart = rightPart = 0;
				tx0 = tx1 = 0;
				tx2 = tx3 = 1;
			}
			// If grid height less than texture height, then uniform scale texture by height.
			if (gridSize.Y < textureSize.Y) {
				topPart = bottomPart = 0;
				ty0 = ty1 = 0;
				ty2 = ty3 = 1;
			}
			// Corners
			layout[0].Rect = new Rectangle(0, 0, leftPart, topPart);
			layout[0].UV = new Rectangle(tx0, ty0, tx1, ty1);
			layout[1].Rect = new Rectangle(gridSize.X - rightPart, 0, gridSize.X, topPart);
			layout[1].UV = new Rectangle(tx2, ty0, tx3, ty1);
			layout[2].Rect = new Rectangle(0, gridSize.Y - bottomPart, leftPart, gridSize.Y);
			layout[2].UV = new Rectangle(tx0, ty2, tx1, ty3);
			layout[3].Rect = new Rectangle(gridSize.X - rightPart, gridSize.Y - bottomPart, gridSize.X, gridSize.Y);
			layout[3].UV = new Rectangle(tx2, ty2, tx3, ty3);
			// Central part
			layout[4].Rect = new Rectangle(leftPart, topPart, gridSize.X - rightPart, gridSize.Y - bottomPart);
			layout[4].UV = new Rectangle(tx1, ty1, tx2, ty2);
			// Sides
			layout[5].Rect = new Rectangle(leftPart, 0, gridSize.X - rightPart, topPart);
			layout[5].UV = new Rectangle(tx1, ty0, tx2, ty1);
			layout[6].Rect = new Rectangle(leftPart, gridSize.Y - bottomPart, gridSize.X - rightPart, gridSize.Y);
			layout[6].UV = new Rectangle(tx1, ty2, tx2, ty3);
			layout[7].Rect = new Rectangle(0, topPart, leftPart, gridSize.Y - bottomPart);
			layout[7].UV = new Rectangle(tx0, ty1, tx1, ty2);
			layout[8].Rect = new Rectangle(gridSize.X - rightPart, topPart, gridSize.X, gridSize.Y - bottomPart);
			layout[8].UV = new Rectangle(tx2, ty1, tx3, ty2);
			for (int i = 0; i < 9; i++) {
				if (flipX) {
					layout[i].Rect.AX = -layout[i].Rect.AX;
					layout[i].Rect.BX = -layout[i].Rect.BX;
				}
				if (flipY) {
					layout[i].Rect.AY = -layout[i].Rect.AY;
					layout[i].Rect.BY = -layout[i].Rect.BY;
				}
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && ClipRegionTest(chain.ClipRegion)) {
				AddSelfAndChildrenToRenderChain(chain, Layer);
			}
		}

		public override void Render()
		{
			BuildLayout(layout);
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.Blending = GlobalBlending;
			Renderer.Shader = GlobalShader;
			for (int i = 0; i < layout.Length; i++) {
				var part = layout[i];
				Renderer.DrawSprite(Texture, GlobalColor, part.Rect.A, part.Rect.Size, part.UV.A, part.UV.B);
			}
		}

		internal protected override bool PartialHitTestByContents (ref HitTestArgs args)
		{
			BuildLayout(layout);
			for (int i = 0; i < layout.Length; i++) {
				if (PartHitTest(layout[i], args.Point))
					return true;
			}
			return false;
		}

		bool PartHitTest(Part part, Vector2 point)
		{
			point = LocalToWorldTransform.CalcInversed().TransformVector(point);
			if (part.Rect.BX < part.Rect.AX) {
				part.Rect.AX = -part.Rect.AX;
				part.Rect.BX = -part.Rect.BX;
				point.X = -point.X;
			}
			if (part.Rect.BY < part.Rect.AY) {
				part.Rect.AY = -part.Rect.AY;
				part.Rect.BY = -part.Rect.BY;
				point.Y = -point.Y;
			}
			if (point.X >= part.Rect.AX && point.Y >= part.Rect.AY && point.X < part.Rect.BX && point.Y < part.Rect.BY) {
				float uf = (point.X - part.Rect.AX) / part.Rect.Width * part.UV.Width + part.UV.AX;
				float vf = (point.Y - part.Rect.AY) / part.Rect.Height * part.UV.Height + part.UV.AY;
				int ui = (int)(Texture.ImageSize.Width * uf);
				int vi = (int)(Texture.ImageSize.Height * vf);
				return !Texture.IsTransparentPixel(ui, vi);
			}
			return false;
		}

		private static float DistanceFromPointToLine(Vector2 A, Vector2 B, Vector2 P)
		{
			var a = A.Y - B.Y;
			var b = B.X - A.X;
			var c = B.Y * A.X - A.Y * B.X;
			return Mathf.Abs(P.X * a + P.Y * b + c) / Mathf.Sqrt(a * a + b * b);
		}

		public bool HitTest(Vector2 point, Widget canvas)
		{
			var matrix = CalcTransitionToSpaceOf(canvas);
			var A = matrix.TransformVector(new Vector2(0, 0));
			var B = matrix.TransformVector(new Vector2(Width, 0));
			var C = matrix.TransformVector(new Vector2(Width, Height));
			var D = matrix.TransformVector(new Vector2(0, Height));
			return DistanceFromPointToLine(A, B, point) <= Height
				&& DistanceFromPointToLine(A, D, point) <= Width
				&& DistanceFromPointToLine(D, C, point) <= Height
				&& DistanceFromPointToLine(B, C, point) <= Width;
		}

		public class NineGridLine
		{
			private readonly int indexA;
			private readonly int indexB;
			private readonly NineGrid nineGrid;
			private Vector2 A => nineGrid.Parts[indexA].Rect.A;
			private Vector2 B => nineGrid.Parts[indexB].Rect.B;

			public NineGridLine(int indexA, int indexB, NineGrid nineGrid)
			{
				this.indexA = indexA;
				this.indexB = indexB;
				this.nineGrid = nineGrid;
			}

			public void Render(Widget canvas)
			{
				var matrix = nineGrid.CalcTransitionToSpaceOf(canvas);
				var A = matrix.TransformVector(this.A);
				var B = matrix.TransformVector(this.B);
				Renderer.DrawLine(A, B, Color4.Red);
			}

			public bool HitTest(Vector2 point, Widget canvas, float radius = 10)
			{
				var matrix = nineGrid.CalcTransitionToSpaceOf(canvas);
				var A = matrix.TransformVector(this.A);
				var B = matrix.TransformVector(this.B);
				return NineGrid.DistanceFromPointToLine(A, B, point) <= radius;
			}
		}
	}
}
