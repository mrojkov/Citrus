using System;
using System.Collections.Generic;
using System.Threading;
using Yuzu;

namespace Lime
{
	public class Animation : ICloneable
	{
#if TANGERINE
		public static Func<Animation, bool> EasingEnabledChecker;
#endif
		private string id;
		private bool isRunning;
		private bool animatorsArePropagated;
		private bool? hasEasings;
		internal Animation Next;
		internal double TimeInternal;
		internal Marker MarkerAhead;
		public event Action Stopped;
		public string RunningMarkerId { get; set; }
		public EasingCalculator EasingCalculator { get; private set; }
		public AnimationEngine AnimationEngine = DefaultAnimationEngine.Instance;
		internal List<IAbstractAnimator> EffectiveAnimators;
		internal List<IAbstractAnimator> EffectiveTriggerableAnimators;
		internal int EffectiveAnimatorsVersion;

		[YuzuMember]
		public bool IsCompound { get; set; }

		[YuzuMember]
		[TangerineIgnore]
		public AnimationTrackList Tracks { get; private set; }

		[YuzuMember]
		[TangerineIgnore]
		public MarkerList Markers { get; private set; }

		[YuzuMember]
		public string Id
		{
			get => id;
			set {
				if (id != value) {
					IdComparisonCode = Toolbox.StringUniqueCodeGenerator.Generate(value);
					id = value;
				}
			}
		}

		public int IdComparisonCode { get; private set; }

		[YuzuMember]
		[TangerineIgnore]
		public bool IsLegacy { get; set; }

		[YuzuMember]
		public string ContentsPath { get; set; }

		public double Time
		{
			get { return TimeInternal; }
			set
			{
				TimeInternal = value;
				MarkerAhead = null;
				RunningMarkerId = null;
				ApplyAnimators();
			}
		}

		public int Frame
		{
			get { return AnimationUtils.SecondsToFrames(Time); }
			set { Time = AnimationUtils.FramesToSeconds(value); }
		}

		public Node Owner { get; internal set; }

		public bool IsRunning
		{
			get { return isRunning; }
			set
			{
				if (isRunning != value) {
					isRunning = value;
					if (isRunning) {
						Load();
					}
					Owner?.RefreshRunningAnimationCount();
				}
			}
		}

		public Animation()
		{
			Markers = new MarkerList(this);
			Tracks = new AnimationTrackList(this);
			EasingCalculator = new EasingCalculator(Markers, this);
		}

		public void Advance(float delta)
		{
			if (IsRunning) {
				AnimationEngine.AdvanceAnimation(this, delta);
			}
		}

		public void FindAnimators(List<IAnimator> animators)
		{
			if (Owner != null) {
				foreach (var node in Owner.Nodes) {
					FindAnimators(node, animators);
				}
			}
		}

		private void FindAnimators(Node node, List<IAnimator> animators)
		{
			foreach (var animator in node.Animators) {
				if (animator.AnimationId == Id) {
					animators.Add(animator);
				}
			}
			if (IsLegacy) {
				return;
			}
			foreach (var animation in node.Animations) {
				if (animation.Id == Id) {
					return;
				}
			}
			foreach (var child in node.Nodes) {
				FindAnimators(child, animators);
			}
		}

		public void Run(string markerId = null)
		{
			if (!TryRun(markerId)) {
				throw new Exception($"Unknown marker '{markerId}'");
			}
		}

		public bool TryRun(string markerId = null, double animationTimeCorrection = 0)
		{
			if (AnimationEngine.TryRunAnimation(this, markerId, animationTimeCorrection)) {
				Stopped = null;
				return true;
			}
			return false;
		}

		internal void InvalidateCache()
		{
			MarkerAhead = null;
			EffectiveAnimators = null;
			hasEasings = null;
			EasingCalculator.Invalidate();
		}

		public void ApplyAnimators()
		{
			Load();
			AnimationEngine.ApplyAnimatorsAndExecuteTriggers(this, Time, Time, false);
		}

		internal void RaiseStopped()
		{
			Stopped?.Invoke();
		}

		public Animation Clone()
		{
			var clone = (Animation)MemberwiseClone();
			clone.Owner = null;
			clone.Next = null;
			clone.Markers = MarkerList.DeepClone(Markers, clone);
			clone.Tracks = Tracks.Clone(clone);
			clone.EffectiveAnimators = null;
			clone.EffectiveTriggerableAnimators = null;
			clone.EffectiveAnimatorsVersion = 0;
			clone.EasingCalculator = new EasingCalculator(clone.Markers, clone);
			return clone;
		}

		public int CalcDurationInFrames()
		{
			if (!AnimationEngine.AreEffectiveAnimatorsValid(this)) {
				AnimationEngine.BuildEffectiveAnimators(this);
			}
			var durationInFrames = 0;
			foreach (var a in EffectiveAnimators) {
				durationInFrames = Math.Max(durationInFrames, a.Duration);
			}
			if (Markers.Count > 0) {
				durationInFrames = Math.Max(durationInFrames, Markers[Markers.Count - 1].Frame);
			}
			return durationInFrames;
		}

