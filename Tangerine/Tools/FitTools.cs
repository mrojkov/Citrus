using System.Linq;
using Lime;
using Tangerine.UI;

namespace Tangerine
{
	public class RestoreOriginalSize : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			foreach (var node in Core.Document.Current.SelectedNodes().Editable()) {
				if (node is Widget) {
					var widget = node as Widget;
					var originalSize = widget.Texture == null ? Widget.DefaultWidgetSize : (Vector2)widget.Texture.ImageSize;
					Core.Operations.SetAnimableProperty.Perform(node, nameof(Widget.Size), originalSize);
				} else if (node is ParticleModifier) {
					var particleModifier = node as ParticleModifier;
					var originalSize = particleModifier.Texture == null ? Widget.DefaultWidgetSize : (Vector2)particleModifier.Texture.ImageSize;
					Core.Operations.SetAnimableProperty.Perform(node, nameof(ParticleModifier.Size), originalSize);
				}
			}
		}
	}

	public class ResetScale : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Scale), Vector2.One);
			}
		}
	}

	public class ResetRotation : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Rotation), 0);
			}
		}
	}

	public class FlipX : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				var s = widget.Scale;
				s.X = -s.X;
				Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Scale), s);
			}
		}
	}

	public class FlipY : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				var s = widget.Scale;
				s.Y = -s.Y;
				Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Scale), s);
			}
		}
	}

	public class FitToContainer : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			var container = (Widget)Core.Document.Current.Container;
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Size), container.Size);
				Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Rotation), 0);
				Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), widget.Pivot * container.Size);
				Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Scale), Vector2.One);
				Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Anchors), Anchors.LeftRightTopBottom);
			}
		}
	}

	public class FitToContent : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes().Editable();
			foreach (var widget in nodes.OfType<Widget>()) {
				Rectangle aabb;
				if (Utils.CalcAABB(widget.Nodes, widget, out aabb)) {
					foreach (var w in widget.Nodes.OfType<Widget>()) {
						Core.Operations.SetAnimableProperty.Perform(w, nameof(Widget.Position), w.Position - aabb.A);
					}
					foreach (var po in widget.Nodes.OfType<PointObject>()) {
						Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), po.Position - aabb.A);
					}
					var p0 = widget.CalcTransitionToSpaceOf(container) * aabb.A;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Size), aabb.Size);
					var p1 = widget.CalcTransitionToSpaceOf(container) * Vector2.Zero;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), widget.Position + p0 - p1);
				}
			}
		}
	}
}
