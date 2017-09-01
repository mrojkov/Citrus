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
						SetCurrentFrameRecursive(node, 0);
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
				node.Update((float)(AnimationUtils.SecondsPerFrame * (frame - node.AnimationFrame)));
				// Set animation frame explicitly to avoid inaccuracy, leading to skipped markers, triggers, etc.
				node.AnimationFrame = frame;
			}

			static void SetCurrentFrameRecursive(Node node, int frame)
			{
				node.AnimationFrame = frame;
				foreach (var child in node.Nodes) {
					SetCurrentFrameRecursive(child, frame);
				}
			}

			static void StopAnimationRecursive(Node node)
			{
				node.IsRunning = false;
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