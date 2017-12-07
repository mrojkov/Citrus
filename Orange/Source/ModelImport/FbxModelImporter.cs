using Lime;
using Orange.FbxImporter;
using System.Collections.Generic;
using System.Linq;

namespace Orange
{
	public partial class FbxModelImporter
	{
		/*
		* According to the fbx-documentation the camera's forward vector
		* points along a node's positive X axis.
		* so we have to rotate it by 90 around the Y-axis to correct it.
		*/
		private readonly static Quaternion CameraPostRotation = Quaternion.CreateFromEulerAngles(Vector3.UnitY * -Mathf.Pi / 2);
		private string path;
		private Manager manager;
		private TargetPlatform platform;
		public Model3D Model { get; private set; }

		public FbxModelImporter(string path, TargetPlatform platform)
		{
			this.platform = platform;
			this.path = path;
			manager = Manager.Create();
			var scene = manager.LoadScene(path);
			Model = new Model3D();
			Model.Nodes.Add(ImportNodes(scene.Root));
			ImportAnimations(scene);
			new Model3DAttachmentParser().Parse(path, useBundle: false)?.ApplyOnImport(Model);
			manager.Destroy();
		}

		private Lime.Node ImportNodes(FbxImporter.Node root, Lime.Node parent = null)
		{
			Node3D node = null;
			if (root == null)
				return null;
			switch (root.Attribute.Type) {
				case NodeAttribute.FbxNodeType.MESH:
					var meshAttribute = root.Attribute as MeshAttribute;
					var mesh = new Mesh3D { Id = root.Name };
					foreach (var submesh in meshAttribute.Submeshes) {
						mesh.Submeshes.Add(ImportSubmesh(submesh, root));
					}
					if (platform == TargetPlatform.Unity) {
						mesh.CullMode = CullMode.CullCounterClockwise;
					}
					node = mesh;
					if (mesh.Submeshes.Count != 0) {
						mesh.SetLocalTransform(root.LocalTranform);
						mesh.RecalcBounds();
						mesh.RecalcCenter();
					}
					break;
				case NodeAttribute.FbxNodeType.CAMERA:
					var cam = root.Attribute as CameraAttribute;
					node = new Camera3D {
						Id = root.Name,
						FieldOfView = cam.FieldOfView * Mathf.DegToRad,
						AspectRatio = cam.AspectRatio,
						NearClipPlane = cam.NearClipPlane,
						FarClipPlane = cam.FarClipPlane,
					};
					node.SetLocalTransform(CorrectCameraTransform(root.LocalTranform));
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

		private Submesh3D ImportSubmesh(Submesh meshAttribute, FbxImporter.Node node)
		{
			var sm = new Submesh3D();
			sm.Mesh = new Mesh {
				VertexBuffers = new[] {
					new VertexBuffer<Mesh3D.Vertex> {
						Data = meshAttribute.Vertices,
					},
				},
				IndexBuffer = new IndexBuffer { Data = meshAttribute.Indices.Select(index => checked((ushort)index)).ToArray() },
				Attributes = new[] { new[] {
					ShaderPrograms.Attributes.Pos1,
					ShaderPrograms.Attributes.Color1,
					ShaderPrograms.Attributes.UV1,
					ShaderPrograms.Attributes.BlendIndices,
					ShaderPrograms.Attributes.BlendWeights,
					ShaderPrograms.Attributes.Normal
				}}
			};

			sm.Material = meshAttribute.MaterialIndex != -1 ?
				node.Materials[meshAttribute.MaterialIndex].ToLime(path) : Material.Default;

			if (meshAttribute.Bones.Length > 0) {
				foreach (var bone in meshAttribute.Bones) {
					sm.BoneNames.Add(bone.Name);
					sm.BoneBindPoses.Add(bone.Offset);
				}
			}
			return sm;
		}

		private Matrix44 CorrectCameraTransform(Matrix44 origin)
		{
			Matrix44 rotationMatrix;
			Vector3 translation;
			Vector3 scale;
			origin.Decompose(out scale, out rotationMatrix, out translation);
			var newRotation = rotationMatrix.Rotation * CameraPostRotation;
			return Matrix44.CreateRotation(newRotation) * Matrix44.CreateScale(scale) * Matrix44.CreateTranslation(translation);
		}

		private void CorrectCameraAnimationKeys(IEnumerable<Keyframe<Quaternion>> keys)
		{
			foreach (var key in keys) {
				key.Value *= CameraPostRotation;
			}
		}

		private void ImportAnimations(Scene scene)
		{
			foreach (var animation in scene.Animations.Animations) {
				var n = Model.TryFind<Node3D>(animation.Key);
				(n.Animators["Scale", animation.MarkerId] as Animator<Vector3>).Keys.AddRange(
					Vector3KeyReducer.Default.Reduce(animation.scaleKeys));
				var rotAnimator = n.Animators["Rotation", animation.MarkerId] as Animator<Quaternion>;
				rotAnimator.Keys.AddRange(
					QuaternionKeyReducer.Default.Reduce(animation.rotationKeys));
				if (n is Camera3D) {
					CorrectCameraAnimationKeys(rotAnimator.Keys);
				}
				(n.Animators["Position", animation.MarkerId] as Animator<Vector3>).Keys.AddRange(
					Vector3KeyReducer.Default.Reduce(animation.positionKeys));
				if (!Model.Animations.Any(a => a.Id == animation.MarkerId)) {
					Model.Animations.Add(new Lime.Animation {
						Id = animation.MarkerId,
					});
				}
			}
		}
	}
}
