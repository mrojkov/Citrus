#if UNITY
using System;
namespace Lime
{

	public class UnityApp 
	{
		public static UnityApp Instance {get; private set;}
		public event Action<float> Updating;
		public event Action Rendering;
		public event Action Destroying; 
		public event Action Activated;

		public UnityApp ()
		{
			Instance = this;
		}

		public void OnUpdate()
		{
			if (Updating != null) {
				Updating(UnityEngine.Time.deltaTime);
			}
		}

		public void OnRendering()
		{
			if (Rendering != null) {
				Rendering();
			}
		}

		public void OnDestroy()
		{
			if (Destroying != null) {
				Destroying();
			}
		}

		public void OnActivate()
		{
			if (Activated != null) {
				Activated();
			}
		}
	}
}
#endif
