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
		private FbxManager manager;
		private Target target;
		private readonly Dictionary<string, CookingRules> cookingRulesMap;

		public Model3D Model { get; private set; }

		public FbxModelImporter(string path, Target target, Dictionary<string, CookingRules> cookingRulesMap, bool applyAttachment = true)
		{
			this.target = target;
			this.path = path;
			this.cookingRulesMap = cookingRulesMap;
			manager = FbxManager.Create();
			var scene = manager.LoadScene(path);
			Model = new Model3D();
			Model.Nodes.Add(ImportNodes(scene.Root));
			ImportAnimations(scene);
			if (applyAttachment && Model3DAttachmentParser.IsAttachmentExists(path)) {
				Model3DAttachmentParser.GetModel3DAttachment(path).Apply(Model);
			}
			manager.Destroy();
		}

		private Lime.Node ImportNodes(FbxImporter.FbxNode root, Lime.Node parent = null)
		{
			Node3D node = null;
			if (root == null)
				return null;
			switch (root.Attribute.Type) {
				case FbxNodeAttribute.FbxNodeType.Mesh:
					var meshAttribute = root.Attribute as FbxMeshAttribute;
					var mesh = new Mesh3D { Id = root.Name };
					foreach (var submesh in meshAttribute.Submeshes) {
						mesh.Submeshes.Add(ImportSubmesh(submesh, root));
					}
					node = mesh;
					if (mesh.Submeshes.Count != 0) {
						mesh.SetLocalTransform(root.LocalTranform);
						mesh.RecalcBounds();
						mesh.RecalcCenter();
					}
					break;
				case FbxNodeAttribute.FbxNodeType.Camera:
					var cam = root.Attribute as FbxCameraAttribute;
					node = new Camera3D {
						Id = root.Name,
						FieldOfView = cam.FieldOfView * Mathf.DegToRad,
						AspectRatio = cam.AspectRatio,
						NearClipPlane = cam.NearClipPlane,
						FarClipPlane = cam.FarClipPlane,
						ProjectionMode = cam.ProjectionMode,
						OrthographicSize = cam.OrthoZoom,
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

		private Submesh3D ImportSubmesh(FbxSubmesh meshAttribute, FbxImporter.FbxNode node)
		{
			var sm = new Submesh3D {
				Mesh = new Mesh<Mesh3D.Vertex> {
					Vertices = meshAttribute.Vertices,
					Indices = meshAttribute.Indices.Select(index => checked((ushort) index)).ToArray(),
					AttributeLocations = new[] {
						ShaderPrograms.Attributes.Pos1,
						ShaderPrograms.Attributes.Color1,
						ShaderPrograms.Attributes.UV1,
						ShaderPrograms.Attributes.BlendIndices,
						ShaderPrograms.Attributes.BlendWeights,
						ShaderPrograms.Attributes.Normal
					}
				},
				Material = meshAttribute.MaterialIndex != -1
					? CreateLimeMaterial(node.Materials[meshAttribute.MaterialIndex], path, target)
					: FbxMaterial.Default
			};


			if (meshAttribute.Bones.Length > 0) {
				foreach (var bone in meshAttribute.Bones) {
					sm.BoneNames.Add(bone.Name);
					sm.BoneBindPoses.Add(bone.Offset);
				}
			}
			return sm;
		}

		public CommonMaterial CreateLimeMaterial(FbxMaterial material, string modelPath, Target target)
		{
			var commonMaterial = new CommonMaterial {
				Name = material.Name
			};
			if (!string.IsNullOrEmpty(material.Path)) {
				var tex = CreateSerializableTexture(modelPath, material.Path);
				commonMaterial.DiffuseTexture = tex;
				var rulesPath = tex.SerializationPath + ".png";

				// TODO: implement U and V wrapping modes separately for cooking rules.
				// Set "Repeat" wrpap mode if wrap mode of any of the components is set as "Repeat".
				var mode = material.WrapModeU == TextureWrapMode.Repeat || material.WrapModeV == TextureWrapMode.Repeat ?
						TextureWrapMode.Repeat : TextureWrapMode.Clamp;
				if (cookingRulesMap.ContainsKey(rulesPath)) {
					var cookingRules = cookingRulesMap[rulesPath] = cookingRulesMap[rulesPath].InheritClone();
					if (cookingRules.CommonRules.WrapMode != mode) {
						cookingRules.CommonRules.WrapMode = mode;
						cookingRules.SourceFilename = rulesPath + ".txt";
						cookingRules.CommonRules.Override(nameof(ParticularCookingRules.WrapMode));
						cookingRules.DeduceEffectiveRules(target);
						cookingRules.Save();
					}
				}
			}
			commonMaterial.DiffuseColor = material.DiffuseColor;
			return commonMaterial;
		}

		private SerializableTexture CreateSerializableTexture(string root, string texturePath)
		{
			return new SerializableTexture(Toolbox.ToUnixSlashes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(root),
				System.IO.Path.GetFileNameWithoutExtension(Toolbox.ToUnixSlashes(texturePath)))));
		}

		private Matrix44 CorrectCameraTransform(Matrix44 origin)
		{
			origin.Decompose(out var scale, out Matrix44 rotationMatrix, out var translation);
			var newRotation = rotationMatrix.Rotation * CameraPostRotation;
			return Matrix44.CreateRotation(newRotation) * Matrix44.CreateScale(scale) * Matrix44.CreateTranslation(translation);
		}

		private void CorrectCameraAnimationKeys(IEnumerable<Keyframe<Quaternion>> keys)
		{
			foreach (var key in keys) {
				key.Value *= CameraPostRotation;
			}
		}

		private void ImportAnimations(FbxScene scene)
		{
			foreach (var animation in scene.Animations.List) {
				var n = Model.TryFind<Node3D>(animation.TargetNodeId);
				var scaleKeys = Vector3KeyReducer.Default.Reduce(animation.ScaleKeys);
				if (scaleKeys.Count != 0) {
					GetOrAddAnimator<Vector3Animator>(animation, n, nameof(Node3D.Scale)).Keys.AddRange(scaleKeys);
				}
				var rotKeys = QuaternionKeyReducer.Default.Reduce(animation.RotationKeys);
				if (rotKeys.Count != 0) {
					if (n is Camera3D) {
						CorrectCameraAnimationKeys(rotKeys);
					}
					GetOrAddAnimator<QuaternionAnimator>(animation, n, nameof(Node3D.Rotation)).Keys.AddRange(rotKeys);
				}
				var posKeys = Vector3KeyReducer.Default.Reduce(animation.PositionKeys);
				if (posKeys.Count != 0) {
					GetOrAddAnimator<Vector3Animator>(animation, n, nameof(Node3D.Position)).Keys.AddRange(posKeys);
				}
				if (!Model.Animations.Any(a => a.Id == animation.AnimationStackName)) {
					Model.Animations.Add(new Animation {
						Id = animation.AnimationStackName,
					});
				}
			}
		}

		private static T GetOrAddAnimator<T>(AnimationData animation, Node3D n, string propName) where  T: IAnimator, new()
		{
			if (n.Animators.TryFind(propName, out var a)) return (T)a;
			var animator = new T {
				AnimationId = animation.AnimationStackName,
				TargetPropertyPath = propName
			};
			n.Animators.Add(animator);
			return animator;
		}
	}
}
