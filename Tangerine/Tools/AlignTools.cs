using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Tangerine.UI.SceneView;

namespace Tangerine
{

	public enum AlignObject
	{
		Selection,
		KeyObject,
		Root
	}

	public class AlignToHandler : CommandHandler
	{
		ICommand command;

		public AlignToHandler(ICommand command)
		{
			this.command = command;
			this.command.Text = AlignToString(AlignPreferences.Instance.AlignObject);
		}

		private static string AlignToString(AlignObject align)
		{
			switch (align) {
				case AlignObject.Selection:
					return "Align to Selection";
				case AlignObject.KeyObject:
					return "Align to Key Object";
				case AlignObject.Root:
					return "Align to Root";
				default:
					throw new ArgumentException();
			}
		}

		public override void Execute()
		{
			AlignObjectContextMenu.Create(command);
		}

		private static class AlignObjectContextMenu
		{
		
			public static void Create(ICommand command)
			{
				var menu = new Menu();
				foreach (AlignObject alignObject in Enum.GetValues(typeof(AlignObject))) {
					menu.Add(new Command(AlignToString(alignObject),
						new ChangeAlignObject(command, alignObject).Execute));
				}
				menu.Popup();
			}

			private class ChangeAlignObject : CommandHandler
			{

				ICommand command;
				AlignObject alignObject;

				public ChangeAlignObject(ICommand command, AlignObject alignObject)
				{
					this.command = command;
					this.alignObject = alignObject;
				}

				public override void Execute()
				{
					AlignPreferences.Instance.AlignObject = alignObject;
					command.Text = AlignToString(alignObject);
				}
			}

		}
	}

	public class CenterHorizontally : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					float d = (container.Width - aabb.Width) / 2 - aabb.A.X;
					if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
						p.X += d;
						Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
					}
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					float d = (container.Width - aabb.Width) / 2 - aabb.A.X;
					if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
						p.X += d;
						Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
					}
				}
			}
		}
	}

	public class CenterVertically : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					float d = (container.Height - aabb.Height) / 2 - aabb.A.Y;
					if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
						p.Y += d;
						Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
					}
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					float d = (container.Height - aabb.Height) / 2 - aabb.A.Y;
					if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
						p.X += d;
						Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
					}
				}
			}
		}
	}

	public class AlignCentersHorizontally : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					float d = aabb.Center.X - widget.CalcAABBInSpaceOf(container).Center.X;
					if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
						p.X += d;
						Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
					}
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					if (Mathf.Abs(aabb.Center.X) > Mathf.ZeroTolerance) {
						p.X += aabb.Center.X;
						Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
					}
				}
			}
		}
	}

	public class AlignCentersVertically : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					float d = aabb.Center.Y - widget.CalcAABBInSpaceOf(container).Center.Y;
					if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
						p.Y += d;
						Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
					}
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					if (Mathf.Abs(aabb.Center.Y) > Mathf.ZeroTolerance) {
						p.Y += aabb.Center.Y;
						Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
					}
				}
			}
		}
	}

	public class AlignTop : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					float d = widget.CalcAABBInSpaceOf(container).Top - aabb.Top;
					if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
						p.Y -= d;
						Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
					}
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					if (Mathf.Abs(p.Y - aabb.Top) > Mathf.ZeroTolerance) {
						p.Y = aabb.Top;
						Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
					}
				}
			}
		}
	}

	public class AlignBottom : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					float d = widget.CalcAABBInSpaceOf(container).Bottom - aabb.Bottom;
					if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
						p.Y -= d;
						Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
					}
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					if (Mathf.Abs(p.Y - aabb.Bottom) > Mathf.ZeroTolerance) {
						p.Y = aabb.Bottom;
						Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
					}
				}
			}
		}
	}

	public class AlignLeft : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					float d = widget.CalcAABBInSpaceOf(container).Left - aabb.Left;
					if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
						p.X -= d;
						Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
					}
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					if (Mathf.Abs(p.X - aabb.Left) > Mathf.ZeroTolerance) {
						p.X = aabb.Left;
						Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
					}
				}
			}
		}
	}

	public class AlignRight : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				foreach (var widget in nodes.Editable().OfType<Widget>()) {
					var p = widget.Position;
					float d = widget.CalcAABBInSpaceOf(container).Right - aabb.Right;
					if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
						p.X -= d;
						Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
					}
				}
				foreach (var po in nodes.Editable().OfType<PointObject>()) {
					var p = po.Position;
					if (Mathf.Abs(p.X - aabb.Right) > Mathf.ZeroTolerance) {
						p.X = aabb.Right;
						Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
					}
				}
			}
		}
	}
}
