using System;
using System.Collections.Generic;
using System.Linq;
using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class AnimationBlender : BehaviorComponent
	{
		private Dictionary<string, BlendingProcess> blendings = new Dictionary<string, BlendingProcess>();

		[YuzuMember]
		public Dictionary<string, AnimationBlending> Options { get; private set; } = new Dictionary<string, AnimationBlending>();

		[TangerineInspect]
		public double BlendDuration
		{
			get {
				if (Owner == null) {
					return 0;
				}
				Options.TryGetValue(Owner.DefaultAnimation.Id ?? "", out var animationBlending);
				return animationBlending?.Option?.Frames ?? 0;
			}
			set {
				if (Owner == null) {
					throw new InvalidOperationException();
				}
				var animationId = Owner.DefaultAnimation.Id ?? "";
				if (!Options.TryGetValue(animationId, out var animationBlending)) {
					animationBlending = new AnimationBlending {
						Option = new BlendingOption()
					};
					Options.Add(animationId, animationBlending);
				}
				animationBlending.Option.Frames = value;
			}
		}

		public bool Enabled { get; set; } = true;

		private Node savedOwner;

		protected internal override void Start()
		{
			foreach (var animation in Owner.Animations) {
				animation.AnimationEngine = BlendAnimationEngine.Instance;
			}
			savedOwner = Owner;
		}

		protected internal override void Stop()
		{
			foreach (var animation in savedOwner.Animations) {
				animation.AnimationEngine = DefaultAnimationEngine.Instance;
			}
		}

		public void Attach(Animation animation, string markerId, string sourceMarkerId = null)
		{
			if (!Enabled) {
				return;
			}
			blendings.Remove(animation.Id ?? "");

			BlendingOption blendingOption = null;
			Options.TryGetValue(animation.Id ?? "", out var animationBlending);
			if (animationBlending != null) {
				if (animationBlending.Option != null) {
					blendingOption = animationBlending.Option;
				}

				animationBlending.MarkersOptions.TryGetValue(markerId, out var markerBlending);
				if (markerBlending != null) {
					if (markerBlending.Option != null) {
						blendingOption = markerBlending.Option;
					}
					if (!string.IsNullOrEmpty(sourceMarkerId)) {
						markerBlending.SourceMarkersOptions.TryGetValue(sourceMarkerId, out var sourceMarkerBlending);
						if (sourceMarkerBlending != null) {
							blendingOption = sourceMarkerBlending;
						}
					}
				}
			}
			if (blendingOption == null) {
				return;
			}

			var blending = new BlendingProcess(Owner, animation, blendingOption.Duration);
			if (blending.HasNodes) {
				blendings.Add(animation.Id ?? "", blending);
			}
		}

		public void UpdateWantedState(Animation animation)
		{
			blendings.TryGetValue(animation.Id ?? "", out var blending);
			blending?.SaveWantedState();
		}

		public void Update(Animation animation, float delta)
		{
			blendings.TryGetValue(animation.Id ?? "", out var blending);
			if (blending == null) {
				return;
			}

			blending.Update(delta);
			if (blending.WasFinished) {
				blendings.Remove(animation.Id ?? "");
			}
		}

		public override NodeComponent Clone()
		{
			var clone = (AnimationBlender)base.Clone();
			clone.blendings = new Dictionary<string, BlendingProcess>();
			return clone;
		}

		private class BlendingProcess
		{
			private readonly double duration;
			private readonly List<NodeState> nodeStates = new List<NodeState>();
			private float time;

			public bool HasNodes => nodeStates.Count > 0;
			public bool WasFinished => time >= duration;

			public BlendingProcess(Node node, Animation animation, double duration)
			{
				this.duration = duration;

				foreach (var descendant in node.Descendants) {
					var nodeState = NodeState.TryGetState(descendant, animation);
					if (nodeState != null) {
						nodeStates.Add(nodeState);
					}
				}
			}

			public void SaveWantedState()
			{
				if (WasFinished) {
					return;
				}

				foreach (var nodeState in nodeStates) {
					nodeState.SaveWantedState();
				}
			}

			public void Update(float delta)
			{
				time += delta;
				if (WasFinished) {
					return;
				}

				var factor = (float)(time / duration);
				foreach (var nodeState in nodeStates) {
					nodeState.Blend(factor);
				}
			}
		}

		private abstract class NodeState
		{
			public static NodeState TryGetState(Node node, Animation animation)
			{
				if (node is Node3D node3D) {
					return Node3DState.TryGetState(node3D, animation);
				}
				if (node is Widget widget) {
					return WidgetState.TryGetState(widget, animation);
				}
				if (node is DistortionMeshPoint point) {
					return DistortionMeshPointState.TryGetState(point, animation);
				}
				if (node is Bone bone) {
					return BoneState.TryGetState(bone, animation);
				}
				return null;
			}

			public abstract void SaveWantedState();
			public abstract void Blend(float factor);

			protected static float Interpolate2DRotation(float factor, float from, float to)
			{
				from = Mathf.Wrap360(from);
				to = Mathf.Wrap360(to);
				var step = to - from;

				if (Mathf.Abs(step) > 180f) {
					step = Mathf.Sign(step) * (Mathf.Abs(step) - 360);
				}

				return from + step * factor;
			}
		}

		private class Node3DState : NodeState
		{
			private readonly Node3D node3D;
			private readonly Vector3 position;
			private readonly Quaternion rotation;
			private readonly Vector3 scale;
			private Vector3 wantedPosition;
			private Quaternion wantedRotation;
			private Vector3 wantedScale;

			public static NodeState TryGetState(Node3D node3D, Animation animation)
			{
				if (node3D is Mesh3D mesh3D && mesh3D.Submeshes.All(sm => sm.Bones.Count == 0)) {
					var existsBones = false;
					for (var i = 0; i < mesh3D.Submeshes.Count; i++) {
						if (mesh3D.Submeshes[i].Bones.Count != 0) {
							continue;
						}
						existsBones = true;
						break;
					}
					if (!existsBones) {
						return null;
					}
				}
				node3D.Animators.TryFind("Position", out Animator<Vector3> positionAnimator, animation.Id);
				node3D.Animators.TryFind("Rotation", out Animator<Quaternion> rotationAnimator, animation.Id);
				node3D.Animators.TryFind("Scale", out Animator<Vector3> scaleAnimator, animation.Id);
				return
					positionAnimator != null || rotationAnimator != null || scaleAnimator != null ?
					new Node3DState(node3D) :
					null;
			}

			private Node3DState(Node3D node3D)
			{
				this.node3D = node3D;
				position = wantedPosition = node3D.Position;
				rotation = wantedRotation = node3D.Rotation;
				scale = wantedScale = node3D.Scale;
			}

			public override void SaveWantedState()
			{
				wantedPosition = node3D.Position;
				wantedRotation = node3D.Rotation;
				wantedScale = node3D.Scale;
			}

			public override void Blend(float factor)
			{
				node3D.Position = Vector3.Lerp(factor, position, wantedPosition);
				node3D.Rotation = Quaternion.Slerp(rotation, wantedRotation, factor);
				node3D.Scale = Vector3.Lerp(factor, scale, wantedScale);
			}
		}

		private class WidgetState : NodeState
		{
			private readonly Widget widget;
			private readonly Vector2 position;
			private readonly float rotation;
			private readonly Vector2 scale;
			private readonly Vector2 size;
			private readonly Color4 color;
			private readonly bool visible;

			private Vector2 wantedPosition;
			private float wantedRotation;
			private Vector2 wantedScale;
			private Vector2 wantedSize;
			private Color4 wantedColor;
			private bool wantedVisible;

			public static NodeState TryGetState(Widget widget, Animation animation)
			{
				if (widget is ParticleEmitter || widget is ParticlesMagnet) {
					return null;
				}

				widget.Animators.TryFind("Position", out Animator<Vector2> positionAnimator, animation.Id);
				widget.Animators.TryFind("Rotation", out Animator<float> rotationAnimator, animation.Id);
				widget.Animators.TryFind("Scale", out Animator<Vector2> scaleAnimator, animation.Id);
				widget.Animators.TryFind("Size", out Animator<Vector2> sizeAnimator, animation.Id);
				widget.Animators.TryFind("Color", out Animator<Color4> colorAnimator, animation.Id);
				widget.Animators.TryFind("Visible", out Animator<bool> visibleAnimator, animation.Id);
				var existsAtLeastOneAnimator =
					positionAnimator != null ||
					rotationAnimator != null ||
					scaleAnimator != null ||
					sizeAnimator != null ||
					colorAnimator != null ||
					visibleAnimator != null;
				return existsAtLeastOneAnimator ? new WidgetState(widget) : null;
			}

			private WidgetState(Widget widget)
			{
				this.widget = widget;
				position = wantedPosition = widget.Position;
				rotation = wantedRotation = widget.Rotation;
				scale = wantedScale = widget.Scale;
				size = wantedSize = widget.Size;
				color = wantedColor = widget.Color;
				visible = wantedVisible = widget.Visible;
			}

			public override void SaveWantedState()
			{
				wantedPosition = widget.Position;
				wantedRotation = widget.Rotation;
				wantedScale = widget.Scale;
				wantedSize = widget.Size;
				wantedColor = widget.Color;
				wantedVisible = widget.Visible;
			}

			public override void Blend(float factor)
			{
				widget.Position = Vector2.Lerp(factor, position, wantedPosition);
				widget.Rotation = Interpolate2DRotation(factor, rotation, wantedRotation);
				widget.Scale = Vector2.Lerp(factor, scale, wantedScale);
				widget.Size = Vector2.Lerp(factor, size, wantedSize);
				widget.Color = Color4.Lerp(factor, color, wantedColor);
				widget.Visible = visible || wantedVisible;
			}
		}

		private class DistortionMeshPointState : NodeState
		{
			private readonly DistortionMeshPoint point;
			private readonly Vector2 position;
			private readonly Color4 color;

			private Vector2 wantedPosition;
			private Color4 wantedColor;

			public static NodeState TryGetState(DistortionMeshPoint point, Animation animation)
			{
				point.Animators.TryFind("Position", out Animator<Vector2> positionAnimator, animation.Id);
				point.Animators.TryFind("Color", out Animator<Color4> colorAnimator, animation.Id);

				return positionAnimator != null || colorAnimator != null ? new DistortionMeshPointState(point) : null;
			}

			private DistortionMeshPointState(DistortionMeshPoint point)
			{
				this.point = point;
				position = wantedPosition = point.Position;
				color = wantedColor = point.Color;
			}

			public override void SaveWantedState()
			{
				wantedPosition = point.Position;
				wantedColor = point.Color;
			}

			public override void Blend(float factor)
			{
				point.Position = Vector2.Lerp(factor, position, wantedPosition);
				point.Color = Color4.Lerp(factor, color, wantedColor);
			}
		}

		private class BoneState : NodeState
		{
			private readonly Bone bone;
			private readonly Vector2 position;
			private readonly float rotation;

			private Vector2 wantedPosition;
			private float wantedRotation;

			public static NodeState TryGetState(Bone bone, Animation animation)
			{
				bone.Animators.TryFind("Position", out Animator<Vector2> positionAnimator, animation.Id);
				bone.Animators.TryFind("Rotation", out Animator<float> rotationAnimator, animation.Id);

				return positionAnimator != null || rotationAnimator != null ? new BoneState(bone) : null;
			}

			private BoneState(Bone bone)
			{
				this.bone = bone;
				position = wantedPosition = bone.Position;
				rotation = wantedRotation = bone.Rotation;
			}

			public override void SaveWantedState()
			{
				wantedPosition = bone.Position;
				wantedRotation = bone.Rotation;
			}

			public override void Blend(float factor)
			{
				bone.Position = Vector2.Lerp(factor, position, wantedPosition);
				bone.Rotation = Interpolate2DRotation(factor, rotation, wantedRotation);
			}
		}
	}

	public class AnimationBlending
	{
		[YuzuMember]
		public BlendingOption Option;

		[YuzuMember]
		public Dictionary<string, MarkerBlending> MarkersOptions = new Dictionary<string, MarkerBlending>();
	}

	public class MarkerBlending
	{
		[YuzuMember]
		public BlendingOption Option;

		[YuzuMember]
		public Dictionary<string, BlendingOption> SourceMarkersOptions = new Dictionary<string, BlendingOption>();
	}

	public class BlendingOption
	{
		[YuzuMember]
		public double Duration { get; set; }

		public double Frames
		{
			get => AnimationUtils.SecondsToFrames(Duration);
			set => Duration = value * AnimationUtils.SecondsPerFrame;
		}

		public BlendingOption() { }

		public BlendingOption(double duration)
		{
			Duration = duration;
		}

		public BlendingOption(int frames)
		{
			Frames = frames;
		}
	}
}
