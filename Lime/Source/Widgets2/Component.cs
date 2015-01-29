using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime.Widgets2
{
	[ProtoContract]
	public class Component : IDisposable
	{
		public Node Owner;

		[ProtoMember(1)]
		public AnimatorCollection Animators;

		internal protected virtual void OnTrigger(string propertyName) { }

		public virtual void Dispose() { }

		public virtual Component Clone()
		{
			var clone = (Component)MemberwiseClone();
			clone.Owner = null;
			clone.Animators = AnimatorCollection.SharedClone(clone, Animators);
			return clone;
		}
	}

	[ProtoContract]
	public class Behaviour : Component
	{
		public virtual bool IsLate { get { return false; } }
		public virtual void Update(float delta) { }
	}

	[ProtoContract]
	public class Drawable : Component
	{
		public Drawable NextToDraw;

		public virtual void AddToRenderChain(RenderChain chain)
		{
			throw new NotImplementedException();
		}

		public virtual void Draw() { }
	}
}
