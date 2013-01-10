using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;
using OpenTK.Graphics.OpenGL;

namespace Tangerine
{
	public class DocumentCanvas : QGLWidget
	{
		public static QGLContext sharedContext = new QGLContext(QGLFormat.DefaultFormat);
		public static QGLWidget sharedWidget = new QGLWidget(sharedContext, The.DefaultQtParent);

		public DocumentCanvas(QWidget parent)
			: base(parent, sharedWidget)
		{
		}

		protected override void InitializeGL()
		{
			base.InitializeGL();
		}

		protected override void ResizeGL(int w, int h)
		{
			Lime.Renderer.Viewport = new Lime.Viewport { X = 0, Y = 0, Width = w, Height = h };
			Lime.Renderer.SetOrthogonalProjection(0, 0, w, h);
		}

		protected override void PaintGL()
		{
			The.Document.RootNode.SafeUpdate(0);
			Lime.Renderer.BeginFrame();
			var chain = new Lime.RenderChain();
			The.Document.RootNode.AddToRenderChain(chain);
			chain.RenderAndClear();
			Lime.Renderer.EndFrame();
		}
	}
}
