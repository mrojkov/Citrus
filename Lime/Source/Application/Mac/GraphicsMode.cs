#if MAC || MONOMAC
using System;

namespace Lime.Platform
{
	public class GraphicsMode : IEquatable<GraphicsMode>
	{
		private int samples;

		private static GraphicsMode defaultMode;
		private static readonly object sync = new object();

		internal GraphicsMode(GraphicsMode mode)
			: this(mode.ColorFormat, mode.Depth, mode.Stencil, mode.Samples, mode.AccumulatorFormat, mode.Buffers)
		{
		}

		internal GraphicsMode(ColorFormat color, int depth, int stencil, int samples, ColorFormat accum, int buffers)
		{
			if (depth < 0)
				throw new ArgumentOutOfRangeException("depth", "Must be greater than, or equal to zero.");
			if (stencil < 0)
				throw new ArgumentOutOfRangeException("stencil", "Must be greater than, or equal to zero.");
			if (buffers < 0)
				throw new ArgumentOutOfRangeException("buffers", "Must be greater than, or equal to zero.");
			if (samples < 0)
				throw new ArgumentOutOfRangeException("samples", "Must be greater than, or equal to zero.");

			this.ColorFormat = color;
			this.Depth = depth;
			this.Stencil = stencil;
			this.Samples = samples;
			this.AccumulatorFormat = accum;
			this.Buffers = buffers;
		}

		public GraphicsMode()
			: this(Default)
		{
		}

		public GraphicsMode(ColorFormat color)
			: this(color, Default.Depth, Default.Stencil, Default.Samples, Default.AccumulatorFormat, Default.Buffers)
		{
		}

		public GraphicsMode(ColorFormat color, int depth)
			: this(color, depth, Default.Stencil, Default.Samples, Default.AccumulatorFormat, Default.Buffers)
		{
		}

		public GraphicsMode(ColorFormat color, int depth, int stencil)
			: this(color, depth, stencil, Default.Samples, Default.AccumulatorFormat, Default.Buffers)
		{
		}

		public GraphicsMode(ColorFormat color, int depth, int stencil, int samples)
			: this(color, depth, stencil, samples, Default.AccumulatorFormat, Default.Buffers)
		{
		}

		public GraphicsMode(ColorFormat color, int depth, int stencil, int samples, ColorFormat accum)
			: this(color, depth, stencil, samples, accum, Default.Buffers)
		{
		}

		public ColorFormat ColorFormat { get; private set; }

		public ColorFormat AccumulatorFormat { get; private set; }

		public int Depth { get; private set; }

		public int Stencil { get; private set; }

		public int Buffers { get; private set; }

		public int Samples 
		{
			get { return samples; }
			private set { samples = value; }
		}

		public static GraphicsMode Default
		{
			get
			{
				lock (sync) {
					if (defaultMode == null) {
						defaultMode = new GraphicsMode(32, 24, 8, 0, 0, 2);
					}
					return defaultMode;
				}
			}
		}

		public override string ToString()
		{
			return String.Format("Color: {0}, Depth: {1}, Stencil: {2}, Samples: {3}, Accum: {4}, Buffers: {5}",
				ColorFormat, Depth, Stencil, Samples, AccumulatorFormat, Buffers);
		}

		public override int GetHashCode()
		{
			return ColorFormat.GetHashCode() ^ Depth ^ Stencil ^ Samples ^ AccumulatorFormat.GetHashCode() ^ Buffers;
		}

		public override bool Equals(object obj)
		{
			var graphicsMode = obj as GraphicsMode;
			return graphicsMode != null && Equals(graphicsMode);
		}

		public bool Equals(GraphicsMode other)
		{
			return GetHashCode() == other.GetHashCode();
		}
	}
}
#endif

