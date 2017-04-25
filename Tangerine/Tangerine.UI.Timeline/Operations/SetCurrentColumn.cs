using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public class SetCurrentColumn : Operation
	{
		protected int Column;

		public override bool IsChangingDocument => false;

		public static void Perform(int column)
		{
			Document.Current.History.Perform(new SetCurrentColumn(column));
		}

		private SetCurrentColumn(int column)
		{
			Column = column;
		}

		public class Processor : OperationProcessor<SetCurrentColumn>
		{
			class Backup { public int Column; }

			protected override void InternalRedo(SetCurrentColumn op)
			{
				op.Save(new Backup { Column = Timeline.Instance.CurrentColumn });
				SetColumn(op.Column);
			}

			protected override void InternalUndo(SetCurrentColumn op)
			{
				SetColumn(op.Restore<Backup>().Column);
			}
		
			void SetColumn(int value)
			{
				var doc = Document.Current;
				if (UserPreferences.Instance.AnimationMode && doc.AnimationFrame != value) {
					doc.Container.SetTangerineFlag(TangerineFlags.IgnoreMarkers, true);
					if (doc.AnimationFrame < value) {
						doc.Container.IsRunning = true;
						UpdateToFrame(doc.Container, value);
					} else {
						SetCurrentFrameRecursive(doc.Container, 0);
						ClearParticlesRecursive(doc.RootNode);
						doc.Container.IsRunning = true;
						UpdateToFrame(doc.Container, value);
					}
					StopAnimationRecursive(doc.Container);
					doc.Container.SetTangerineFlag(TangerineFlags.IgnoreMarkers, false);
				} else {
					doc.AnimationFrame = value;
					doc.Container.Update(0);
					ClearParticlesRecursive(doc.RootNode);
				}
				Timeline.Instance.EnsureColumnVisible(value);
			}

			static void UpdateToFrame(Node node, int frame)
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