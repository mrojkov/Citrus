using Lime;
using Orange.FbxImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Orange
{
	public partial class FbxModelImporter
	{
		private string path;
		public Scene scene;

		public FbxModelImporter(string path, TargetPlatform platform)
		{
			scene = Manager.Instance.LoadScene(path);
		}

		public Model3D GetModel()
		{
			var Model = new Model3D();
			Model.Nodes.Add(ImportNodes(scene.Root));
			return Model;
		}

		private Lime.Node ImportNodes(FbxImporter.Node root, Lime.Node parent = null)
		{
			Lime.Node node = null;
			if (root.Attribute != NodeAttribute.Empty ) {
				switch (root.Attribute.Type) {
					case NodeAttribute.FbxNodeType.MESH:
						if ((root.Attribute as MeshAttribute).Vertices.Length != 0) {
							node = GetMeshNode(root.Attribute as MeshAttribute, root);
						}
						break;
				}
			} else {
				node = new Node3D { Id = root.Name };
			}
			if (node != null) {
				if (parent != null) {
					parent.Nodes.Add(node);
				}
				foreach (var child in root.Children) {
					ImportNodes(child, node);
				}
			}

			return node;
		}

		private Node3D GetMeshNode(MeshAttribute meshAttribute, FbxImporter.Node node)
		{
			var mesh = new Mesh3D { Id = node.Name };
			
				var sm = new Submesh3D();
				sm.Mesh = new Mesh {
					VertexBuffers = new[] {
						new VertexBuffer<Mesh3D.Vertex> {
							Data = meshAttribute.Vertices,
						}
					},
					IndexBuffer = new IndexBuffer { Data = meshAttribute.Indices.Select(index => checked((ushort)index)).ToArray() },
					Attributes = new[] { new[] {
						ShaderPrograms.Attributes.Pos1,
						ShaderPrograms.Attributes.Color1,
						ShaderPrograms.Attributes.UV1,
						//ShaderPrograms.Attributes.BlendIndices,
						//ShaderPrograms.Attributes.BlendWeights
					}}
				};
			mesh.Submeshes.Add(sm);
			mesh.SetLocalTransform(node.LocalTransform);
			mesh.RecalcBounds();
			mesh.RecalcCenter();
			return mesh;
		}
	}
}
