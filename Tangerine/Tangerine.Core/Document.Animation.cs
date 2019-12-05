using System;
using System.Linq;
using Lime;
using System.Collections.Generic;

namespace Tangerine.Core
{
	public sealed partial class Document
	{
		private readonly IAnimationPositioner AnimationPositioner = new AnimationPositioner();

		public bool SlowMotion { get; set; }

		public static void SetCurrentFrameToNode(Animation animation, int frameIndex, bool stopAnimations = true)
		{
			Current.AnimationPositioner.SetAnimationFrame(animation, frameIndex, CoreUserPreferences.Instance.AnimationMode, stopAnimations);
		}

		public void TogglePreviewAnimation()
		{
			if (PreviewAnimation) {
				PreviewAnimation = false;
				PreviewScene = false;
				Animation.IsRunning = false;
				StopAnimationRecursive(PreviewAnimationContainer);
				if (!CoreUserPreferences.Instance.StopAnimationOnCurrentFrame) {
					SetCurrentFrameToNode(Animation, PreviewAnimationBegin);
				}
				AudioSystem.StopAll();
				ForceAnimationUpdate();
				ClearParticlesRecursive(Animation.OwnerNode);
			} else {
				foreach (var node in RootNode.Descendants) {
					if (node is ITangerinePreviewAnimationListener t) {
						t.OnStart();
					}
				}
				int savedAnimationFrame = AnimationFrame;
				SetCurrentFrameToNode(Animation, AnimationFrame, stopAnimations: false);
				PreviewScene = true;
				PreviewAnimation = true;
				Animation.IsRunning = PreviewAnimation;
				PreviewAnimationBegin = savedAnimationFrame;
				PreviewAnimationContainer = Container;
			}
			Application.InvalidateWindows();
		}

		private static void ClearParticlesRecursive(Node node)
		{
			if (node is ParticleEmitter emitter) {
				emitter.ClearParticles();
			}
			foreach (var child in node.Nodes) {
				ClearParticlesRecursive(child);
			}
		}

		private static void StopAnimationRecursive(Node node)
		{
			void StopAnimation(Node n)
			{
				foreach (var animation in n.Animations) {
					animation.IsRunning = false;
				}
			}
			StopAnimation(node);
			foreach (var descendant in node.Descendants) {
				StopAnimation(descendant);
			}
		}

		public void ForceAnimationUpdate()
		{
			SetCurrentFrameToNode(Current.Animation, Current.AnimationFrame);
		}
	}
}
