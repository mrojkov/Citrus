#if OPENGL
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Since Android can revoke the graphics context when the app is going to sleep, 
	/// every OpenGL object must implement this interface and register itself in the GLObjectRegistry
	/// </summary>
	interface IGLObject
	{
		void Discard();
	}

	class GLObjectRegistry
	{
		public static GLObjectRegistry Instance = new GLObjectRegistry();
		private List<WeakReference> items = new List<WeakReference>();

		public void Add(IGLObject item)
		{
#if Android			
			lock (items) {
				foreach (var i in items) {
					if (!i.IsAlive) {
						i.Target = item;
						return;
					}
				}
				items.Add(new WeakReference(item));
			}
#endif
		}

		public void DiscardObjects()
		{
			lock (items) {
				foreach (var i in items) {
					var obj = i.Target as IGLObject;
					if (obj != null) {
						obj.Discard();
					}
				}
			}
		}
	}
}
#endif