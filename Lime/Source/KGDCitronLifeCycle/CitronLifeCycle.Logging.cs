using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		private class LoggedCase
		{
			public int TotalCount;
			public readonly Dictionary<string, int> TotalCountByStack = new Dictionary<string, int>();

			public int PerSecondCount;
			public Dictionary<string, int> PerSecondCountByStack = new Dictionary<string, int>();

			public int AccCount;
			public Dictionary<string, int> AccCountByStack = new Dictionary<string, int>();

			private readonly Stopwatch stopwatch = new Stopwatch();

			public void FixLog(string prefix = null)
			{
				if (!IsLoggingActive) {
					return;
				}

				TotalCount++;
				AccCount++;

				string key = "";
				if (!string.IsNullOrEmpty(prefix)) {
					key += prefix + "\n";
				}
				key += new StackTrace().ToString();

				int count;

				TotalCountByStack.TryGetValue(key, out count);
				TotalCountByStack[key] = count + 1;

				AccCountByStack.TryGetValue(key, out count);
				AccCountByStack[key] = count + 1;
			}

			public void OnFrameStart()
			{
				if (!stopwatch.IsRunning) {
					stopwatch.Start();
				}

				if (!(stopwatch.Elapsed.TotalSeconds >= 1)) {
					return;
				}

				PerSecondCount = AccCount;
				PerSecondCountByStack = AccCountByStack;

				AccCount = 0;
				AccCountByStack = new Dictionary<string, int>();

				stopwatch.Restart();
			}

			public string GetDebugInfo(string name)
			{
				return name + ": " + PerSecondCount + "/" + TotalCount;
			}
		}

		public static bool IsLoggingActive { get; private set; }

		private static readonly LoggedCase managerNoneStateCase = new LoggedCase();
		private static readonly LoggedCase updateOnStartCase = new LoggedCase();
		private static readonly LoggedCase tasksUpdateOnStartCase = new LoggedCase();
		private static readonly LoggedCase advanceAnimationsRecursiveOnStartCase = new LoggedCase();
		private static readonly LoggedCase advanceAnimationsRecursiveAfterRunAnimationCase = new LoggedCase();
		private static readonly LoggedCase hierarchyChangedDuringAnimationCase = new LoggedCase();
		private static readonly LoggedCase hierarchyChangedDuringUpdatedPhaseCase = new LoggedCase();
		private static readonly LoggedCase runAnimationDuringUpdatedPhaseCase = new LoggedCase();
		private static readonly LoggedCase immediatelyOnStoppedCase = new LoggedCase();

		public static void ActivateLogging(ref Action onFrameStarting)
		{
			if (IsLoggingActive) {
				return;
			}
			IsLoggingActive = true;

			onFrameStarting += OnFrameStarting;
#pragma warning disable 618
			NodeManager.GlobalHierarchyChanged += OnHierarchyChanged;
#pragma warning restore 618
		}

		public static void DeactivateLogging(ref Action onFrameStarting)
		{
			if (!IsLoggingActive) {
				return;
			}
			IsLoggingActive = false;

			// ReSharper disable once DelegateSubtraction
			onFrameStarting -= OnFrameStarting;
#pragma warning disable 618
			NodeManager.GlobalHierarchyChanged -= OnHierarchyChanged;
#pragma warning restore 618
		}

		private static void OnHierarchyChanged(HierarchyChangedEventArgs e)
		{
			if (pipelinedAdvanceAnimationActiveCount > 0) {
				hierarchyChangedDuringAnimationCase.FixLog(e.Action + " " + e.Child);
			}
			if (e.Manager.GetPhase() >= NodeManagerPhase.Layout) {
				hierarchyChangedDuringUpdatedPhaseCase.FixLog(e.Action + " " + e.Child);
			}
		}

		private static void OnFrameStarting()
		{
			managerNoneStateCase.OnFrameStart();
			updateOnStartCase.OnFrameStart();
			tasksUpdateOnStartCase.OnFrameStart();
			advanceAnimationsRecursiveOnStartCase.OnFrameStart();
			advanceAnimationsRecursiveAfterRunAnimationCase.OnFrameStart();
			hierarchyChangedDuringAnimationCase.OnFrameStart();
			hierarchyChangedDuringUpdatedPhaseCase.OnFrameStart();
			runAnimationDuringUpdatedPhaseCase.OnFrameStart();
			immediatelyOnStoppedCase.OnFrameStart();
		}

		public static string[] GetDebugInfo()
		{
			if (!IsLoggingActive) {
				return new string[0];
			}

			return new[] {
				hierarchyChangedDuringAnimationCase.GetDebugInfo(nameof(hierarchyChangedDuringAnimationCase)),
				hierarchyChangedDuringUpdatedPhaseCase.GetDebugInfo(nameof(hierarchyChangedDuringUpdatedPhaseCase)),
				runAnimationDuringUpdatedPhaseCase.GetDebugInfo(nameof(runAnimationDuringUpdatedPhaseCase)),
				immediatelyOnStoppedCase.GetDebugInfo(nameof(immediatelyOnStoppedCase)),
				advanceAnimationsRecursiveAfterRunAnimationCase.GetDebugInfo(
					nameof(advanceAnimationsRecursiveAfterRunAnimationCase)
				),
				advanceAnimationsRecursiveOnStartCase.GetDebugInfo(nameof(advanceAnimationsRecursiveOnStartCase)),
				updateOnStartCase.GetDebugInfo(nameof(updateOnStartCase)),
				tasksUpdateOnStartCase.GetDebugInfo(nameof(tasksUpdateOnStartCase)),
				managerNoneStateCase.GetDebugInfo(nameof(managerNoneStateCase))
			};
		}
	}
}
