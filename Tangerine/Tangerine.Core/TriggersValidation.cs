using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core.Operations;

namespace Tangerine.Core
{
	public class TriggersValidatorOnSetProperty : TriggersValidatorProcessor<SetProperty>
	{
		protected override void InternalRedo(SetProperty op)
		{
			string refinedTrigger;
			if (!TryRefineTrigger(op.Property.Name, op.Obj, op.Value, out refinedTrigger)) {
				return;
			}

			op.Property.SetValue(op.Obj, refinedTrigger, null);
		}

		protected override void InternalUndo(SetProperty op) { }

	}

	public class TriggersValidatorOnSetKeyframe : TriggersValidatorProcessor<SetKeyframe>
	{

		private class Backup
		{
			internal readonly object KeyframeValue;

			public Backup(object keyframeValue)
			{
				KeyframeValue = keyframeValue;
			}
		}

		protected override void InternalRedo(SetKeyframe op)
		{
			string refinedTrigger;
			if (!TryRefineTrigger(op.PropertyName, op.Animable, op.Keyframe.Value, out refinedTrigger)) {
				return;
			}

			op.Save(new Backup(op.Keyframe.Value));
			op.Keyframe.Value = refinedTrigger;
		}

		protected override void InternalUndo(SetKeyframe op)
		{
			// ReSharper disable once NotAccessedVariable
			Backup backup;
			if (!op.Find(out backup)) {
				return;
			}
			op.Keyframe.Value = op.Restore<Backup>().KeyframeValue;
		}

	}

	public abstract class TriggersValidatorProcessor<T> : OperationProcessor<T> where T : IOperation
	{

		protected bool TryRefineTrigger(string propertyName, object container, object triggerValue,
			out string refinedTriggerValue)
		{
			refinedTriggerValue = null;

			if (propertyName != nameof(Node.Trigger) || !(container is Node) || triggerValue == null) {
				return false;
			}

			Node node = (Node) container;

			TriggersValidation.Trigger trigger = TriggersValidation.Trigger.TryParse((string) triggerValue);
			if (trigger.Elements.RemoveAll(element => node.Markers.All(marker => marker.Id != element.MarkerId)) == 0) {
				return false;
			}

			refinedTriggerValue = trigger.Compose();

			return true;
		}

	}

	public static class TriggersValidation
	{
		public class Trigger
		{

			public class Element
			{
				public string MarkerId;
				public string AnimationId;

				public Element(string markerId, string animationId)
				{
					MarkerId = markerId;
					AnimationId = animationId;
				}
			}

			public readonly List<Element> Elements = new List<Element>();

			private Trigger(IEnumerable<Element> elements)
			{
				Elements.AddRange(elements);
			}

			public string Compose()
			{
				if (Elements.Count == 0) return null;
				return string.Join(",", Elements.Select(el => {
					if (el.AnimationId != null) {
						return el.MarkerId + "@" + el.AnimationId;
					}
					return el.MarkerId;
				}));
			}

			public static Trigger TryParse(string value)
			{
				if (value == null) return null;
				if (value.Length == 0) return new Trigger(Enumerable.Empty<Element>());

				return new Trigger(
					value.Split(',').
						Select(el => {
							string elTrimmed = el.Trim();
							string[] arr = elTrimmed.Split(new char[] {'@'}, 2);
							return new Element(arr[0], arr.Length > 1 ? arr[1] : null);
						})
				);
			}

		}
	}

}
