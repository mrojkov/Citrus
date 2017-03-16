using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Yuzu;

namespace Lime
{
	public class Model3DAttachment
	{
		public const string DefaultAnimationName = "Default";

		public List<Animation> Animations = new List<Animation>();

		public class Animation
		{
			public string Name;
			public int StartFrame;
			public int LastFrame;
			public List<Marker> Markers = new List<Marker>();
			public List<string> Nodes = new List<string>();
			public List<string> IgnoredNodes = new List<string>();
		}

		public void Apply(Node3D model)
		{
			ProcessAnimations(model);
		}

		private void ProcessAnimations(Node3D model)
		{
			if (Animations.Count == 0) {
				return;
			}
			if (model.Animations.Count == 0) {
				throw new Exception("Model attachment requires at least one animation");
			}
			var srcAnimation = model.Animations[0];
			foreach (var animationData in Animations) {
				var animation = srcAnimation;
				if (animationData.Name != Model3DAttachment.DefaultAnimationName) {
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

	class Model3DAttachmentParser
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
			public Dictionary<string, ModelAnimationFormat> Animations = null;

			[YuzuOptional]
			public Dictionary<string, ModelMaterialEffectFormat> MaterialEffects = null;

			[YuzuOptional]
			public List<UVAnimationFormat> UVAnimations = null;
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
		}

		public class ModelMarkerFormat
		{
			[YuzuOptional]
			public int Frame = 0;

			[YuzuOptional]
			public string Action = null;

			[YuzuOptional]
			public string JumpTarget = null;
		}

		public class ModelMaterialEffectFormat
		{
			[YuzuOptional]
			public string MaterialName = null;

			[YuzuOptional]
			public string Path = null;
		}

		public Model3DAttachment Parse(string modelPath)
		{
			var attachmentPath = modelPath + ".Attachment.txt";
			if (!AssetBundle.Instance.FileExists(attachmentPath)) {
				return null;
			}
			try {
				var modelAttachmentFormat = Serialization.ReadObject<ModelAttachmentFormat>(attachmentPath);
				var attachment = new Model3DAttachment();
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
							animation.Markers.Add(marker);
						}
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
				return attachment;
			} catch (System.Exception e) {
				throw new System.Exception(modelPath + ": " + e.Message, e);
			}
		}

		private int FixFrame(int frame, float fps = 16)
		{
			return AnimationUtils.MsecsToFrames((int)(frame * 1000 / fps + 0.5));
		}
	}
}