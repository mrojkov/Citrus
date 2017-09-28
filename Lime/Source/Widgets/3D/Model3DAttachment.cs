using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuzu;

namespace Lime
{
	public class Model3DAttachment
	{
		public const string DefaultAnimationName = "Default";

		public readonly List<MeshOption> MeshOptions = new List<MeshOption>();
		public readonly List<Animation> Animations = new List<Animation>();
		public readonly List<MaterialEffect> MaterialEffects = new List<MaterialEffect>();
		public float ScaleFactor;

		public class MeshOption
		{
			public string Id;
			public bool HitTestTarget;
			public CullMode? CullMode;
		}

		public class Animation
		{
			public string Name;
			public int StartFrame;
			public int LastFrame;
			public List<Marker> Markers = new List<Marker>();
			public List<string> Nodes = new List<string>();
			public List<string> IgnoredNodes = new List<string>();
			public BlendingOption Blending;
			public readonly Dictionary<string, MarkerBlending> MarkersBlendings = new Dictionary<string, MarkerBlending>();
		}

		public class MaterialEffect
		{
			public string Name;
			public string MaterialName;
			public string Path;
			public BlendingOption Blending;
		}

		public void Apply(Node3D model)
		{
			ProcessMeshOptions(model);
			ProcessAnimations(model);
			ProcessMaterialEffects(model);
		}

		public void ApplyScaleFactor(Node3D model)
		{
			if (ScaleFactor == 1) {
				return;
			}
			var sf = Vector3.One * ScaleFactor;
			var nodes = model.Descendants.OfType<Node3D>();
			Vector3 tranlation;
			Quaternion rotation;
			Vector3 scale;
			foreach (var node in nodes) {
				node.Position *= sf;
				if (node is Mesh3D) {
					foreach (var submesh in (node as Mesh3D).Submeshes) {
						foreach (VertexBuffer<Mesh3D.Vertex> vb in submesh.Mesh.VertexBuffers) {
							for (var i = 0; i < vb.Data.Length; i++) {
								vb.Data[i].Pos *= sf;
							}
						};
						for (int i = 0; i < submesh.BoneBindPoses.Count; i++) {
							submesh.BoneBindPoses[i].Decompose(out scale, out rotation, out tranlation);
							tranlation *= sf;
							submesh.BoneBindPoses[i] =
								Matrix44.CreateRotation(rotation) *
								Matrix44.CreateScale(scale) *
								Matrix44.CreateTranslation(tranlation);
						}
					}
				}
				foreach (var animator in node.Animators) {
					if (animator.TargetProperty == "Position") {
						foreach (Keyframe<Vector3> key in animator.Keys) {
							key.Value *= sf;
						}
					}
				}
			}
		}

		private void ProcessMeshOptions(Node3D model)
		{
			if (MeshOptions.Count == 0) {
				return;
			}

			var meshes = model.Descendants
				.OfType<Mesh3D>()
				.Where(d => !string.IsNullOrEmpty(d.Id));
			foreach (var mesh in meshes) {
				foreach (var meshOption in MeshOptions) {
					if (mesh.Id != meshOption.Id) {
						continue;
					}

					if (meshOption.HitTestTarget) {
						mesh.HitTestTarget = true;
						mesh.SkipRender = true;
					}
					if (meshOption.CullMode.HasValue) {
						mesh.CullMode = meshOption.CullMode.Value;
					}
					break;
				}
			}
		}

		private void ProcessAnimations(Node3D model)
		{
			if (Animations.Count == 0) {
				return;
			}
			var srcAnimation = model.DefaultAnimation;
			foreach (var animationData in Animations) {
				var animation = srcAnimation;
				if (animationData.Name != DefaultAnimationName) {
					animation = new Lime.Animation {
						Id = animationData.Name
					};
					foreach (var node in GetAnimationNodes(model, animationData)) {
						CopyAnimationKeys(node, srcAnimation, animation);
					}
					model.Animations.Add(animation);
				}

				foreach (var markerData in animationData.Markers) {
					animation.Markers.AddOrdered(markerData.Clone());
				}

				if (animationData.Blending != null || animationData.MarkersBlendings.Count > 0) {
					animation.AnimationEngine = BlendAnimationEngine.Instance;
					var blender = model.Components.GetOrAdd<AnimationBlender>();

					var animationBlending = new AnimationBlending() {
						Option = animationData.Blending
					};
					if (animationData.MarkersBlendings.Count > 0) {
						foreach (var markersBlendings in animationData.MarkersBlendings) {
							animationBlending.MarkersOptions.Add(markersBlendings.Key, markersBlendings.Value);
						}
					}
					blender.Options.Add(animation.Id ?? "", animationBlending);
				}
			}
		}

