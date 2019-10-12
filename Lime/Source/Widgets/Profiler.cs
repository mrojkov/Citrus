using System;

namespace Lime
{
	public class Profiler : Widget
	{
		public const int MaxFramesCount = 256;

		public static Profiler Instance { get; private set; }

		public Profiler(Widget panel)
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = this;
			Presenter = new WidgetFlatFillPresenter(new Color4(53, 53, 55, /* TODO */ 255));
			AddNode(new ProfilerTab<UsageGPU>());
			panel.AddNode(this);
		}

		private enum ChartsInfoGPU : int
		{
			Opaque = 0,
			Transparent = 1,
			PostProcessing = 2
		}

		private class UsageGPU : AreaChartsGroup<ChartsInfoGPU>
		{
			public UsageGPU() : base(MaxFramesCount) { }
		}

		private class ProfilerTab<TChartsGroup> : Widget where TChartsGroup : ChartsGroup, new()
		{
			public readonly ChartLegend Legend;
			public readonly TChartsGroup ChartsGroup;

			public ProfilerTab() : base()
			{
				Layout = new HBoxLayout { Spacing = 6 };
				Presenter = new WidgetFlatFillPresenter(new Color4(53, 53, 55, /* TODO */ 255));

				ChartsGroup = new TChartsGroup();
				Legend = new ChartLegend(ChartsGroup);

				AddNode(Legend);
				AddNode(ChartsGroup);
			}
		}
	}
}
