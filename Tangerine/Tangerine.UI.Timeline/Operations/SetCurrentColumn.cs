using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public class SetCurrentColumn : Operation
	{
		protected int Column;
		protected Node Container;

		public override bool IsChangingDocument => false;

		public static void Perform(int column, Node container)
		{
			Document.Current.History.Perform(new SetCurrentColumn(column, container));
		}

		public static void Perform(int column)
		{
			Document.Current.History.Perform(new SetCurrentColumn(column, Document.Current.Container));
		}

		private SetCurrentColumn(int column, Node node)
		{
			Column = column;
			Container = node;
		}

		public class Processor : OperationProcessor<SetCurrentColumn>
		{
			class Backup { public int Column; }

			protected override void InternalRedo(SetCurrentColumn op)
			{
				op.Save(new Backup { Column = Timeline.Instance.CurrentColumn });
				SetColumn(op.Column, op.Container);
			}

			protected override void InternalUndo(SetCurrentColumn op)
			{
				SetColumn(op.Restore<Backup>().Column, op.Container);
			}

			void SetColumn(int value, Node node)
			{
				Audio.GloballyEnable = false;
				try {
					var doc = Document.Current;
					if (Core.UserPreferences.Instance.Get<UserPreferences>().AnimationMode && doc.AnimationFrame != value) {
						node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, true);
						StopAnimationRecursive(node);
						SetTimeRecursive(node, 0);
						ClearParticlesRecursive(doc.RootNode);
						node.IsRunning = true;
						FastForwardToFrame(node, value);
						StopAnimationRecursive(node);
						node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, false);
					} else {
						node.AnimationFrame = value;
						node.Update(0);
						ClearParticlesRecursive(node);
					}
					Timeline.Instance.EnsureColumnVisible(value);
				} finally {
					Audio.GloballyEnable = true;
				}
			}

			static void FastForwardToFrame(Node node, int frame)
			{
				var forwardDelta = AnimationUtils.SecondsPerFrame * (frame - node.AnimationFrame);
				// Make sure that animation engine will invoke triggers on last frame
				forwardDelta += 0.000001;
				// Hack: CompatibilityAnimationEngine workaround
				if (Document.Current.Format == DocumentFormat.Scene) {
					forwardDelta *= 2f;
				}
				node.Update((float)forwardDelta);
				node.AnimationFrame = frame;
			}

			static void SetTimeRecursive(Node node, double time)
			{
				foreach (var animation in node.Animations) {
					animation.Time = time;
				}
				foreach (var child in node.Nodes) {
					SetTimeRecursive(child, time);
				}
			}

			static void StopAnimationRecursive(Node node)
			{
				foreach (var animation in node.Animations) {
					animation.IsRunning = false;
				}
				foreach (var child in node.Nodes) {
					StopAnimationRecursive(child);
				}
			}

			static void ClearParticlesRecursive(Node node)
			{
				if (node is ParticleEmitter) {
					var e = (ParticleEmitter)node;
					e.ClearParticles();
				}
				foreach (var child in node.Nodes) {
					ClearParticlesRecursive(child);
				}
			}
		}
	}
}