		private void ProcessMaterialEffects(Node3D model)
		{
			var effectEngine = new MaterialEffectEngine();
			foreach (var effect in MaterialEffects) {
				var effectPresenter = new MaterialEffectPresenter(effect.MaterialName, effect.Name, effect.Path);
				model.CompoundPresenter.Add(effectPresenter);

				if (effect.Blending != null) {
					effectPresenter.Animation.AnimationEngine = BlendAnimationEngine.Instance;
					var blender = effectPresenter.Scene.Components.GetOrAdd<AnimationBlender>();
					var animationBlending = new AnimationBlending() {
						Option = effect.Blending
					};
					blender.Options.Add(effectPresenter.Animation.Id ?? "", animationBlending);
				}

				var animation = new Lime.Animation {
					Id = effect.Name,
					AnimationEngine = effectEngine
				};

				foreach (var marker in effectPresenter.Animation.Markers) {
					animation.Markers.Add(marker.Clone());
				}

				model.Animations.Add(animation);
			}
		}

		private void CopyAnimationKeys(Node node, Lime.Animation srcAnimation, Lime.Animation dstAnimation)
		{
			var srcAnimators = new List<IAnimator>(node.Animators.Where(i => i.AnimationId == srcAnimation.Id));
			foreach (var srcAnimator in srcAnimators) {
				var dstAnimator = srcAnimator.Clone();
				dstAnimator.AnimationId = dstAnimation.Id;
				node.Animators.Add(dstAnimator);
			}
		}

		private IEnumerable<Node> GetAnimationNodes(Node3D model, Animation animationData)
		{
			if (animationData.Nodes.Count > 0) {
				return animationData.Nodes.Distinct().Select(model.FindNode);
			}
			if (animationData.IgnoredNodes.Count > 0) {
				var ignoredNodes = new HashSet<Node>(animationData.IgnoredNodes.Select(model.FindNode));
				return model.Descendants.Where(i => !ignoredNodes.Contains(i));
			}
			return model.Descendants;
		}
	}

	public class Model3DAttachmentParser
	{
		public enum UVAnimationType
		{
			Rotation,
			Offset
		}

		public enum UVAnimationOverlayBlending
		{
			Multiply,
			Overlay,
			Add
		}

		public class ModelAttachmentFormat
		{
			[YuzuOptional]
			public Dictionary<string, MeshOptionFormat> MeshOptions = null;

			[YuzuOptional]
			public Dictionary<string, ModelAnimationFormat> Animations = null;

			[YuzuOptional]
			public Dictionary<string, ModelMaterialEffectFormat> MaterialEffects = null;

			[YuzuOptional]
			public List<UVAnimationFormat> UVAnimations = null;

			[YuzuOptional]
			public float ScaleFactor = 1f;
		}

		public class MeshOptionFormat
		{
			[YuzuOptional]
			public bool HitTestTarget = false;

			[YuzuOptional]
			public string CullMode = null;
		}

		public class UVAnimationFormat
		{
			[YuzuOptional]
			public string MeshName = null;

			[YuzuOptional]
			public string DiffuseTexture = null;

			[YuzuOptional]
			public string OverlayTexture = null;

			[YuzuOptional]
			public string MaskTexture = null;

			[YuzuOptional]
			public float AnimationSpeed = 0;

			[YuzuOptional]
			public UVAnimationType AnimationType = UVAnimationType.Rotation;

			[YuzuOptional]
			public UVAnimationOverlayBlending BlendingMode = UVAnimationOverlayBlending.Multiply;

			[YuzuOptional]
			public bool AnimateOverlay = false;

			[YuzuOptional]
			public float TileX = 1f;

			[YuzuOptional]
			public float TileY = 1f;
		}

		public class ModelAnimationFormat
		{
			[YuzuOptional]
			public int StartFrame = 0;

			[YuzuOptional]
			public int LastFrame = 0;

			[YuzuOptional]
			public List<string> Nodes = null;

			[YuzuOptional]
			public List<string> IgnoredNodes = null;

			[YuzuOptional]
			public Dictionary<string, ModelMarkerFormat> Markers = null;

			[YuzuOptional]
			public int Blending = 0;
		}

		public class ModelMarkerFormat
		{
			[YuzuOptional]
			public int Frame = 0;

			[YuzuOptional]
			public string Action = null;

