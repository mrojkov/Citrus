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
	public static class CopyKeyframes
	{
		public static void Perform()
		{
			Clipboard.Text = CopyToString();
		}

		private static string CopyToString()
		{
			var keys = GetKeyframes();
			var stream = new System.IO.MemoryStream();
			Serialization.WriteObject(Document.Current.Path, stream, keys, Serialization.Format.JSON);
			var text = Encoding.UTF8.GetString(stream.ToArray());
			return text;
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

			foreach (var row in Document.Current.TopLevelSelectedRows()) {
				spans = row.Components.Get<GridSpanListComponent>()?.Spans;
				if (spans == null) {
					continue;
				}
				var animable = row.Components.Get<NodeRow>()?.Node as IAnimable;
				if (animable == null) {
					continue;
				}
				foreach (var animator in animable.Animators) {
					foreach (var keyframe in animator.Keys.Where(i => spans.Any(j => j.Contains(i.Frame)))) {
						list.Add(new RowKeyBinding {
							AnimationId = animator.AnimationId,
							Frame = keyframe.Frame - startCol,
							Property = animator.TargetProperty,
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
		public string AnimationId { get; set; }

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
			string data = Clipboard.Text;
			if (String.IsNullOrEmpty(data) || !Document.Current.TopLevelSelectedRows().Any()) {
				return;
			}
			int startRow = Document.Current.TopLevelSelectedRows().First().Index;
			var spans = Document.Current.Rows[startRow].Components.Get<GridSpanListComponent>()?.Spans;
			if (spans == null || !spans.Any()) {
				return;
			}
			int startCol = spans.First().A;
			var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(data));
			List<RowKeyBinding> keys;
			try {
				keys = Serialization.ReadObject<List<RowKeyBinding>>(Document.Current.Path, stream);
			}
			catch (System.Exception) { return; }
			Document.Current.History.DoTransaction(() => {
				foreach (var key in keys) {
					int rowIndex = startRow + key.Row;
					int colIndex = startCol + key.Frame;
					if (rowIndex >= Document.Current.Rows.Count || colIndex < 0) {
						continue;
					}
					var animable = Document.Current.Rows[rowIndex].Components.Get<NodeRow>()?.Node as IAnimable;
					if (animable == null) {
						continue;
					}
					var property = animable.GetType().GetProperty(key.Property);
					if (property == null) {
						continue;
					}
					key.Keyframe.Frame = colIndex;
					SetKeyframe.Perform(animable, key.Property, key.AnimationId, key.Keyframe);
				}
			});
		}
	}

	public static class DeleteKeyframes
	{
		public static void Perform()
		{
			Document.Current.History.DoTransaction(() => {
				foreach (var row in Document.Current.TopLevelSelectedRows()) {
					var spans = row.Components.Get<GridSpanListComponent>()?.Spans;
					if (spans == null) {
						continue;
					}
					var animable = row.Components.Get<NodeRow>()?.Node as IAnimable;
					if (animable == null) {
						continue;
					}
					foreach (var animator in animable.Animators.ToList()) {
						foreach (var keyframe in animator.Keys.Where(i => spans.Any(j => j.Contains(i.Frame))).ToList()) {
							RemoveKeyframe.Perform(animator, keyframe.Frame);
						}
					}
				}
			});
		}
	}
}
