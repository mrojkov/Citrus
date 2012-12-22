using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	/// <summary>
	/// Этот класс задает геометрию отображения ключа на таймлайне
	/// </summary>
	public class KeyTransient
	{
		public Lime.TangerinePropertyAttribute Attribute;
		public QColor QColor;
		public int Line;
		public int Length;
		public int Frame;
	}

	/// <summary>
	/// Этот класс позволяет пробежаться по всем ключам данного нода и возвращает список транзиентов
	/// </summary>
	public class KeyTransientCollector
	{
		public static List<KeyTransient> GetTransients(Lime.Node node)
		{
			var transients = GatherTransientsFromAnimators(node);
			transients.Sort((a, b) => a.Frame - b.Frame);
			AllocateLines(transients);
			return transients;
		}

		private static void AllocateLines(List<KeyTransient> transients)
		{
			var attributes = new Lime.TangerinePropertyAttribute[KeyGrid.MaxLinesPerRow];
			var endings = new int[KeyGrid.MaxLinesPerRow];
			int[] priorityMap = GenerateLinesPriorityMap(KeyGrid.MaxLinesPerRow);
			foreach (var transient in transients) {
				if (!TryJoinTransientToPreviousOne(attributes, endings, priorityMap, transient)) {
					PutTransientToFreeLine(attributes, endings, priorityMap, transient);
				}
			}
		}

		private static void PutTransientToFreeLine(Lime.TangerinePropertyAttribute[] attributes, int[] endings, int[] priorityMap, KeyTransient transient)
		{
			foreach (int j in priorityMap) {
				if (endings[j] < transient.Frame) {
					endings[j] = 0;
				}
				if (endings[j] == 0) {
					endings[j] = transient.Frame + transient.Length - 1;
					transient.Line = j;
					attributes[j] = transient.Attribute;
					break;
				}
			}
		}

		private static bool TryJoinTransientToPreviousOne(Lime.TangerinePropertyAttribute[] attributes, int[] endings, int[] priorityMap, KeyTransient transient)
		{
			foreach (int j in priorityMap) {
				if (endings[j] == transient.Frame - 1) {
					if (transient.Attribute == attributes[j]) {
						transient.Line = j;
						endings[j] = transient.Frame + transient.Length - 1;
						return true;
					}
				}
			}
			return false;
		}

		private static int[] GenerateLinesPriorityMap(int maxLines)
		{
			int[] linesPriorityMap = new int[maxLines];
			for (int i = 0; i < KeyGrid.MaxLinesPerRow; i++) {
				int s = (i % 2) == 0 ? 1 : -1;
				linesPriorityMap[i] = KeyGrid.MaxLinesPerRow / 2 + (i + 1) * s / 2;
			}
			return linesPriorityMap;
		}

		private static List<KeyTransient> GatherTransientsFromAnimators(Lime.Node node)
		{
			var transients = new List<KeyTransient>();
			foreach (var ani in node.Animators) {
				var attr = GetAnimatorAttributes(node, ani);
				for (int i = 0; i < ani.Frames.Length; i++) {
					var key0 = ani[i];
					var transient = new KeyTransient() {
						Line = 0,
						Attribute = attr,
						QColor = KeyTransientPalette.GetColor(attr.KeyColor),
						Frame = key0.Frame
					};
					if (i < ani.Frames.Length - 1) {
						var key1 = ani[i + 1];
						if (!key0.Value.Equals(key1.Value)) {
							transient.Length = key1.Frame - transient.Frame;
						}
					}
					transients.Add(transient);
				}
			}
			return transients;
		}

		private static Lime.TangerinePropertyAttribute GetAnimatorAttributes(Lime.Node node, Lime.Animator ani)
		{
			var prop = node.GetType().GetProperty(ani.TargetProperty);
			var attrs = prop.GetCustomAttributes(typeof(Lime.TangerinePropertyAttribute), false);
			if (attrs.Length == 0) {
				return Lime.TangerinePropertyAttribute.Null;
			}
			var attr = attrs[0] as Lime.TangerinePropertyAttribute;
			return attr;
		}
	}

}
