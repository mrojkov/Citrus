using Lime;
using Orange.FbxImporter;
using System.Collections.Generic;
using System.Linq;

namespace Orange
{
	public partial class FbxModelImporter
	{
		private string path;
		private Scene scene;

		public Model3D Model { get; private set; }

		public FbxModelImporter(string path, TargetPlatform platform)
		{
			this.path = path;
			scene = Manager.Instance.LoadScene(path);
			Model = new Model3D();
			Model.Nodes.Add(ImportNodes(scene.Root));
		}

		private Lime.Node ImportNodes(FbxImporter.Node root, Lime.Node parent = null)
		{
			Node3D node = null;
			if (root == null)
				return null;
			switch (root.Attribute.Type) {
				case NodeAttribute.FbxNodeType.MESH:
					if ((root.Attribute as MeshAttribute).Vertices.Length != 0) {
						node = GetMeshNode(root.Attribute as MeshAttribute, root);
					}
					break;
				default: 
					node = new Node3D { Id = root.Name };
					node.SetLocalTransform(root.LocalTranform);
					break;
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
			sm.Material = node.Material.ToLime(path);
			mesh.Submeshes.Add(sm);
			mesh.SetLocalTransform(node.LocalTranform);
			mesh.RecalcBounds();
			mesh.RecalcCenter();
			return mesh;
		}
	}
}
