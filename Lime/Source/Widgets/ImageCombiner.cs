using System;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// This interface must implements every node which can be used as a source for ImageCombiner.
	/// </summary>
	public interface IImageCombinerArg
	{
		/// <summary>
		/// Called by ImageCombiner in update cycle.
		/// It notifies that widget will be used in combining, and
		/// must not be drawn on render pass.
		/// </summary>
		void SkipRender();

		ITexture GetTexture();

		Vector2 Size { get; }

		Color4 Color { get; }

		Matrix32 CalcLocalToParentTransform();

		bool GloballyVisible { get; }

		Blending Blending { get; }

		ShaderId Shader { get; }

		Matrix32 UVTransform { get; }
	}

	public class ImageCombiner : Node
	{
		[YuzuMember]
		public bool Enabled { get; set; }

		[YuzuMember]
		public Blending Blending { get; set; }

		[YuzuMember]
		public ShaderId Shader { get; set; }

		public ShaderProgram CustomShaderProgram;

		public ImageCombiner()
		{
			Presenter = DefaultPresenter.Instance;
			Enabled = true;
			Blending = Blending.Inherited;
			Shader = ShaderId.Inherited;
		}

		static bool AreVectorsClockwiseOrdered(Vector2 u, Vector2 v, Vector2 w)
		{
			return (v.Y - u.Y) * (w.X - v.X) > (v.X - u.X) * (w.Y - v.Y);
		}

		bool GetArgs(out IImageCombinerArg arg1, out IImageCombinerArg arg2)
		{
			if (Parent != null) {
				int index = Parent.Nodes.IndexOf(this);
				if (index < Parent.Nodes.Count - 2) {
					arg1 = Parent.Nodes[index + 1] as IImageCombinerArg;
					arg2 = Parent.Nodes[index + 2] as IImageCombinerArg;
					if (arg1 != null & arg2 != null)
						return true;
				}
			}
			arg1 = arg2 = null;
			return false;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (Enabled) {
				IImageCombinerArg arg1, arg2;
				if (GetArgs(out arg1, out arg2)) {
					arg1.SkipRender();
					arg2.SkipRender();
					var widget1 = arg1 as Widget;
					var widget2 = arg2 as Widget;
					if (
						widget1 != null && !widget1.ClipRegionTest(chain.ClipRegion) || 
						widget2 != null && !widget2.ClipRegionTest(chain.ClipRegion)
					) {
						return;
					}
				}
				AddSelfToRenderChain(chain, Layer);
			}
		}

		static readonly Vector2[] outVertices = new Vector2[64];

		private void ClipPolygonByLine(Vector2[] vertices, ref int numVertices, Vector2 a, Vector2 b)
		{
			const float Eps = 1e-5f;
			int numOutVertices = 0;
			for (int i = 0; i < numVertices; i++) {
				int j = (i < numVertices - 1) ? i + 1 : 0;
				Vector2 u = vertices[i];
				Vector2 v = vertices[j];

				float d1 = (u.Y - a.Y) * (b.X - a.X) - (u.X - a.X) * (b.Y - a.Y);
				float d2 = (v.Y - a.Y) * (b.X - a.X) - (v.X - a.X) * (b.Y - a.Y);

				int s1 = Math.Abs(d1) < Eps ? 0 : ((d1 < 0) ? -1 : 1);
				int s2 = Math.Abs(d2) < Eps ? 0 : ((d2 < 0) ? -1 : 1);

				// if the first point lies inside visible half-plane or on the line, then include it into list.
				if (s1 >= 0)
					outVertices[numOutVertices++] = u;
				// the line crosses the edge.
				if (s1 > 0 && s2 < 0 || s1 < 0 && s2 > 0) {
					float z = (v.X - u.X) * (b.Y - a.Y) - (v.Y - u.Y) * (b.X - a.X);
					float t = d1 / z;
					Vector2 p;
					p.X = u.X + (v.X - u.X) * t;
					p.Y = u.Y + (v.Y - u.Y) * t;
					outVertices[numOutVertices++] = p;
				}
			}
			if (numOutVertices < 3)
				numVertices = 0;
			else {
				for (int i = 0; i < numOutVertices; i++)
					vertices[i] = outVertices[i];
				numVertices = numOutVertices;
			}
		}

		private void GenerateNewPoligon(Vector2[] arg1, Vector2[] arg2, Vector2[] outCoords)
		{
			float minX = float.MaxValue;
			float minY = float.MaxValue; ;
			float maxX = float.MinValue; ;
			float maxY = float.MinValue; ;
			for (int i = 0; i < 4; i++) {
				minX = Mathf.Min(minX, Mathf.Min(arg1[i].X, arg2[i].X));
				minY = Mathf.Min(minY, Mathf.Min(arg1[i].Y, arg2[i].Y));
				maxX = Mathf.Max(maxX, Mathf.Max(arg1[i].X, arg2[i].X));
				maxY = Mathf.Max(maxY, Mathf.Max(arg1[i].Y, arg2[i].Y));
			}
			outCoords[0].X = minX;
			outCoords[0].Y = minY;
			outCoords[1].X = maxX;
			outCoords[1].Y = minY;
			outCoords[2].X = maxX;
			outCoords[2].Y = maxY;
			outCoords[3].X = minX;
			outCoords[3].Y = maxY;
		}

		static readonly Vector2[] coords1 = new Vector2[4];
		static readonly Vector2[] coords2 = new Vector2[4];
		static readonly Vector2[] cropCoords = new Vector2[8];
		static readonly Vector2[] newPoligonCoords = new Vector2[4];
		static readonly Vertex[] vertices = new Vertex[8];
		static readonly Vector2[] rect = new Vector2[4] {
			new Vector2(0, 0),
			new Vector2(1, 0),
			new Vector2(1, 1),
			new Vector2(0, 1)
		};

		private void RenderHelper(IImageCombinerArg arg1, IImageCombinerArg arg2)
		{
			Matrix32 transform1 = Matrix32.Scaling(arg1.Size) * arg1.CalcLocalToParentTransform();
			Matrix32 transform2 = Matrix32.Scaling(arg2.Size) * arg2.CalcLocalToParentTransform();
			// source rectangle
			int numCoords = 4;
			for (int i = 0; i < 4; i++) {
				coords1[i] = rect[i] * transform1;
				cropCoords[i] = coords1[i];
			}
			for (int i = 0; i < 4; i++)
				coords2[i] = rect[i] * transform2;
			

			// Эти матрицы переводят координаты вершин изображения в текстурные координаты.
			Matrix32 uvTransform1 = transform1.CalcInversed();
			Matrix32 uvTransform2 = transform2.CalcInversed();
			ITexture texture1 = arg1.GetTexture();
			ITexture texture2 = arg2.GetTexture();
			Color4 color = arg1.Color * arg2.Color * Parent.AsWidget.GlobalColor;




			if (Shader == ShaderId.Silhuette || Shader == ShaderId.Inherited) {

				bool clockwiseOrder = AreVectorsClockwiseOrdered(coords2[0], coords2[1], coords2[2]);

				// clip invisible parts
				for (int i = 0; i < 4; i++) {
					int j = (i < 3) ? i + 1 : 0;
					Vector2 v1 = clockwiseOrder ? coords2[j] : coords2[i];
					Vector2 v2 = clockwiseOrder ? coords2[i] : coords2[j];
					ClipPolygonByLine(cropCoords, ref numCoords, v1, v2);
				}

				if (numCoords < 3)
					return;
				for (int i = 0; i < numCoords; i++) {
					vertices[i].Pos = cropCoords[i];
					vertices[i].Color = color;
					vertices[i].UV1 = cropCoords[i] * uvTransform1 * arg1.UVTransform;
					vertices[i].UV2 = cropCoords[i] * uvTransform2 * arg2.UVTransform;
				}
				Renderer.DrawTriangleFan(texture1, texture2, vertices, numCoords);
			}else if (Shader == ShaderId.InversedSilhuette) { 
				for (int i = 0; i < 4; i++) {
					vertices[i].Pos = coords1[i];
					vertices[i].Color = color;
					vertices[i].UV1 = coords1[i] * uvTransform1 * arg1.UVTransform;
					vertices[i].UV2 = coords1[i] * uvTransform2 * arg2.UVTransform;
				}

				Renderer.DrawTriangleFan(texture1, texture2, vertices, 4);
			}else if (Shader == ShaderId.VisibleMaskSilhouette) {
				for (int i = 0; i < 4; i++){
					vertices[i].Pos = coords2[i];
					vertices[i].Color = color;
					vertices[i].UV1 = coords2[i] * uvTransform1 * arg1.UVTransform;
					vertices[i].UV2 = coords2[i] * uvTransform2 * arg2.UVTransform;
				}

				Renderer.DrawTriangleFan(texture1, texture2, vertices, 4);
			}else if (Shader == ShaderId.Sum || Shader == ShaderId.Subtract) {
				GenerateNewPoligon(coords1, coords2, newPoligonCoords);

				for (int i = 0; i < 4; i++) {
					vertices[i].Pos = newPoligonCoords[i];
					vertices[i].Color = color;
					vertices[i].UV1 = newPoligonCoords[i] * uvTransform1 * arg1.UVTransform;
					vertices[i].UV2 = newPoligonCoords[i] * uvTransform2 * arg2.UVTransform;
				}

				Renderer.DrawTriangleFan(texture1, texture2, vertices, 4);
			}

		}

		public override void Render()
		{
			if (Parent.AsWidget == null) {
				return;
			}
			IImageCombinerArg arg1, arg2;
			if (!GetArgs(out arg1, out arg2)) {
				return;
			}
			if (!arg1.GloballyVisible || !arg2.GloballyVisible) {
				return;
			}
			if (arg1.GetTexture() == null || arg2.GetTexture() == null) {
				return;
			}
			Renderer.Transform1 = Parent.AsWidget.LocalToWorldTransform;
			Renderer.Blending = Blending == Blending.Inherited ? Parent.AsWidget.GlobalBlending : Blending;
			var shader = Shader == ShaderId.Inherited ? Parent.AsWidget.GlobalShader : Shader;
			if (arg2.Shader == ShaderId.Silhuette) {
				Renderer.Shader = ShaderId.Silhuette;
			} else if (arg1.Shader == ShaderId.Silhuette) {
				Renderer.Shader = ShaderId.Silhuette;
				Toolbox.Swap(ref arg1, ref arg2);
			} else {
				Renderer.Shader = shader;
			}
			if (Renderer.Shader == ShaderId.Custom) {
				Renderer.CustomShaderProgram = CustomShaderProgram;
			}
			RenderHelper(arg1, arg2);
		}
	}
}