		public double CalcDurationInSeconds() => CalcDurationInFrames() * AnimationUtils.SecondsPerFrame;

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Load()
		{
			if (animatorsArePropagated || string.IsNullOrEmpty(ContentsPath) || Owner == null) {
				return;
			}
			var d = AnimationData.Load(ContentsPath);
			foreach (var animator in d.Animators) {
				var clone = animator.Clone();
				var (host, index) = AnimationUtils.GetPropertyHost(Owner, clone.TargetPropertyPath);
				if (host == null) {
					continue;
				}
				clone.TargetPropertyPath = clone.TargetPropertyPath.Substring(index);
				host.Animators.Add(clone);
			}
			animatorsArePropagated = true;
		}

		public AnimationData GetData()
		{
			var d = new AnimationData();
			var animators = new List<IAnimator>();
			FindAnimators(animators);
			foreach (var animator in animators) {
				var node = (Node)animator.Owner;
				var propertyPath = $"{node.Id}/{animator.TargetPropertyPath}";
				while (node.Parent != Owner) {
					node = node.Parent;
					propertyPath = $"{node.Id}/{propertyPath}";
				}
				var clone = animator.Clone();
				clone.TargetPropertyPath = propertyPath;
				d.Animators.Add(clone);
			}
			return d;
		}

		public static string FixAntPath(string path) => path.Replace('|', '_');

		public bool HasEasings()
		{
			if (!hasEasings.HasValue) {
				hasEasings = false;
				foreach (var marker in Markers) {
					if (!marker.Easing.IsDefault()) {
						hasEasings = true;
					}
				}
			}
			return hasEasings.Value;
		}

		public class AnimationData
		{
			public delegate bool LoadingDelegate(string path, ref AnimationData instance);
			public delegate void LoadedDelegate(string path, AnimationData instance);
			public static ThreadLocal<LoadingDelegate> Loading;
			public static ThreadLocal<LoadedDelegate> Loaded;

			[YuzuMember]
			public List<IAnimator> Animators { get; private set; } = new List<IAnimator>();

			public static AnimationData Load(string path)
			{
				AnimationData instance = null;
				path = FixAntPath(path);
				path += ".ant";
				if (Loading?.Value?.Invoke(path, ref instance) ?? false) {
					Loaded?.Value?.Invoke(path, instance);
					return instance;
				}
				instance = Serialization.ReadObject<AnimationData>(path);
				Loaded?.Value?.Invoke(path, instance);
				return instance;
			}
		}
	}

	public class EasingCalculator
	{
		private Animation owner;
		private MarkerList markers;
		private double easingStartTime;
		private double easingEndTime;
		private double previousTime;
		private double easedPreviousTime;
		private double currentTime;
		private double easedCurrentTime;
		private CubicBezier easingCurve;

		public EasingCalculator(MarkerList markers, Animation owner)
		{
			this.owner = owner;
			this.markers = markers;
			Invalidate();
		}

		public void Invalidate()
		{
			easingCurve = null;
			easingStartTime = easingEndTime = 0;
			currentTime = previousTime = float.NaN;
		}

		private void CacheEasing(double time)
		{
			easingCurve = null;
			easingStartTime = 0;
			easingEndTime = 0;
			if (markers.Count == 0) {
				easingStartTime = double.NegativeInfinity;
				easingEndTime = double.PositiveInfinity;
				return;
			}
			var frame = AnimationUtils.SecondsToFrames(time);
			int i = -1;
			foreach (var marker in markers) {
				if (marker.Frame > frame) {
					break;
				}
				i++;
			}
			if (i == -1) {
				easingStartTime = double.NegativeInfinity;
				easingEndTime = markers[0].Time;
				return;
			}
			if (i == markers.Count - 1) {
				easingStartTime = markers[i].Time;
				easingEndTime = double.PositiveInfinity;
				return;
			}
			var currentMarker = markers[i];
			var nextMarker = markers[i + 1];
			easingStartTime = currentMarker.Time;
			easingEndTime = nextMarker.Time;
			if (!currentMarker.Easing.IsDefault()) {
				var e = currentMarker.Easing;
				easingCurve = new CubicBezier(e.P1X, e.P1Y, e.P2X, e.P2Y);
			}
		}

		public double EaseTime(double time)
		{
#if TANGERINE
			if (!Animation.EasingEnabledChecker?.Invoke(owner) ?? true) {
				return time;
			}
#endif
			if (time == previousTime) {
				return easedPreviousTime;
			}
			if (time == currentTime) {
				return easedCurrentTime;
			}
			if (time < easingStartTime || time >= easingEndTime) {
				CacheEasing(time);
			}
			previousTime = currentTime;
			easedPreviousTime = easedCurrentTime;
			currentTime = time;
			if (easingCurve != null) {
				var d = easingEndTime - easingStartTime;
				var p = (time - easingStartTime) / d;
				var p2 = easingCurve.SolveWithEpsilon(p, 1e-5);
				easedCurrentTime = p2 * d + easingStartTime;
			} else {
				easedCurrentTime = time;
			}
			return easedCurrentTime;
		}
	}
}
