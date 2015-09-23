using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lime;

namespace Orange
{
	static class AssimpExtensions
	{
		public static Color4 ToLime(this Assimp.Color4D value)
		{
			return Color4.FromFloats(value.R, value.G, value.B, value.A);
		}

		public static Vector3 ToLime(this Assimp.Vector3D value)
		{
			return new Vector3(value.X, value.Y, value.Z);
		}

		public static Quaternion ToLime(this Assimp.Quaternion value)
		{
			return new Quaternion(value.X, value.Y, value.Z, value.W);
		}

		public static Matrix44 ToLime(this Assimp.Matrix4x4 value)
		{
			return new Matrix44(
				value.A1, value.B1, value.C1, value.D1,
				value.A2, value.B2, value.C2, value.D2,
				value.A3, value.B3, value.C3, value.D3,
				value.A4, value.B4, value.C4, value.D4
			);
		}
	}

	public class ModelImporter
	{
		private class MaterialImportData
		{
			public Dictionary<string, TextureReference> Textures { get; private set; }

			public ModelMaterial CreateMaterial()
			{
				return new ModelMaterial {
					DiffuseTexture = TryGetSerializableTexture("Diffuse"),
					OpacityTexture = TryGetSerializableTexture("Opacity"),
					SpecularTexture = TryGetSerializableTexture("Specular"),
					BumpTexture = TryGetSerializableTexture("Bump")
				};
			}

			private SerializableTexture TryGetSerializableTexture(string name)
			{
				TextureReference reference;
				if (Textures.TryGetValue(name, out reference)) {
					return reference.ToSerializableTexture();
				}
				return null;
			}
		}

		private struct TextureReference
		{
			public string Path { get; set; }
			public int UVChannel { get; set; }

			public SerializableTexture ToSerializableTexture()
			{
				return new SerializableTexture(Path);
			}
		}

		private string path;
		private Assimp.Scene aiScene;
		private TargetPlatform platform;
		private Dictionary<string, Assimp.Camera> aiCameras = new Dictionary<string, Assimp.Camera>();

		public ModelNode RootNode { get; private set; }

		public ModelImporter(string path, TargetPlatform platform)
		{
			this.path = path;
			this.platform = platform;
			using (var aiContext = new Assimp.AssimpContext()) {
				aiContext.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesConfig(true));
				var postProcess = Assimp.PostProcessSteps.Triangulate;
				if (platform != TargetPlatform.Unity) {
					postProcess |= Assimp.PostProcessSteps.FlipUVs;
				}
				aiScene = aiContext.ImportFile(path, postProcess);
				FindCameras();
				RootNode = ImportNode(aiScene.RootNode);
				ImportAnimations();
			}
		}

		private void FindCameras()
		{
			aiCameras = aiScene.Cameras.ToDictionary(camera => camera.Name);
		}

		private ModelNode ImportNode(Assimp.Node aiNode)
		{
			ModelNode node = null;
			if (aiNode.HasMeshes) {
				var mesh = new ModelMesh { Id = aiNode.Name };
				mesh.SetLocalTransform(aiNode.Transform.ToLime());
				foreach (var aiMeshIndex in aiNode.MeshIndices) {
					var aiMesh = aiScene.Meshes[aiMeshIndex];
					if (aiMesh.HasVertices) {
						mesh.Submeshes.Add(ImportSubmesh(aiMesh));
					}
				}
				mesh.BoundingSphere = BoundingSphere.CreateFromPoints(mesh.Submeshes.SelectMany(submesh => submesh.Geometry.Vertices));
				node = mesh;
			} else {
				Assimp.Camera camera;
				if (aiCameras.TryGetValue(aiNode.Name, out camera)) {
					node = ImportCamera(camera, aiNode);
				} else {
					node = new ModelNode { Id = aiNode.Name };
					node.SetLocalTransform(aiNode.Transform.ToLime());
				}
			}
			foreach (var aiChild in aiNode.Children) {
				node.Nodes.Add(ImportNode(aiChild));
			}
			return node;
		}

		private ModelNode ImportCamera(Assimp.Camera aiCamera, Assimp.Node aiNode)
		{
			var camera = new ModelCamera {
				Id = aiCamera.Name,
				FieldOfView = aiCamera.FieldOfview,
				AspectRatio = aiCamera.AspectRatio,
				NearClipPlane = aiCamera.ClipPlaneNear,
				FarClipPlane = aiCamera.ClipPlaneFar,
			};
			camera.SetLocalTransform(aiNode.Transform.ToLime());
			return camera;
		}

