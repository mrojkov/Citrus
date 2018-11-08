using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Tangerine.UI.Timeline.Components;
using Yuzu;

namespace Tangerine.UI.Timeline.Operations
{
	internal static class KeyframeClipboard
	{
		public static List<RowKeyBinding> Keys;
	}

	public static class CopyKeyframes
	{
		public static void Perform()
		{
			KeyframeClipboard.Keys = GetKeyframes();
		}

		private static List<RowKeyBinding> GetKeyframes()
		{
			var list = new List<RowKeyBinding>();

			int startRow = Document.Current.TopLevelSelectedRows().First().Index;
			var spans = Document.Current.Rows[startRow].Components.Get<GridSpanListComponent>()?.Spans;
			if (spans == null || !spans.Any()) {
				return list;
			}
			int startCol = spans.First().A;

			foreach (var row in Document.Current.SelectedRows()) {
				spans = row.Components.Get<GridSpanListComponent>()?.Spans;
				if (spans == null) {
					continue;
				}
				var animable = row.Components.Get<NodeRow>()?.Node as IAnimationHost;
				if (animable == null) {
					continue;
				}
				foreach (var animator in animable.Animators) {
					if (animator.AnimationId != Document.Current.AnimationId) {
						continue;
					}
					foreach (var keyframe in animator.Keys.Where(i => spans.Any(j => j.Contains(i.Frame)))) {
						list.Add(new RowKeyBinding {
							Frame = keyframe.Frame - startCol,
							Property = animator.TargetPropertyPath,
							Row = row.Index - startRow,
							Keyframe = keyframe
						});
					}
				}
			}
			return list;
		}
	}

	internal class RowKeyBinding
	{
		[YuzuMember]
		public int Row { get; set; }

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
				foreach (var key in keys) {
					int rowIndex = startRow + key.Row;
					int colIndex = startCol + key.Frame;
					if (rowIndex >= Document.Current.Rows.Count || colIndex < 0) {
						continue;
					}
					var animationHost = Document.Current.Rows[rowIndex].Components.Get<NodeRow>()?.Node as IAnimationHost;
					if (animationHost == null) {
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
					var animable = row.Components.Get<NodeRow>()?.Node as IAnimationHost;
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
