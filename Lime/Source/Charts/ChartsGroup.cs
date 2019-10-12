using System;

namespace Lime
{
	public abstract class ChartsGroup : Widget
	{
		protected const int ChartMaxHeight = 100;
		protected const int NonChartsLinesCount = 4;
		protected const int ChartsVerticesOffset = NonChartsLinesCount * 6;
		protected const int ControlPointsSpacing = 4;

		public readonly ChartMaterial Material;
		public readonly Mesh<Vector3> Mesh;
		protected readonly Vector3[] vertices;

		public readonly string[] Names;
		public readonly Vector4[] Colors;

		protected readonly int submeshVerticesCount;
		protected readonly int controlPointsCount;

		public class Submesh
		{
			public bool Enable = false;
			public int ColorIndex = -1;
			public float[] Points = null;
		}
		protected Submesh[] submeshes;

		public ChartsGroup(string[] chartsNames, int controlPointsCount)
		{
			this.controlPointsCount = controlPointsCount;
			submeshVerticesCount = 2 * controlPointsCount;
			vertices = new Vector3[ChartsVerticesOffset + submeshVerticesCount * chartsNames.Length];
			{
				float maxX = controlPointsCount * ControlPointsSpacing;
				float fpsConvertConst = ChartMaxHeight * 0.9f / 60.0f;
				int[] colors = new int[3] { 12, 0, 11 };
				for (int i = 0; i < 3; i++) { // 15 30 60 fps
					float fpsHeight = ChartMaxHeight - fpsConvertConst * 15.0f * Mathf.Pow(2, i);
					vertices[i * 6 + 0] = new Vector3((i % 2) * maxX, fpsHeight + 1 * (i % 2), colors[i]);
					vertices[i * 6 + 1] = new Vector3((i % 2) * maxX, fpsHeight + 1 * (1 - i % 2), colors[i]);
					vertices[i * 6 + 2] = new Vector3((1 - i % 2) * maxX, fpsHeight + 1 * (i % 2), colors[i]);
					vertices[i * 6 + 3] = new Vector3((i % 2) * maxX, fpsHeight + 1 * (i % 2), colors[i]);
					vertices[i * 6 + 4] = new Vector3((1 - i % 2) * maxX, fpsHeight + 1 * (i % 2), colors[i]);
					vertices[i * 6 + 5] = new Vector3((1 - i % 2) * maxX, fpsHeight + 1 * (1 - i % 2), colors[i]);
				}
				vertices[18] = new Vector3(200, ChartMaxHeight * 0.1f, 0);
				vertices[19] = new Vector3(201, ChartMaxHeight * 0.1f, 0);
				vertices[20] = new Vector3(201, ChartMaxHeight, 0);
				vertices[21] = new Vector3(200, ChartMaxHeight * 0.1f, 0);
				vertices[22] = new Vector3(201, ChartMaxHeight, 0);
				vertices[23] = new Vector3(200, ChartMaxHeight, 0);
			}
			Names = chartsNames;
			Colors = new Vector4[13] {
				Color4.White.ToVector4(),
				new Color4(76, 176, 80).ToVector4(), new Color4(33, 150, 243).ToVector4(), new Color4(254, 87, 34).ToVector4(),
				new Color4(255, 235, 60).ToVector4(), new Color4(0, 150, 136).ToVector4(), new Color4(165, 208, 97).ToVector4(),
				new Color4(62, 115, 181).ToVector4(), new Color4(234, 30, 99).ToVector4(), new Color4(96, 105, 139).ToVector4(),
				new Color4(121, 85, 71).ToVector4(), new Color4(0, 255, 0).ToVector4(), new Color4(255, 0, 0).ToVector4()
			};
			Material = new ChartMaterial() { Colors = Colors };
			Mesh = new Mesh<Vector3> {
				Vertices = vertices,
				Topology = PrimitiveTopology.TriangleStrip,
				AttributeLocations = ChartMaterial.ShaderProgram.MeshAttribLocations,
				DirtyFlags = MeshDirtyFlags.Vertices | MeshDirtyFlags.AttributeLocations
			};
			submeshes = new Submesh[Names.Length];
			for (int j = ChartsVerticesOffset, i = 0; i < submeshes.Length; i++) {
				var submesh = new Submesh();
				submeshes[i] = submesh;
				submesh.Enable = true;
				submesh.ColorIndex = i + 1;
				submesh.Points = new float[controlPointsCount];
				for (int k = 0; k < submeshVerticesCount; k++, j++) {
					vertices[j] = new Vector3(
						/* position X */ (k / 2) * ControlPointsSpacing,
						/* position Y */ (k % 2) * 0,
						/* color index */ i + 1);
				}
			}
			Layout = new VBoxLayout();
			Presenter = new ChartsPresenter(this);
		}

