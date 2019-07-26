using System;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	using Polygon = List<Vector2>;

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

	public enum ImageCombinerOperation
	{
		[TangerineIgnore]
		None,
		Multiply,
		CutOut
	}

	[TangerineRegisterNode(Order = 19)]
	[TangerineVisualHintGroup("/All/Nodes/Images")]
	public class ImageCombiner : Node
	{
		[YuzuMember]
		[TangerineKeyframeColor(9)]
		public bool Enabled { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(10)]
		public Blending Blending { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(11)]
		public ShaderId Shader { get; set; }

		[YuzuMember]
		public ImageCombinerOperation Operation { get; set; } = ImageCombinerOperation.Multiply;

		public IMaterial CustomMaterial { get; set; }

		public ImageCombiner()
		{
			Presenter = DefaultPresenter.Instance;
			Enabled = true;
			Blending = Blending.Inherited;
			Shader = ShaderId.Inherited;
		}

		private static bool AreVectorsClockwiseOrdered(Vector2 u, Vector2 v, Vector2 w)
		{
			return (v.Y - u.Y) * (w.X - v.X) > (v.X - u.X) * (w.Y - v.Y);
		}

		public bool GetArgs(out IImageCombinerArg arg1, out IImageCombinerArg arg2)
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
#if TANGERINE
			if (GetTangerineFlag(TangerineFlags.Hidden)) {
				return;
			}
#endif // TANGERINE
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

		protected internal override Lime.RenderObject GetRenderObject()
		{
			if (Parent.AsWidget == null) {
				return null;
			}
			IImageCombinerArg arg1, arg2;
			if (!GetArgs(out arg1, out arg2)) {
				return null;
			}
			if (!arg1.GloballyVisible || !arg2.GloballyVisible) {
				return null;
			}
			var texture1 = arg1.GetTexture();
			var texture2 = arg2.GetTexture();
			if (texture1 == null || texture2 == null) {
				return null;
			}
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.Operation = Operation;
			ro.Arg1Texture = texture1;
			ro.Arg2Texture = texture2;
			ro.Arg1Transform = Matrix32.Scaling(arg1.Size) * arg1.CalcLocalToParentTransform();
			ro.Arg2Transform = Matrix32.Scaling(arg2.Size) * arg2.CalcLocalToParentTransform();
			ro.Arg1UVTransform = arg1.UVTransform;
			ro.Arg2UVTransform = arg2.UVTransform;
			ro.LocalToWorldTransform = Parent.AsWidget.LocalToWorldTransform;
			ro.Color = arg1.Color * arg2.Color * Parent.AsWidget.GlobalColor;
			ro.Arg12CommonMaterial = CustomMaterial;
			ro.Arg1Material = CustomMaterial;
			if (ro.Arg1Material == null) {
				var shader = Shader == ShaderId.Inherited ? Parent.AsWidget.GlobalShader : Shader;
				if (arg2.Shader == ShaderId.Silhuette) {
					shader = ShaderId.Silhuette;
				} else if (arg1.Shader == ShaderId.Silhuette) {
					shader = ShaderId.Silhuette;
					Toolbox.Swap(ref arg1, ref arg2);
					Toolbox.Swap(ref texture1, ref texture2);
				}
				var blending = Blending == Blending.Inherited ? Parent.AsWidget.GlobalBlending : Blending;
				ro.Arg1Material = WidgetMaterial.GetInstance(blending, shader, WidgetMaterial.GetNumTextures(texture1, null));
				ro.Arg12CommonMaterial = WidgetMaterial.GetInstance(
					blending, shader, WidgetMaterial.GetNumTextures(texture1, texture2),
					Operation == ImageCombinerOperation.Multiply ? TextureBlending.Multiply : TextureBlending.CutOut);
			}
			return ro;
		}

		private class RenderObject : Lime.RenderObject
		{
			public ImageCombinerOperation Operation;
			public Matrix32 LocalToWorldTransform;
			public Matrix32 Arg1Transform;
			public Matrix32 Arg2Transform;
			public Matrix32 Arg1UVTransform;
			public Matrix32 Arg2UVTransform;
			public IMaterial Arg1Material;
			public IMaterial Arg12CommonMaterial;
			public ITexture Arg1Texture;
			public ITexture Arg2Texture;
			public Color4 Color;

			private static Polygon arg12CommonArea = new Polygon();
			private static Polygon[] arg1RemainedAreas = new Polygon[4];
			private static int arg1RemainedAreaCount;
			private static Polygon arg2Area = new Polygon();
			private static Polygon splitResult1 = new Polygon();
			private static Polygon splitResult2 = new Polygon();
			private static Vertex[] vertices = new Vertex[10];
			private static Vector2[] rect = {
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(1, 1),
				new Vector2(0, 1)
			};

			public override void Render()
			{
				arg12CommonArea.Clear();
				arg2Area.Clear();
				for (int i = 0; i < 4; i++) {
					arg12CommonArea.Add(rect[i] * Arg1Transform);
					arg2Area.Add(rect[i] * Arg2Transform);
				}
				arg1RemainedAreaCount = 0;
				var clockwiseOrder = AreVectorsClockwiseOrdered(arg2Area[0], arg2Area[1], arg2Area[2]);
				for (int i = 0; i < 4; i++) {
					int j = (i < 3) ? i + 1 : 0;
					var v1 = clockwiseOrder ? arg2Area[j] : arg2Area[i];
					var v2 = clockwiseOrder ? arg2Area[i] : arg2Area[j];
					if (Operation == ImageCombinerOperation.Multiply) {
						SplitPolygon(arg12CommonArea, splitResult1, splitResult2, v1, v2);
						Toolbox.Swap(ref arg12CommonArea, ref splitResult1);
					} else if (Operation == ImageCombinerOperation.CutOut) {
						SplitPolygon(arg12CommonArea, splitResult1, splitResult2, v1, v2);
						Toolbox.Swap(ref arg12CommonArea, ref splitResult1);
						if (splitResult2.Count > 0) {
							Toolbox.Swap(ref splitResult2, ref arg1RemainedAreas[arg1RemainedAreaCount++]);
							if (splitResult2 == null) {
								splitResult2 = new Polygon();
							}
						}
					} else {
						throw new InvalidOperationException();
					}
				}
				// Following matrices transform vertices into texture coordinates.
				var uvTransform1 = Arg1Transform.CalcInversed() * Arg1UVTransform;
				var uvTransform2 = Arg2Transform.CalcInversed() * Arg2UVTransform;
				Renderer.Transform1 = LocalToWorldTransform;
				if (Operation == ImageCombinerOperation.Multiply) {
					PrepareVertices(arg12CommonArea);
					Renderer.DrawTriangleFan(Arg1Texture, Arg2Texture, Arg12CommonMaterial, vertices, arg12CommonArea.Count);
				} else {
					for (int i = 0; i < arg1RemainedAreaCount; i++) {
						PrepareVertices(arg1RemainedAreas[i]);
						Renderer.DrawTriangleFan(Arg1Texture, null, Arg1Material, vertices, arg1RemainedAreas[i].Count);
					}
					PrepareVertices(arg12CommonArea);
					Renderer.DrawTriangleFan(Arg1Texture, Arg2Texture, Arg12CommonMaterial, vertices, arg12CommonArea.Count);
				}

				void PrepareVertices(Polygon polygon)
				{
					for (int i = 0; i < polygon.Count; i++) {
						vertices[i].Pos = polygon[i];
						vertices[i].Color = Color;
						var uv1 = polygon[i] * uvTransform1;
						var uv2 = polygon[i] * uvTransform2;
						vertices[i].UV1 = uv1;
						vertices[i].UV2 = uv2;
					}
				}
			}

			protected override void OnRelease()
			{
				Arg12CommonMaterial = null;
				Arg1Material = null;
				Arg1Texture = null;
				Arg2Texture = null;
			}

			private void SplitPolygon(Polygon polygon, Polygon result1, Polygon result2, Vector2 a, Vector2 b)
			{
				const float Eps = 1e-5f;
				result1.Clear();
				result2.Clear();
				for (int i = 0; i < polygon.Count; i++) {
					int j = (i < polygon.Count - 1) ? i + 1 : 0;
					var u = polygon [i];
					var v = polygon [j];
					float d1 = (u.Y - a.Y) * (b.X - a.X) - (u.X - a.X) * (b.Y - a.Y);
					float d2 = (v.Y - a.Y) * (b.X - a.X) - (v.X - a.X) * (b.Y - a.Y);
					int s1 = Math.Abs(d1) < Eps ? 0 : ((d1 < 0) ? -1 : 1);
					int s2 = Math.Abs(d2) < Eps ? 0 : ((d2 < 0) ? -1 : 1);
					// If the first point lies inside visible half-plane or on the line, then include it into list.
					if (s1 >= 0) {
						result1.Add(u);
					}
					if (s1 <= 0) {
						result2.Add(u);
					}
					// The line crosses the edge.
					if (s1 > 0 && s2 < 0 || s1 < 0 && s2 > 0) {
						float z = (v.X - u.X) * (b.Y - a.Y) - (v.Y - u.Y) * (b.X - a.X);
						float t = d1 / z;
						Vector2 p;
						p.X = u.X + (v.X - u.X) * t;
						p.Y = u.Y + (v.Y - u.Y) * t;
						result1.Add(p);
						result2.Add(p);
					}
				}
				if (result1.Count < 3) {
					result1.Clear();
				}
				if (result2.Count < 3) {
					result2.Clear();
				}
			}
		}
	}
}

