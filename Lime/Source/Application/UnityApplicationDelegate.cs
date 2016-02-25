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

		protected virtual void Update()
		{
			OnUpdate();
		}

		protected virtual void OnPostRender()
		{
			OnRendering();
		}

		protected virtual void OnApplicationQuit()
		{
			OnDestroy();
		}
	}
}
#endif