		public void SetActive(int chartIndex, bool value) => submeshes[chartIndex].Enable = value;

		public int GetActiveChartsCount()
		{
			int activeCount = 0;
			foreach (var element in submeshes) {
				activeCount += element.Enable ? 1 : 0;
			}
			return activeCount;
		}

		public int GetActiveVerticesCount() => ChartsVerticesOffset + submeshVerticesCount * GetActiveChartsCount();

		public abstract void PushControlPoints(float[] points);

		public abstract void UpdateVertices();

		private class ChartsPresenter : IPresenter
		{
			private ChartsGroup chartsGroup;

			public ChartsPresenter(ChartsGroup chartsGroup)
			{
				this.chartsGroup = chartsGroup;
			}

			public bool PartialHitTest(Node node, ref HitTestArgs args) => node.PartialHitTest(ref args);

			public Lime.RenderObject GetRenderObject(Node node)
			{
				var widget = (Widget)node;
				var ro = RenderObjectPool<RenderObject>.Acquire();
				ro.CaptureRenderState(widget);
				ro.chartsGroup = chartsGroup;
				return ro;
			}

			public IPresenter Clone() => (IPresenter)MemberwiseClone();

			private class RenderObject : WidgetRenderObject
			{
				public ChartsGroup chartsGroup;

				private Random random = new Random();

				public override void Render()
				{
					Renderer.MainRenderList.Flush();

					var material = chartsGroup.Material;
					material.Matrix = Renderer.FixupWVP((Matrix44)LocalToWorldTransform * Renderer.WorldViewProjection);
					material.Apply(0);

					var newVertices = new float[] { random.Next(0, 30), random.Next(0, 30), random.Next(0, 30) };
					chartsGroup.PushControlPoints(newVertices);
					chartsGroup.UpdateVertices();

					var mesh = chartsGroup.Mesh;
					mesh.Topology = PrimitiveTopology.TriangleStrip;
					mesh.Draw(ChartsVerticesOffset, chartsGroup.GetActiveChartsCount() * chartsGroup.submeshVerticesCount);
					mesh.Topology = PrimitiveTopology.TriangleList;
					mesh.Draw(0, ChartsVerticesOffset);
				}
			}
		}
	}

	public class ChartLegend : Widget
	{
		public ChartLegend(ChartsGroup chartsGroup)
		{
			Layout = new VBoxLayout { Spacing = 6 };
			Padding = new Thickness(4, 8);
			var names = chartsGroup.Names;
			var colors = chartsGroup.Colors;
			for (int i = 0; i < names.Length; i++) {
				var widget = new Widget {
					Layout = new HBoxLayout { Spacing = 6 }
				};
				var color = colors[i + 1];
				var checkBox = new ColorCheckBox(Color4.FromFloats(color.X, color.Y, color.Z));
				int iterationIndex = i;
				checkBox.Changed += args => {
					chartsGroup.SetActive(iterationIndex, args.Value);
				};
				widget.AddNode(checkBox);
				widget.AddNode(
					new SimpleText {
						Text = names[i],
						Color = Theme.Colors.BlackText
					}
				);
				AddNode(widget);
			}
		}
	}
}
