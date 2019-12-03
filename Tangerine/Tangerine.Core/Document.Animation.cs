using System;
using System.Linq;
using Lime;
using System.Collections.Generic;

namespace Tangerine.Core
{
	public sealed partial class Document
	{
		private List<(AnimationPath, double)> savedAnimationsTimes;
		private IAnimationPositioner compatAnimationPositioner = new CompatibilityAnimationPositioner();
		private IAnimationPositioner betterAnimationPositioner = new BetterAnimationPositioner();
		public IAnimationPositioner AnimationPositioner =>
			CoreUserPreferences.Instance.UseBetterAnimationPositioner ? betterAnimationPositioner : compatAnimationPositioner;

		public bool SlowMotion { get; set; }

		public static bool CacheAnimationsStates
		{
			get => Current.AnimationPositioner.CacheAnimationsStates;
			set => Current.AnimationPositioner.CacheAnimationsStates = value;
		}

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
					Container.Components.Add(new RestoreAnimationsTimesComponent(savedAnimationsTimes));
				}
				AudioSystem.StopAll();
				AnimationPositioner.CacheAnimationsStates = true;
				ForceAnimationUpdate();
				AnimationPositioner.CacheAnimationsStates = false;
			} else {
				SaveAnimationsTimes();
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

		[NodeComponentDontSerialize]
		[UpdateStage(typeof(PostLateUpdateStage))]
		private class RestoreAnimationsTimesComponent : BehaviorComponent
		{
			private readonly List<(AnimationPath, double)> savedAnimationsTimes;

			public RestoreAnimationsTimesComponent(List<(AnimationPath, double)> savedAnimationsTimes)
			{
				this.savedAnimationsTimes = savedAnimationsTimes;
			}

			protected override void Update(float delta)
			{
				foreach (var (animationPath, time) in savedAnimationsTimes) {
					animationPath.GetAnimation(Document.Current.RootNode).Time =
						CoreUserPreferences.Instance.AnimationMode && CoreUserPreferences.Instance.ResetAnimationsTimes ? 0 : time;
				}
				Owner.Components.Remove(this);
			}
		}

		private void SaveAnimationsTimes()
		{
			void Save(Node node)
			{
				foreach (var animation in node.Animations) {
					savedAnimationsTimes.Add((new AnimationPath(animation, Document.Current.RootNode), animation.Time));
				}
			}
			savedAnimationsTimes = new List<(AnimationPath, double)>();
			foreach (var node in Container.Descendants) {
				Save(node);
			}
			var currentNode = Container;
			do {
				Save(currentNode);
				currentNode = currentNode.Parent;
			} while (currentNode != RootNode.Parent);
		}
	}
}
