using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace Lime
{
	[DebuggerTypeProxy(typeof(WidgetDebugView))]
	internal class WidgetDebugView
	{
		private Widget widget;

		public struct WidgetBasis
		{
			public Vector2 Position;
			public Vector2 Scale;
			public float Rotation;

			public override string ToString()
			{
				return string.Format("Position: {0}; Scale: {1}; Rotation: {2}",
					Position, Scale, Rotation);
			}
		}

		public struct WidgetPainting
		{
			public Color4 Color;
			public Blending Blending;
			public bool Visible;
			public override string ToString()
			{
				return string.Format("Visible: {0}, Color: {1}; Blending: {2}", Visible, Color, Blending);
			}
		}

		public WidgetDebugView(Widget widget)
		{
			this.widget = widget;
		}

		public string Id { get { return widget.Id; } }

		public Node Parent { get { return widget.Parent; } }

		public Marker[] Markers { get { return widget.DefaultAnimation.Markers.ToArray(); } }

		public Vector2 Size { get { return widget.Size; } }

		public Vector2 Pivot { get { return widget.Pivot; } }

		public WidgetBasis LocalBasis
		{
			get
			{
				return new WidgetBasis {
					Position = widget.Position,
					Scale = widget.Scale,
					Rotation = widget.Rotation,
				};
			}
		}

		public WidgetPainting LocalPainting
		{
			get
			{
				return new WidgetPainting {
					Visible = widget.Visible,
					Color = widget.Color,
					Blending = widget.Blending
				};
			}
		}

		public WidgetBasis GlobalBasis
		{
			get
			{
				var b = widget.LocalToWorldTransform.ToTransform2();
				return new WidgetBasis {
					Position = b.Translation,
					Rotation = b.Rotation,
					Scale = b.Scale,
				};
			}
		}

		public WidgetPainting GlobalPainting
		{
			get
			{
				return new WidgetPainting {
					Visible = widget.GloballyVisible,
					Color = widget.GlobalColor,
					Blending = widget.GlobalBlending
				};
			}
		}

		public Node[] Nodes { get { return widget.Nodes.ToArray(); } }

		public string[] VisibilityIssues
		{
			get {
				return widget.GetVisibilityIssues().ToArray();
			}
		}
	}
}
