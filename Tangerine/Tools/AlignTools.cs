using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.UI;

namespace Tangerine
{
	public class CenterHorizontally : DocumentCommandHandler
	{
		public override void Execute()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					p.X += (container.Width - aabb.Width) / 2 - aabb.A.X;
					Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), p);
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					p.X += (container.Width - aabb.Width) / 2 - aabb.A.X;
					Core.Operations.SetProperty.Perform(po, nameof(Widget.Position), p);
				}
			}
		}
	}

	public class CenterVertically : DocumentCommandHandler
	{
		public override void Execute()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					p.Y += (container.Height - aabb.Height) / 2 - aabb.A.Y;
					Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), p);
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					p.Y += (container.Height - aabb.Height) / 2 - aabb.A.Y;
					Core.Operations.SetProperty.Perform(po, nameof(Widget.Position), p);
				}
			}
		}
	}

	public class AlignCentersHorizontally : DocumentCommandHandler
	{
		public override void Execute()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					p.X += aabb.Center.X - widget.CalcAABBInSpaceOf(container).Center.X;
					Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), p);
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					p.X += aabb.Center.X;
					Core.Operations.SetProperty.Perform(po, nameof(Widget.Position), p);
				}
			}
		}
	}

	public class AlignCentersVertically : DocumentCommandHandler
	{
		public override void Execute()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					p.Y += aabb.Center.Y - widget.CalcAABBInSpaceOf(container).Center.Y;
					Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), p);
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					p.Y += aabb.Center.Y;
					Core.Operations.SetProperty.Perform(po, nameof(Widget.Position), p);
				}
			}
		}
	}

	public class AlignTop : DocumentCommandHandler
	{
		public override void Execute()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					p.Y -= widget.CalcAABBInSpaceOf(container).Top - aabb.Top;
					Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), p);
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					p.Y = aabb.Top;
					Core.Operations.SetProperty.Perform(po, nameof(Widget.Position), p);
				}
			}
		}
	}

	public class AlignBottom : DocumentCommandHandler
	{
		public override void Execute()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					p.Y -= widget.CalcAABBInSpaceOf(container).Bottom - aabb.Bottom;
					Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), p);
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					p.Y = aabb.Bottom;
					Core.Operations.SetProperty.Perform(po, nameof(Widget.Position), p);
				}
			}
		}
	}

	public class AlignLeft : DocumentCommandHandler
	{
		public override void Execute()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					p.X -= widget.CalcAABBInSpaceOf(container).Left - aabb.Left;
					Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), p);
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					p.X = aabb.Left;
					Core.Operations.SetProperty.Perform(po, nameof(Widget.Position), p);
				}
			}
		}
	}

	public class AlignRight : DocumentCommandHandler
	{
		public override void Execute()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					p.X -= widget.CalcAABBInSpaceOf(container).Right - aabb.Right;
					Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), p);
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					p.X = aabb.Right;
					Core.Operations.SetProperty.Perform(po, nameof(Widget.Position), p);
				}
			}
		}
	}
}
