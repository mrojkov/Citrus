using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		private class Pivot
		{
			public static readonly Pivot Default = new Pivot();

			public Matrix44? Translation;
			public Matrix44? RotationOffset;
			public Matrix44? RotationPivot;
			public Matrix44? PreRotation;
			public Matrix44? Rotation;
			public Matrix44? PostRotation;
			public Matrix44? RotationPivotInverse;
			public Matrix44? ScalingOffset;
			public Matrix44? ScalingPivot;
			public Matrix44? Scaling;
			public Matrix44? ScalingPivotInverse;
			public Matrix44? GeometricTranslation;
			public Matrix44? GeometricRotation;
			public Matrix44? GeometricScaling;

			public Matrix44 GetTransform(Vector3? scale, Quaternion? rotation, Vector3? translation)
			{
				var transform = Matrix44.Identity;
				if (GeometricScaling.HasValue) {
					transform *= GeometricScaling.Value;
				}
				if (GeometricRotation.HasValue) {
					transform *= GeometricRotation.Value;
				}
				if (GeometricTranslation.HasValue) {
					transform *= GeometricTranslation.Value;
				}
				if (ScalingPivotInverse.HasValue) {
					transform *= ScalingPivotInverse.Value;
				}
				if (scale.HasValue) {
					transform *= Matrix44.CreateScale(scale.Value);
				} else if (Scaling.HasValue) {
					transform *= Scaling.Value;
				}
				if (ScalingPivot.HasValue) {
					transform *= ScalingPivot.Value;
				}
				if (ScalingOffset.HasValue) {
					transform *= ScalingOffset.Value;
				}
				if (RotationPivotInverse.HasValue) {
					transform *= RotationPivotInverse.Value;
				}
				if (PostRotation.HasValue) {
					transform *= PostRotation.Value;
				}
				if (rotation.HasValue) {
					transform *= Matrix44.CreateRotation(rotation.Value);
				} else if (Rotation.HasValue) {
					transform *= Rotation.Value;
				}
				if (PreRotation.HasValue) {
					transform *= PreRotation.Value;
				}
				if (RotationPivot.HasValue) {
					transform *= RotationPivot.Value;
				}
				if (RotationOffset.HasValue) {
					transform *= RotationOffset.Value;
				}
				if (translation.HasValue) {
					transform *= Matrix44.CreateTranslation(translation.Value);
				} else if (Translation.HasValue) {
					transform *= Translation.Value;
				}
				return transform;
			}
		}

		private class MaterialImportData
		{
			public Dictionary<string, TextureReference> Textures { get; private set; }

			public Material CreateMaterial()
			{
				return new Material {
					DiffuseTexture = TryGetSerializableTexture("Diffuse"),
					OpacityTexture = TryGetSerializableTexture("Opacity"),
					SpecularTexture = TryGetSerializableTexture("Specular"),
					HeightTexture = TryGetSerializableTexture("Bump")
				};
			}

			private SerializableTexture TryGetSerializableTexture(string name)
			{
				TextureReference reference;
				return Textures.TryGetValue(name, out reference) ? reference.ToSerializableTexture() : null;
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
		private Dictionary<string, Pivot> pivots = new Dictionary<string, Pivot>();

		private KeyframeReducer keyframeReducer =
			new KeyframeReducer(new CommonInterpolationDetectorProvider(1e-8f));

		public Node3D RootNode { get; private set; }

		public ModelImporter(string path, TargetPlatform platform)
		{
			this.path = path;
			this.platform = platform;
			using (var aiContext = new Assimp.AssimpContext()) {
				aiContext.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesConfig(true));
				var postProcess = Assimp.PostProcessSteps.Triangulate;
				postProcess |= Assimp.PostProcessSteps.FlipUVs;
				if (platform == TargetPlatform.Unity) {
					postProcess |= Assimp.PostProcessSteps.FlipWindingOrder;
				}
				postProcess |= Assimp.PostProcessSteps.LimitBoneWeights;
				aiScene = aiContext.ImportFile(path, postProcess);
				FindCameras();
				ImportNodes();
				ImportSkeleton();
				ImportAnimations();
			}
		}

		private void ImportSkeleton()
		{
			ImportSkeleton(aiScene.RootNode);
		}

		private void ImportSkeleton(Assimp.Node aiNode)
		{
			if (aiNode.HasMeshes) {
				var mesh = RootNode.Find<Mesh3D>(aiNode.Name);
				for (var i = 0; i < aiNode.MeshCount; i++) {
					var aiMesh = aiScene.Meshes[aiNode.MeshIndices[i]];
					if (aiMesh.HasBones) {
						foreach (var bone in aiMesh.Bones) {
							mesh.Submeshes[i].BoneIndices.Add(mesh.Bones.Count);
							mesh.Bones.Add(RootNode.Find<Node3D>(bone.Name));
							mesh.BoneBindPoseInverses.Add(bone.OffsetMatrix.ToLime());
						}
					}
				}
			}
			foreach (var aiChild in aiNode.Children) {
				ImportSkeleton(aiChild);
			}
		}

		private void FindCameras()
		{
			aiCameras = aiScene.Cameras.ToDictionary(camera => camera.Name);
		}

		private void ImportNodes()
		{
			RootNode = ImportNodes(aiScene.RootNode, null, null);
		}

		private Node3D ImportNodes(Assimp.Node aiNode, Assimp.Node aiParent, Node parent)
		{
			Node3D node = null;
			if (aiNode.HasMeshes) {
				var mesh = new Mesh3D { Id = aiNode.Name };
				mesh.SetLocalTransform(CalcRelativeTransform(aiNode, aiParent));
				foreach (var aiMeshIndex in aiNode.MeshIndices) {
					var aiMesh = aiScene.Meshes[aiMeshIndex];
					if (aiMesh.HasVertices) {
						mesh.Submeshes.Add(ImportSubmesh(aiMesh));
					}
				}
				mesh.BoundingSphere = BoundingSphere.CreateFromPoints(mesh.Submeshes.SelectMany(submesh => submesh.Geometry.Vertices));
				node = mesh;
			} else if (aiNode.Name.Contains("_$AssimpFbx$")) {
				var ownerNodeName = GetNodeName(aiNode.Name);
				Pivot pivot;
				if (!pivots.TryGetValue(ownerNodeName, out pivot)) {
					pivot = new Pivot();
					pivots.Add(ownerNodeName, pivot);
				}
				Matrix44 transform = aiNode.Transform.ToLime();
				if (aiNode.Name.EndsWith("_Translation")) {
					pivot.Translation = transform;
				} else if (aiNode.Name.EndsWith("_RotationOffset")) {
					pivot.RotationOffset = transform;
				} else if (aiNode.Name.EndsWith("_RotationPivot")) {
					pivot.RotationPivot = transform;
				} else if (aiNode.Name.EndsWith("_PreRotation")) {
					pivot.PreRotation = transform;
				} else if (aiNode.Name.EndsWith("_Rotation")) {
					pivot.Rotation = transform;
				} else if (aiNode.Name.EndsWith("_PostRotation")) {
					pivot.PostRotation = transform;
				} else if (aiNode.Name.EndsWith("_RotationPivotInverse")) {
					pivot.RotationPivotInverse = transform;
				} else if (aiNode.Name.EndsWith("_ScalingOffset")) {
					pivot.ScalingOffset = transform;
				} else if (aiNode.Name.EndsWith("_ScalingPivot")) {
					pivot.ScalingPivot = transform;
				} else if (aiNode.Name.EndsWith("_Scaling")) {
					pivot.Scaling = transform;
				} else if (aiNode.Name.EndsWith("_ScalingPivotInverse")) {
					pivot.ScalingPivotInverse = transform;
				} else if (aiNode.Name.EndsWith("_GeometricTranslation")) {
					pivot.GeometricTranslation = transform;
				} else if (aiNode.Name.EndsWith("_GeometricRotation")) {
					pivot.GeometricRotation = transform;
				} else if (aiNode.Name.EndsWith("_GeometricScaling")) {
					pivot.GeometricScaling = transform;
				} else {
					throw new Lime.Exception("Unknown $AssimpFbx$ node: \"{0}\"", aiNode.Name);
				}
			} else {
				Assimp.Camera camera;
				if (aiCameras.TryGetValue(aiNode.Name, out camera)) {
					node = ImportCamera(camera, aiNode, aiParent);
				} else {
					node = new Node3D { Id = aiNode.Name };
					node.SetLocalTransform(CalcRelativeTransform(aiNode, aiParent));
				}
			}
			if (node != null) {
				if (parent != null) {
					parent.Nodes.Add(node);
				}
				aiParent = aiNode;
				parent = node;
			}
			foreach (var aiChild in aiNode.Children) {
				ImportNodes(aiChild, aiParent, parent);
			}
			return node;
		}

		private Node3D ImportCamera(Assimp.Camera aiCamera, Assimp.Node aiNode, Assimp.Node aiParent)
		{
			var camera = new Camera3D {
				Id = aiCamera.Name,
				FieldOfView = aiCamera.FieldOfview,
				AspectRatio = aiCamera.AspectRatio,
				NearClipPlane = aiCamera.ClipPlaneNear,
				FarClipPlane = aiCamera.ClipPlaneFar,
			};
			camera.SetLocalTransform(CalcRelativeTransform(aiNode, aiParent));
			return camera;
		}

		private Submesh3D ImportSubmesh(Assimp.Mesh mesh)
		{
			return new Submesh3D {
				// TODO: Materials
				Material = ImportMaterial(aiScene.Materials[mesh.MaterialIndex]),
				GeometryReference = new GeometryBufferReference(ImportGeometry(mesh))
			};
		}

		private GeometryBuffer ImportGeometry(Assimp.Mesh mesh)
		{
			var res = new GeometryBuffer();
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
			if (mesh.HasBones) {
				var indices = new byte[4];
				var weights = new float[4];
				res.BlendIndices = new BlendIndices[res.Vertices.Length];
				res.BlendWeights = new BlendWeights[res.Vertices.Length];
				for (var i = 0; i < res.Vertices.Length; i++) {
					var count = 0;
					for (var j = 0; j < mesh.BoneCount; j++) {
						var b = mesh.Bones[j];
						for (var k = 0; k < b.VertexWeightCount; k++) {
							var w = b.VertexWeights[k];
							if (w.VertexID != i) {
								continue;
							}
							indices[count] = (byte)j;
							weights[count] = w.Weight;
							count++;
						}
					}
					if (count == 0) {
						Console.WriteLine("Warning");
					} else {
						if (count > 0) {
							res.BlendIndices[i].Index0 = indices[0];
							res.BlendWeights[i].Weight0 = weights[0];
						}
						if (count > 1) {
							res.BlendIndices[i].Index1 = indices[1];
							res.BlendWeights[i].Weight1 = weights[1];
						}
						if (count > 2) {
							res.BlendIndices[i].Index2 = indices[2];
							res.BlendWeights[i].Weight2 = weights[2];
						}
						if (count > 3) {
							res.BlendIndices[i].Index3 = indices[3];
							res.BlendWeights[i].Weight3 = weights[3];
						}
					}
				}
			}
			return res;
		}

		private Material ImportMaterial(Assimp.Material material)
		{
			var res = new Material();
			res.Name = material.Name;
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
				Path.GetFileNameWithoutExtension(Toolbox.ToUnixSlashes(texture.FilePath))));
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
			var animationId = GetAnimationName(aiAnimation.Name);
			var channelGroups = aiAnimation
				.NodeAnimationChannels
				.GroupBy(channel => GetNodeName(channel.NodeName));
			foreach (var channelGroup in channelGroups) {
				var n = RootNode.TryFind<Node3D>(channelGroup.Key);
				var aiScaleKeys = new List<Assimp.VectorKey>();
				var aiRotationKeys = new List<Assimp.QuaternionKey>();
				var aiTranslationKeys = new List<Assimp.VectorKey>();
				Pivot pivot;
				if (!pivots.TryGetValue(n.Id, out pivot)) {
					pivot = Pivot.Default;
				}
				foreach (var aiChannel in channelGroup) {
					if (aiChannel.NodeName.EndsWith("_$AssimpFbx$_Scaling")) {
						aiScaleKeys = aiChannel.ScalingKeys;
					} else if (aiChannel.NodeName.EndsWith("_$AssimpFbx$_Rotation")) {
						aiRotationKeys = aiChannel.RotationKeys;
					} else if (aiChannel.NodeName.EndsWith("_$AssimpFbx$_Translation")) {
						aiTranslationKeys = aiChannel.PositionKeys;
					} else {
						aiScaleKeys = aiChannel.ScalingKeys;
						aiRotationKeys = aiChannel.RotationKeys;
						aiTranslationKeys = aiChannel.PositionKeys;
					}
				}
				var times = aiScaleKeys.Select(k => k.Time)
					.Union(aiRotationKeys.Select(k => k.Time))
					.Union(aiTranslationKeys.Select(k => k.Time))
					.OrderBy(t => t)
					.ToList();
				int prevScaleIndex = -1;
				int prevRotationIndex = -1;
				int prevTranslationIndex = -1;
				double prevScaleTime = 0.0;
				double prevRotationTime = 0.0;
				double prevTranslationTime = 0.0;
				Vector3? prevScale = null;
				Quaternion? prevRotation = null;
				Vector3? prevTranslation = null;
				var scaleKeys = new List<Keyframe<Vector3>>();
				var rotationKeys = new List<Keyframe<Quaternion>>();
				var translationKeys = new List<Keyframe<Vector3>>();
				foreach (var time in times) {
					Vector3? scale;
					int scaleIndex = aiScaleKeys.FindIndex(k => k.Time == time);
					if (scaleIndex != -1) {
						scale = aiScaleKeys[scaleIndex].Value.ToLime();
						prevScaleIndex = scaleIndex;
						prevScaleTime = time;
						prevScale = scale;
					} else if (prevScaleIndex != -1 && prevScaleIndex + 1 < aiScaleKeys.Count) {
						var nextScaleKey = aiScaleKeys[prevScaleIndex + 1];
						var nextScaleTime = nextScaleKey.Time;
						var nextScale = nextScaleKey.Value.ToLime();
						var amount = (float)((time - prevScaleTime) / (nextScaleTime - prevScaleTime));
						scale = Vector3.Lerp(amount, prevScale.Value, nextScale);
					} else {
						scale = prevScale;
					}
					Quaternion? rotation;
					int rotationIndex = aiRotationKeys.FindIndex(k => k.Time == time);
					if (rotationIndex != -1) {
						rotation = aiRotationKeys[rotationIndex].Value.ToLime();
						prevRotationIndex = rotationIndex;
						prevRotationTime = time;
						prevRotation = rotation;
					} else if (prevRotationIndex != -1 && prevRotationIndex + 1 < aiRotationKeys.Count) {
						var nextRotationKey = aiRotationKeys[prevRotationIndex + 1];
						var nextRotationTime = nextRotationKey.Time;
						var nextRotation = nextRotationKey.Value.ToLime();
						var amount = (float)((time - prevRotationTime) / (nextRotationTime - prevRotationTime));
						rotation = Quaternion.Slerp(prevRotation.Value, nextRotation, amount);
					} else {
						rotation = prevRotation;
					}
					Vector3? translation;
					int translationIndex = aiTranslationKeys.FindIndex(k => k.Time == time);
					if (translationIndex != -1) {
						translation = aiTranslationKeys[translationIndex].Value.ToLime();
						prevTranslationIndex = translationIndex;
						prevTranslationTime = time;
						prevTranslation = translation;
					} else if (prevTranslationIndex != -1 && prevTranslationIndex + 1 < aiTranslationKeys.Count) {
						var nextTranslationKey = aiTranslationKeys[prevTranslationIndex + 1];
						var nextTranslationTime = nextTranslationKey.Time;
						var nextTranslation = nextTranslationKey.Value.ToLime();
						var amount = (float)((time - prevTranslationTime) / (nextTranslationTime - prevTranslationTime));
						translation = Vector3.Lerp(amount, prevTranslation.Value, nextTranslation);
					} else {
						translation = prevTranslation;
					}
					var transform = pivot.GetTransform(scale, rotation, translation);
					Vector3 finalScale;
					Quaternion finalRotation;
					Vector3 finalTranslation;
					transform.Decompose(out finalScale, out finalRotation, out finalTranslation);
					scaleKeys.Add(new Keyframe<Vector3>(TimeToFrame(time, aiAnimation.TicksPerSecond), finalScale));
					rotationKeys.Add(new Keyframe<Quaternion>(TimeToFrame(time, aiAnimation.TicksPerSecond), finalRotation));
					translationKeys.Add(new Keyframe<Vector3>(TimeToFrame(time, aiAnimation.TicksPerSecond), finalTranslation));
				}
				(n.Animators["Scale", animationId] as Animator<Vector3>).Keys.AddRange(
					keyframeReducer.Reduce(scaleKeys));
				(n.Animators["Rotation", animationId] as Animator<Quaternion>).Keys.AddRange(
					keyframeReducer.Reduce(rotationKeys));
				(n.Animators["Position", animationId] as Animator<Vector3>).Keys.AddRange(
					keyframeReducer.Reduce(translationKeys));
			}
			RootNode.Animations.Add(new Animation {
				Id = animationId
			});
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

		private static int TimeToFrame(double ticks, double ticksPerSecond)
		{
			return AnimationUtils.MsecsToFrames((int)(ticks * 1000 / ticksPerSecond + 0.5));
		}

		private static string GetNodeName(string name)
		{
			int index = name.IndexOf("_$AssimpFbx$", StringComparison.Ordinal);
			return index >= 0 ? name.Remove(index) : name;
		}

		private static string GetAnimationName(string name)
		{
			return name.Replace("AnimStack::", "");
		}

		private static Matrix44 CalcRelativeTransform(Assimp.Node node, Assimp.Node ancestor)
		{
			Assimp.Matrix4x4 transform = node.Transform;
			Assimp.Node parent = node.Parent;
			while (parent != null && parent != ancestor) {
				transform *= parent.Transform;
				parent = parent.Parent;
			}
			return transform.ToLime();
		}
	}
}