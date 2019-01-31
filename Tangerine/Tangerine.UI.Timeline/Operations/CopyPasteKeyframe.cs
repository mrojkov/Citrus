using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Tangerine.UI.Timeline.Components;
using Yuzu;

namespace Tangerine.UI.Timeline.Operations
{
	internal static class KeyframeClipboard
	{
		public static List<AnimationHostKeyBinding> Keys;
	}

	public static class CopyKeyframes
	{
		public static void Perform()
		{
			KeyframeClipboard.Keys = GetKeyframes();
		}

		private static List<AnimationHostKeyBinding> GetKeyframes()
		{
			var list = new List<AnimationHostKeyBinding>();

			int startRow = Document.Current.TopLevelSelectedRows().First().Index;
			var spans = Document.Current.Rows[startRow].Components.Get<GridSpanListComponent>()?.Spans;
			if (spans == null || !spans.Any()) {
				return list;
			}
			int startCol = spans.First().A;
			int animationHostIndex = -1;

			foreach (var row in Document.Current.SelectedRows()) {
				spans = row.Components.Get<GridSpanListComponent>()?.Spans;
				if (spans == null) {
					continue;
				}
				var animable = row.Components.Get<NodeRow>()?.Node as IAnimationHost;
				if (animable == null) {
					continue;
				}
				animationHostIndex++;

				foreach (var animator in animable.Animators) {
					if (animator.AnimationId != Document.Current.AnimationId) {
						continue;
					}
					foreach (var keyframe in animator.Keys.Where(i => spans.Any(j => j.Contains(i.Frame)))) {
						list.Add(new AnimationHostKeyBinding {
							Frame = keyframe.Frame - startCol,
							Property = animator.TargetPropertyPath,
							AnimationHostOrderIndex = animationHostIndex,
							Keyframe = keyframe
						});
					}
				}
			}
			return list;
		}
	}

	internal class AnimationHostKeyBinding
	{
		[YuzuMember]
		public int AnimationHostOrderIndex { get; set; }

		[YuzuMember]
		public int Frame { get; set; }

		[YuzuMember]
		public string Property { get; set; }

		[YuzuMember]
		public IKeyframe Keyframe { get; set; }
	}

	public static class CutKeyframes
	{
		public static void Perform()
		{
			CopyKeyframes.Perform();
			DeleteKeyframes.Perform();
		}
	}

	public static class PasteKeyframes
	{
		public static void Perform()
		{
			var keys = KeyframeClipboard.Keys;
			if (keys == null || !Document.Current.TopLevelSelectedRows().Any()) {
				return;
			}
			int startRow = Document.Current.TopLevelSelectedRows().First().Index;
			var spans = Document.Current.Rows[startRow].Components.Get<GridSpanListComponent>()?.Spans;
			if (spans == null || !spans.Any()) {
				return;
			}
			int startCol = spans.First().A;
			Document.Current.History.DoTransaction(() => {
				var rows = Document.Current.Rows;
				int rowIndex = startRow;
				int animationHostIndex = 0;
				IAnimationHost animationHost = null;
				Node node = null;

				foreach (var key in keys) {
					int colIndex = startCol + key.Frame;
					if (rowIndex >= Document.Current.Rows.Count || colIndex < 0) {
						continue;
					}
					while (rowIndex < rows.Count) {
						node = rows[rowIndex].Components.Get<NodeRow>()?.Node;
						animationHost = node;
						if (animationHost != null) {
							if (animationHostIndex == key.AnimationHostOrderIndex) {
								break;
							}
							animationHostIndex++;
						}
						++rowIndex;
					}
					if (rowIndex >= rows.Count) {
						break;
					}
					if (node.EditorState().Locked) {
						continue;
					}
					var (pd, _, _) = AnimationUtils.GetPropertyByPath(animationHost, key.Property);
					if (pd.Info == null) {
						continue;
					}
					var keyframe = key.Keyframe.Clone();
					keyframe.Frame = colIndex;
					SetKeyframe.Perform(animationHost, key.Property, Document.Current.AnimationId, keyframe);
				}
			});
		}
	}

	public static class DeleteKeyframes
	{
		public static void Perform()
		{
			Document.Current.History.DoTransaction(() => {
				foreach (var row in Document.Current.SelectedRows()) {
					var spans = row.Components.Get<GridSpanListComponent>()?.Spans;
					if (spans == null) {
						continue;
					}
					var node = row.Components.Get<NodeRow>()?.Node;
					if (node.EditorState().Locked) {
						continue;
					}
					var animable = (IAnimationHost) node;
					if (animable == null) {
						continue;
					}
					foreach (var animator in animable.Animators.ToList()) {
						if (animator.AnimationId != Document.Current.AnimationId) {
							continue;
						}
						foreach (var keyframe in animator.Keys.Where(i => spans.Any(j => j.Contains(i.Frame))).ToList()) {
							RemoveKeyframe.Perform(animator, keyframe.Frame);
						}
					}
				}
			});
		}
	}
}
