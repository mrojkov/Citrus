using System;
using Lime;
using Orange.FbxImporter;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orange.FbxImporter
{
	public class FbxImportOptions
	{
		public string Path;
		public Target Target;
		public Dictionary<string, CookingRules> CookingRulesMap = new Dictionary<string, CookingRules>();
		public bool ApplyAttachment = true;
	}

	public partial class FbxModelImporter : IDisposable
	{
		/*
		* According to the fbx-documentation the camera's forward vector
		* points along a node's positive X axis.
		* so we have to rotate it by 90 around the Y-axis to correct it.
		*/
		private static readonly Quaternion CameraPostRotation = Quaternion.CreateFromEulerAngles(Vector3.UnitY * -Mathf.Pi / 2);
		private readonly FbxManager manager;
		private readonly FbxImportOptions options;

		public FbxModelImporter(FbxImportOptions options)
		{
			this.options = options;
			manager = FbxManager.Create();
		}

		public Model3D LoadModel()
		{
			var scene = LoadRaw();
			var model = new Model3D();
			model.Nodes.Add(ImportNodes(scene.Root));
			ImportAnimations(model, scene);
			if (options.ApplyAttachment && Model3DAttachmentParser.IsAttachmentExists(options.Path)) {
				Model3DAttachmentParser.GetModel3DAttachment(options.Path).Apply(model);
			}
			return model;
		}

		public FbxScene LoadRaw()
		{
			return manager.LoadScene(options);
		}

		private Lime.Node ImportNodes(FbxNode root, Node parent = null)
		{
			Node3D node = null;
			if (root == null)
				return null;
			switch (root.Attribute.Type) {
				case FbxNodeAttribute.FbxNodeType.Mesh:
					var meshAttribute = root.Attribute as FbxMeshAttribute;
					var mesh = new Mesh3D {
						Id = root.Name,
						SkinningMode = meshAttribute.SkinningMode
					};
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

		private Submesh3D ImportSubmesh(FbxSubmesh meshAttribute, FbxNode node)
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
						ShaderPrograms.Attributes.Normal,
						ShaderPrograms.Attributes.Tangent
					}
				},
				Material = meshAttribute.MaterialIndex != -1 && node.Materials != null
					? GetOrCreateLimeMaterial(node.Materials[meshAttribute.MaterialIndex])
					: FbxMaterial.Default
			};
			MeshUtils.RemoveDuplicates(sm.Mesh);
			if (meshAttribute.Bones.Length > 0) {
				foreach (var bone in meshAttribute.Bones) {
					sm.BoneNames.Add(bone.Name);
					sm.BoneBindPoses.Add(bone.Offset);
				}
			}
			return sm;
		}

		private Dictionary<FbxMaterialDescriptor, CommonMaterial> MaterialPool = new Dictionary<FbxMaterialDescriptor, CommonMaterial>();

		public CommonMaterial GetOrCreateLimeMaterial(FbxMaterial material)
		{
			if (MaterialPool.ContainsKey(material.MaterialDescriptor)) {
				return MaterialPool[material.MaterialDescriptor];
			}
			var commonMaterial = new CommonMaterial {
				Id = material.MaterialDescriptor.Name
			};
			if (!string.IsNullOrEmpty(material.MaterialDescriptor.Path)) {
				var tex = CreateSerializableTexture(options.Path, material.MaterialDescriptor.Path);
				commonMaterial.DiffuseTexture = tex;
				var rulesPath = tex.SerializationPath + ".png";

				// TODO: implement U and V wrapping modes separately for cooking rules.
				// Set "Repeat" wrpap mode if wrap mode of any of the components is set as "Repeat".
				var mode = material.MaterialDescriptor.WrapModeU == TextureWrapMode.Repeat || material.MaterialDescriptor.WrapModeV == TextureWrapMode.Repeat ?
						TextureWrapMode.Repeat : TextureWrapMode.Clamp;
				if (options.CookingRulesMap.ContainsKey(rulesPath)) {
					var cookingRules = options.CookingRulesMap[rulesPath] = options.CookingRulesMap[rulesPath].InheritClone();
					if (cookingRules.CommonRules.WrapMode != mode) {
						cookingRules.CommonRules.WrapMode = mode;
						cookingRules.SourceFilename = rulesPath + ".txt";
						cookingRules.CommonRules.Override(nameof(ParticularCookingRules.WrapMode));
						cookingRules.DeduceEffectiveRules(options.Target);
						cookingRules.Save();
					}
				}
			}
			commonMaterial.DiffuseColor = material.MaterialDescriptor.DiffuseColor;
			MaterialPool[material.MaterialDescriptor] = commonMaterial;
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

		private void ImportAnimations(Model3D model, FbxScene scene)
		{
			if (scene.Animations == null) {
				return;
			}
			foreach (var animation in scene.Animations.List) {
				var n = model.TryFind<Node3D>(animation.TargetNodeId);
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
				if (!model.Animations.Any(a => a.Id == animation.AnimationStackName)) {
					model.Animations.Add(new Animation {
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

		public void Dispose()
		{
			manager.Destroy();
		}
	}
}
