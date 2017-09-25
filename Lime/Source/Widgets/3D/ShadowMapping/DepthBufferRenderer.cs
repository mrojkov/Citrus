using System;
using System.Collections.Generic;

namespace Lime
{
	public interface IShadowCaster
	{
		bool CastShadow { get; set; }
		void RenderDepthBuffer(IMaterial mat);
	}

	public interface IShadowReciever
	{ }

	public class DepthBufferRenderer
	{
		public ITexture Texture
		{ get { return map; } }

		private Viewport3D viewport;
		private DepthBuffer depthmat;
		private ITexture map;

		public DepthBufferRenderer(Viewport3D viewport, int size)
		{
			if (viewport == null) {
				throw new NullReferenceException(nameof(viewport));
			}

			this.viewport = viewport;
			this.map = new RenderTexture(size, size, RenderTextureFormat.RGBA8);
			this.depthmat = new DepthBuffer();
		}

		public void Render(Matrix44 lightViewProjection, WindowRect lightViewport)
		{
			depthmat.ViewProjection = lightViewProjection;
			map.SetAsRenderTarget();

			Renderer.Clear(1.0f, 1.0f, 1.0f, 1.0f);

			var oldViewport = Renderer.Viewport;
			var oldWorld = Renderer.World;
			var oldView = Renderer.View;
			var oldProj = Renderer.Projection;
			var oldZTestEnabled = Renderer.ZTestEnabled;
			var oldZWriteEnabled = Renderer.ZWriteEnabled;
			var oldCullMode = Renderer.CullMode;

			Renderer.Flush();
			Renderer.Viewport = lightViewport;
			Renderer.World = Matrix44.Identity;

			var list = new List<RenderItem>();
			var chain = new RenderChain();
			foreach (var node in viewport.Nodes) {
				node.AddToRenderChain(chain);
			}

			for (var i = 0; i < RenderChain.LayerCount; i++) {
				var layer = chain.Layers[i];
				if (layer == null || layer.Count == 0) {
					continue;
				}
				for (var j = 0; j < layer.Count; j++) {
					var node = layer[j].Node.AsNode3D;
					list.Add(new RenderItem {
						Node = node,
						Distance = node.CalcDistanceToCamera(viewport.Camera)
					});
				}

				list.Sort(RenderOrderComparers.FrontToBack);
				for (int j = 0; j < list.Count; ++j) {
					RenderNode(list[j].Node);
				}
			}

			chain.Clear();

			Renderer.DrawRect(new Vector2(0, 0), new Vector2(1536, 1536), Color4.Red);

			Renderer.World = oldWorld;
			Renderer.Viewport = oldViewport;
			Renderer.View = oldView;
			Renderer.Projection = oldProj;
			Renderer.ZTestEnabled = oldZTestEnabled;
			Renderer.ZWriteEnabled = oldZWriteEnabled;
			Renderer.CullMode = oldCullMode;

			map.RestoreRenderTarget();
		}

		private void RenderNode(Node node)
		{
			var chain = new RenderChain();
			for (int i = 0; i < node.Nodes.Count; ++i) {
				node.Nodes[i].AddToRenderChain(chain);
			}

			for (int i = 0; i < chain.Layers.Length; i++) {
				var list = chain.Layers[i];
				if (list == null || list.Count == 0) {
					continue;
				}

				for (int j = list.Count - 1; j >= 0; j--) {
					var t = list[j];
					var caster = t.Node as IShadowCaster;
					if (caster != null && caster.CastShadow)
						caster.RenderDepthBuffer(depthmat);
				}

				list.Clear();
			}
		}

		#region Render Item

		private static class RenderOrderComparers
		{
			public static readonly BackToFrontComparer BackToFront = new BackToFrontComparer();
			public static readonly FrontToBackComparer FrontToBack = new FrontToBackComparer();
		}

		private class BackToFrontComparer : Comparer<RenderItem>
		{
			public override int Compare(RenderItem x, RenderItem y)
			{
				return x.Distance.CompareTo(y.Distance);
			}
		}

		private class FrontToBackComparer : Comparer<RenderItem>
		{
			public override int Compare(RenderItem x, RenderItem y)
			{
				return y.Distance.CompareTo(x.Distance);
			}
		}

		private struct RenderItem
		{
			public Node Node;
			public float Distance;
		}

		#endregion
	}
}
