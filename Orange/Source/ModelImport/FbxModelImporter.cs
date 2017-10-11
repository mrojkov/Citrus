using Lime;
using Orange.FbxImporter;
using System.Collections.Generic;
using System.Linq;

namespace Orange
{
	public partial class FbxModelImporter
	{
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
			new Model3DAttachmentParser().Parse(path, useBundle: false)?.ApplyScaleFactor(Model);
			manager.Destroy();
		}

		private Lime.Node ImportNodes(FbxImporter.Node root, Lime.Node parent = null)
		{
			Node3D node = null;
			if (root == null)
				return null;
			switch (root.Attributes[0].Type) {
				case NodeAttribute.FbxNodeType.MESH:
					var mesh = new Mesh3D { Id = root.Name };
					foreach (var attribute in root.Attributes) {
						if ((attribute as MeshAttribute).Vertices.Length > 0) {
							mesh.Submeshes.Add(ImportSubmesh(attribute as MeshAttribute, root));
						}
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
					var cam = root.Attributes[0] as CameraAttribute;
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

		private Submesh3D ImportSubmesh(MeshAttribute meshAttribute, FbxImporter.Node node)
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
			/*
			* According to the fbx-documentation the camera's forward vector
			* points along a node's positive X axis.
			* so we have to rotate it by 90 around the Y-axis to correct it.
			*/
			Matrix44 rotationMatrix;
			Vector3 translation;
			Vector3 scale;
			origin.Decompose(out scale, out rotationMatrix, out translation);
			var newRotation = rotationMatrix.Rotation * Quaternion.CreateFromEulerAngles(Vector3.UnitY * -Mathf.Pi / 2);
			return Matrix44.CreateRotation(newRotation) * Matrix44.CreateScale(scale) * Matrix44.CreateTranslation(translation);
		}

		private void ImportAnimations(Scene scene)
		{
			foreach (var animation in scene.Animations.Animations) {
				var n = Model.TryFind<Node3D>(animation.Key);
				var scaleKeys = new List<Keyframe<Vector3>>();
				var rotationKeys = new List<Keyframe<Quaternion>>();
				var translationKeys = new List<Keyframe<Vector3>>();
				for (int i = 0; i < animation.TimeSteps.Length; i++) {
					var time = AnimationUtils.SecondsToFrames(animation.TimeSteps[i]);
					Vector3 finalScale;
					Quaternion finalRotation;
					Vector3 finalTranslation;
					if (n is Camera3D) {
						animation.Transform[i] = CorrectCameraTransform(animation.Transform[i]);
					}

					animation.Transform[i].Decompose(out finalScale, out finalRotation, out finalTranslation);
					scaleKeys.Add(new Keyframe<Vector3>(time, finalScale));
					rotationKeys.Add(new Keyframe<Quaternion>(time, finalRotation));
					translationKeys.Add(new Keyframe<Vector3>(time, finalTranslation));
				}

				(n.Animators["Scale", animation.MarkerId] as Animator<Vector3>).Keys.AddRange(
					Vector3KeyReducer.Default.Reduce(scaleKeys));

				(n.Animators["Rotation", animation.MarkerId] as Animator<Quaternion>).Keys.AddRange(
					QuaternionKeyReducer.Default.Reduce(rotationKeys));

				(n.Animators["Position", animation.MarkerId] as Animator<Vector3>).Keys.AddRange(
					Vector3KeyReducer.Default.Reduce(translationKeys));
				if (!Model.Animations.Any(a => a.Id == animation.MarkerId)) {
					Model.Animations.Add(new Lime.Animation {
						Id = animation.MarkerId,
					});
				}
			}
		}
	}
}