			[YuzuOptional]
			public string JumpTarget = null;

			[YuzuOptional]
			public readonly Dictionary<string, int> SourceMarkersBlending = null;

			[YuzuOptional]
			public int Blending = 0;
		}

		public class ModelMaterialEffectFormat
		{
			[YuzuOptional]
			public string MaterialName = null;

			[YuzuOptional]
			public string Path = null;

			[YuzuOptional]
			public int Blending = 0;
		}

		public Model3DAttachment Parse(string modelPath)
		{
			modelPath = AssetPath.CorrectSlashes(
				Path.Combine(Path.GetDirectoryName(modelPath) ?? "",
				Path.GetFileNameWithoutExtension(AssetPath.CorrectSlashes(modelPath) ?? "")
			));
			var attachmentPath = modelPath + ".Attachment.txt";
			if (!AssetBundle.Current.FileExists(attachmentPath)) {
				return null;
			}
			try {
				var modelAttachmentFormat = Serialization.ReadObject<ModelAttachmentFormat>(attachmentPath);
				var attachment = new Model3DAttachment {
					ScaleFactor = modelAttachmentFormat.ScaleFactor
				};

				if (modelAttachmentFormat.MeshOptions != null) {
					foreach (var meshOptionFormat in modelAttachmentFormat.MeshOptions) {
						var meshOption = new Model3DAttachment.MeshOption() {
							Id = meshOptionFormat.Key,
							HitTestTarget = meshOptionFormat.Value.HitTestTarget
						};
						if (!string.IsNullOrEmpty(meshOptionFormat.Value.CullMode)) {
							switch (meshOptionFormat.Value.CullMode) {
								case "None":
									meshOption.CullMode = CullMode.None;
									break;
								case "CullClockwise":
									meshOption.CullMode = CullMode.CullClockwise;
									break;
								case "CullCounterClockwise":
									meshOption.CullMode = CullMode.CullCounterClockwise;
									break;
							}
						}
						attachment.MeshOptions.Add(meshOption);
					}
				}

				if (modelAttachmentFormat.Animations != null) {
					foreach (var animationFormat in modelAttachmentFormat.Animations) {
						var animation = new Model3DAttachment.Animation {
							Name = animationFormat.Key,
							StartFrame = animationFormat.Value.StartFrame,
							LastFrame = animationFormat.Value.LastFrame
						};

						if (animationFormat.Value.Markers != null) {
							foreach (var markerFormat in animationFormat.Value.Markers) {
								var marker = new Marker {
									Id = markerFormat.Key,
									Frame = FixFrame(markerFormat.Value.Frame)
								};
								if (!string.IsNullOrEmpty(markerFormat.Value.Action)) {
									switch (markerFormat.Value.Action) {
										case "Start":
											marker.Action = MarkerAction.Play;
											break;
										case "Stop":
											marker.Action = MarkerAction.Stop;
											break;
										case "Jump":
											marker.Action = MarkerAction.Jump;
											marker.JumpTo = markerFormat.Value.JumpTarget;
											break;
									}
								}
								if (markerFormat.Value.Blending > 0) {
									var markerBlending = new MarkerBlending() {
										Option = new BlendingOption(markerFormat.Value.Blending)
									};
									animation.MarkersBlendings.Add(markerFormat.Key, markerBlending);
								}
								if (markerFormat.Value.SourceMarkersBlending != null) {
									MarkerBlending markerBlending;
									animation.MarkersBlendings.TryGetValue(markerFormat.Key, out markerBlending);
									if (markerBlending == null) {
										markerBlending = new MarkerBlending();
										animation.MarkersBlendings.Add(markerFormat.Key, markerBlending);
									}

									foreach (var sourceMarkerFormat in markerFormat.Value.SourceMarkersBlending) {
										markerBlending.SourceMarkersOptions.Add(
											sourceMarkerFormat.Key,
											new BlendingOption(sourceMarkerFormat.Value)
										);
									}
								}

								animation.Markers.Add(marker);
							}
						}

						if (animationFormat.Value.Blending > 0) {
							animation.Blending = new BlendingOption(animationFormat.Value.Blending);
						}

						if (animationFormat.Value.Nodes != null) {
							animation.Nodes = animationFormat.Value.Nodes;
						}

						if (animationFormat.Value.IgnoredNodes != null && animationFormat.Value.IgnoredNodes.Count > 0) {
							if (animation.Nodes.Count > 0) {
								throw new Exception("Conflict between 'Nodes' and 'IgnoredNodes' in animation '{0}", animation.Name);
							}
							animation.IgnoredNodes = animationFormat.Value.IgnoredNodes;
						}

						attachment.Animations.Add(animation);
					}
				}

				if (modelAttachmentFormat.MaterialEffects != null) {
					foreach (var materialEffectFormat in modelAttachmentFormat.MaterialEffects) {
						var materialEffect = new Model3DAttachment.MaterialEffect() {
							Name = materialEffectFormat.Key,
							MaterialName = materialEffectFormat.Value.MaterialName,
							Path = FixPath(modelPath, materialEffectFormat.Value.Path)
						};
						if (materialEffectFormat.Value.Blending > 0) {
							materialEffect.Blending = new BlendingOption(materialEffectFormat.Value.Blending);
						}
						attachment.MaterialEffects.Add(materialEffect);
					}
				}

				return attachment;
			} catch (System.Exception e) {
				throw new System.Exception(modelPath + ": " + e.Message, e);
			}
		}

