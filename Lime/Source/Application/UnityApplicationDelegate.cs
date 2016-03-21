#if UNITY
using System;
using UnityEngine;
namespace Lime
{
	public class UnityApplicationDelegate : MonoBehaviour
	{
		public static UnityApplicationDelegate Instance {get; private set;}
		public event Action<float> Updating;
		public event Action Rendering;
		public event Action Destroying; 
		public event Action Activated;

		public UnityApplicationDelegate ()
		{
			Instance = this;
		}

		public void OnActivate()
		{
			if (Activated != null) {
				Activated();
			}
		}

		protected virtual void Update()
		{
			if (Updating != null) {
				Updating(UnityEngine.Time.deltaTime);
			}
		}

		protected virtual void OnPostRender()
		{
			if (Rendering != null) {
				Rendering();
			}
		}

		protected virtual void OnApplicationQuit()
		{
			if (Destroying != null) {
				Destroying();
			}
		}
	}
}
#endif
