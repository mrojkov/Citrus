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
			ImportAnimations();
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
						ShaderPrograms.Attributes.BlendIndices,
						ShaderPrograms.Attributes.BlendWeights
					}}
				};
			sm.Material = node.Material.ToLime(path);
			if (meshAttribute.Bones.Length > 0) {
				foreach (var bone in meshAttribute.Bones) {
					sm.BoneNames.Add(bone.Name);
					sm.BoneBindPoses.Add(bone.Offset);
				}
			}
			mesh.Submeshes.Add(sm);
			mesh.SetLocalTransform(node.LocalTranform);
			mesh.RecalcBounds();
			mesh.RecalcCenter();
			return mesh;
		}

		private void ImportAnimations() {
			foreach(var animation in scene.Animations.Animations) {
				var n = Model.TryFind<Node3D>(animation.Key);
				var animationId = scene.Animations.Name;
				var scaleKeys = new List<Keyframe<Vector3>>();
				var rotationKeys = new List<Keyframe<Quaternion>>();
				var translationKeys = new List<Keyframe<Vector3>>();
				for (int i = 0; i < animation.TimeSteps.Length; i++) {
					var time = AnimationUtils.SecondsToFrames(animation.TimeSteps[i]);
					Vector3 finalScale;
					Quaternion finalRotation;
					Vector3 finalTranslation;
					animation.Transform[i].Decompose(out finalScale, out finalRotation, out finalTranslation);
					scaleKeys.Add(new Keyframe<Vector3>(time, finalScale));
					rotationKeys.Add(new Keyframe<Quaternion>(time, finalRotation));
					translationKeys.Add(new Keyframe<Vector3>(time, finalTranslation));
				}
				
				(n.Animators["Scale", animationId] as Animator<Vector3>).Keys.AddRange(
					Vector3KeyReducer.Default.Reduce(scaleKeys));

				(n.Animators["Rotation", animationId] as Animator<Quaternion>).Keys.AddRange(
					QuaternionKeyReducer.Default.Reduce(rotationKeys));

				(n.Animators["Position", animationId] as Animator<Vector3>).Keys.AddRange(
					Vector3KeyReducer.Default.Reduce(translationKeys));
				Model.Animations.Add(new Lime.Animation {
					Id = animationId
				});
			}
		}
	}
}
