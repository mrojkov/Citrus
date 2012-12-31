using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public class ActionManager
	{
		public static readonly ActionManager Instance = new ActionManager();

		List<Action> actions = new List<Action>();

		public void Initialize()
		{
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			foreach (var type in assembly.GetTypes()) {
				if (type.IsSubclassOf(typeof(Action))) {
					var ctr = type.GetConstructor(new Type[] {});
					if (ctr != null) {
						var action = ctr.Invoke(null) as Action;
						actions.Add(action);
					}
				}
			}
		}
	}
}