		private static string FixPath(string modelPath, string path)
		{
			var baseDir = Path.GetDirectoryName(modelPath);
			return AssetPath.CorrectSlashes(Path.Combine(AssetPath.CorrectSlashes(baseDir), AssetPath.CorrectSlashes(path)));
		}

		private static int FixFrame(int frame, double fps = 30)
		{
			return AnimationUtils.SecondsToFrames(frame / fps);
		}
	}

	public class MaterialEffectEngine : DefaultAnimationEngine
	{
		public override void AdvanceAnimation(Animation animation, float delta)
		{
			var effectAnimation = GetPresenter(animation).Animation;
			effectAnimation.AnimationEngine.AdvanceAnimation(effectAnimation, delta);

			base.AdvanceAnimation(animation, delta);
		}

		public override void ApplyAnimators(Animation animation, bool invokeTriggers)
		{
			var effectAnimation = GetPresenter(animation).Animation;
			effectAnimation.AnimationEngine.ApplyAnimators(effectAnimation, invokeTriggers);

			base.ApplyAnimators(animation, invokeTriggers);
		}

		public override bool TryRunAnimation(Animation animation, string markerId)
		{
			var presenter = GetPresenter(animation);
			if (
				!base.TryRunAnimation(animation, markerId) ||
				!presenter.Animation.AnimationEngine.TryRunAnimation(presenter.Animation, markerId)
			) {
				return false;
			}

			presenter.WasSnapshotDeprecated = true;
			foreach (var material in GetMaterialsForAnimation(animation)) {
				material.DiffuseTexture = presenter.Snapshot;
			}
			return true;
		}

		private static MaterialEffectPresenter GetPresenter(Animation animation)
		{
			return animation.Owner.CompoundPresenter
				.OfType<MaterialEffectPresenter>()
				.First(p => p.EffectName == animation.Id);
		}

		public static IEnumerable<CommonMaterial> GetMaterialsForAnimation(Animation animation)
		{
			return GetMaterials(animation.Owner, GetPresenter(animation).MaterialName);
		}

		private static IEnumerable<CommonMaterial> GetMaterials(Node model, string name)
		{
			return model
				.Descendants
				.OfType<Mesh3D>()
				.SelectMany(i => i.Submeshes)
				.Select(i => i.Material)
				.Cast<CommonMaterial>()
				.Where(i => i.Name == name);
		}
	}

	public class MaterialEffectPresenter : CustomPresenter
	{
		private static readonly RenderChain renderChain = new RenderChain();
		private ITexture snapshot;

		public string MaterialName { get; }
		public string EffectName { get; }
		public Widget Scene { get; private set; }
		public bool WasSnapshotDeprecated { get; set; } = true;
		public Animation Animation => Scene.DefaultAnimation;
		public ITexture Snapshot => snapshot ?? (snapshot = new RenderTexture((int)Scene.Width, (int)Scene.Height));

		public MaterialEffectPresenter(string materialName, string effectName, string path)
		{
			MaterialName = materialName;
			EffectName = effectName;
			Scene = new Frame(path);
		}

		public override void Render(Node node)
		{
			if (!Animation.IsRunning && !WasSnapshotDeprecated) {
				return;
			}

			Scene.RenderToTexture(Snapshot, renderChain);
			renderChain.Clear();
			WasSnapshotDeprecated = false;
		}

		public override IPresenter Clone()
		{
			var clone = (MaterialEffectPresenter)MemberwiseClone();
			clone.snapshot = null;
			clone.Scene = Scene.Clone<Widget>();
			return clone;
		}
	}
}