		private ModelSubmesh ImportSubmesh(Assimp.Mesh mesh)
		{
			return new ModelSubmesh {
				// TODO: Materials
				Material = ImportMaterial(aiScene.Materials[mesh.MaterialIndex]),
				Geometry = ImportGeometry(mesh)
			};
		}

		private Mesh ImportGeometry(Assimp.Mesh mesh)
		{
			var res = new Mesh();
			res.Vertices = mesh.Vertices.Select(AssimpExtensions.ToLime).ToArray();
			res.Indices = mesh.GetIndices().Select(index => checked((ushort)index)).ToArray();
			if (mesh.HasTextureCoords(0)) {
				res.UV1 = mesh.TextureCoordinateChannels[0].Select(uv => new Vector2(uv.X, uv.Y)).ToArray();
			}
			if (mesh.HasVertexColors(0)) {
				res.Colors = mesh.VertexColorChannels[0].Select(AssimpExtensions.ToLime).ToArray();
			} else {
				res.Colors = Enumerable.Repeat(Color4.White, mesh.VertexCount).ToArray();
			}
			return res;
		}

		private ModelMaterial ImportMaterial(Assimp.Material material)
		{
			var res = new ModelMaterial();
			if (material.HasTextureDiffuse) {
				res.DiffuseTexture = CreateSerializableTexture(material.TextureDiffuse);
			}
			if (material.HasColorDiffuse) {
				res.DiffuseColor = material.ColorDiffuse.ToLime();
			}
			if (material.HasColorEmissive) {
				res.EmissiveColor = material.ColorEmissive.ToLime();
			}
			if (material.HasColorSpecular) {
				res.SpecularColor = material.ColorSpecular.ToLime();
			}
			return res;
		}

		private SerializableTexture CreateSerializableTexture(Assimp.TextureSlot texture)
		{
			var texturePath = Toolbox.ToUnixSlashes(Path.Combine(Path.GetDirectoryName(path), 
				Path.GetFileNameWithoutExtension(texture.FilePath)));
			return new SerializableTexture(texturePath);
		}

		private void ImportAnimations()
		{
			foreach (var aiAnimation in aiScene.Animations) {
				ImportAnimation(aiAnimation);
			}
		}

		private void ImportAnimation(Assimp.Animation aiAnimation)
		{
			var animation = new Animation {
				Id = aiAnimation.Name
			};
			foreach (var channel in aiAnimation.NodeAnimationChannels) {
				var node = RootNode.Find<ModelNode>(channel.NodeName);
				var scaleAnimator = node.Animators["Scale", animation.Id] as Animator<Vector3>;
				var rotationAnimator = node.Animators["Rotation", animation.Id] as Animator<Quaternion>;
				var positionAnimator = node.Animators["Position", animation.Id] as Animator<Vector3>;
				AddDistinctKeys(
					scaleAnimator,
					channel.ScalingKeys.Select(key => new Keyframe<Vector3>(TimeToFrame(key.Time), key.Value.ToLime()))
				);
				AddDistinctKeys(
					rotationAnimator,
					channel.RotationKeys.Select(key => new Keyframe<Quaternion>(TimeToFrame(key.Time), key.Value.ToLime()))
				);
				AddDistinctKeys(
					positionAnimator,
					channel.PositionKeys.Select(key => new Keyframe<Vector3>(TimeToFrame(key.Time), key.Value.ToLime()))
				);
			}
			RootNode.Animations.Add(animation);
		}

		//TODO: Move to KeyframeCollection
		private void AddDistinctKeys<T>(Animator<T> animator, IEnumerable<Keyframe<T>> keys)
		{
			foreach (var k in keys) {
				var i = animator.Keys.Count;
				if (i > 0 && k.Frame == animator.Keys[i - 1].Frame) {
					animator.Keys[i - 1] = k;
				} else {
					animator.Keys.Add(k);
				}
			}
		}

		private static int TimeToFrame(double time)
		{
			return AnimationUtils.MsecsToFrames((int)(time * 1000 + 0.5f));
		}
	}
}
