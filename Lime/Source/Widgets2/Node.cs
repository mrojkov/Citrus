using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using System.Linq;
using ProtoBuf;
using System.ComponentModel;
using System.Diagnostics;

namespace Lime.Widgets2
{
	public delegate void UpdateHandler(float delta);

	[ProtoContract]
	[ProtoInclude(101, typeof(Widget))]
	[ProtoInclude(102, typeof(ParticleModifier))]
	[ProtoInclude(103, typeof(ImageCombiner))]
	[ProtoInclude(104, typeof(PointObject))]
	[ProtoInclude(105, typeof(Bone))]
	[ProtoInclude(106, typeof(Audio))]
	[ProtoInclude(107, typeof(SplineGear))]
	[ProtoInclude(108, typeof(TextStyle))]
	[ProtoInclude(109, typeof(LinearLayout))]
	[DebuggerTypeProxy(typeof(NodeDebugView))]
	public class Node : IDisposable
	{
		internal protected bool GlobalValuesValid;

		#region properties
#if WIN
		public Guid Guid { get; set; }
#endif
	
		[ProtoMember(1)]
		public string Id { get; set; }

		[ProtoMember(2)]
		public string ContentsPath { get; set; }

		public Node Parent { get; internal set; }

		public Widget AsWidget { get; internal set; }

		internal Node NextToRender;
		public Node NextSibling;

		public int Layer { get; set; }

		[ProtoMember(6)]
		public NodeList Nodes;

		public Animation Animation;

		[ProtoMember(8)]
		public string Tag { get; set; }

		public object UserData { get; set; }

		public ComponentList Components;

		#endregion
		#region Methods

		public Node()
		{
			Nodes = new NodeList(this);
		}

		#region Animation fallbacks
		public bool TryRunAnimation(string markerId)
		{
			return Animation.TryRun(markerId);
		}

		public void RunAnimation(string markerId)
		{
			Animation.Run(markerId);
		}

		public bool IsRunning { get { return Animation.IsRunning; } set { Animation.IsRunning = value; } }
		public bool IsStopped { get { return Animation.IsStopped; } set { Animation.IsStopped = value; } }
		public string CurrentAnimation { get { return Animation.Current; } set { Animation.Current = value; } }
		public MarkerCollection Markers { get { return Animation.Markers; } }
		public AnimatorCollection Animators { get { return Animation.Animators; } }
		public float AnimationSpeed { get { return Animation.Speed; } set { Animation.Speed = value; } }
		public int AnimationTime { get { return Animation.Time; } set { Animation.Time = value; } }
		public int AnimationFrame { get { return Animation.Frame; } set { Animation.Frame = value; } }
		#endregion

		public virtual void Dispose()
		{
			for (var n = Nodes.FirstOrNull(); n != null; n = n.NextSibling) {
				n.Dispose();
			}
			foreach (var c in Components) {
				c.Dispose();
			}
			Components.Clear();
		}

		internal protected void InvalidateGlobalValues()
		{
			GlobalValuesValid = false;
			for (var n = Nodes.FirstOrNull(); n != null; n = n.NextSibling) {
				if (n.GlobalValuesValid) {
					n.InvalidateGlobalValues();
				}
			}
		}

		protected void RecalcGlobalValues()
		{
			if (Parent != null && !Parent.GlobalValuesValid) {
				Parent.RecalcGlobalValues();
			}
			RecalcGlobalValuesUsingParents();
			GlobalValuesValid = true;
		}

		protected virtual void RecalcGlobalValuesUsingParents() { }

		public Node GetRoot()
		{
			Node node = this;
			while (node.Parent != null) {
				node = node.Parent;
			}
			return node;
		}

		public bool ChildOf(Node node)
		{
			for (Node n = Parent; n != null; n = n.Parent) {
				if (n == node)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Slow but safe deep cloning. This function is based on protobuf-net serialization.
		/// </summary>
		public Node DeepCloneSafe()
		{
			return Serialization.DeepClone<Node>(this);
		}

		/// <summary>
		/// Slow but safe deep cloning. This function is based on protobuf-net serialization.
		/// </summary>
		public T DeepCloneSafe<T>() where T : Node
		{
			return DeepCloneSafe() as T;
		}

		/// <summary>
		/// Use for fast deep cloning of unchangeable objects. This function based on MemberwiseClone().
		/// Animators keys, Markers and SkinningWeights are shared between clone and original.
		/// </summary>
		public virtual Node DeepCloneFast()
		{
			var clone = (Node)MemberwiseClone();
			clone.Parent = null;
			clone.NextSibling = null;
			clone.AsWidget = clone as Widget;
			clone.GlobalValuesValid = false;
			clone.Nodes = Nodes.DeepCloneFast(clone);
			clone.Components = Components.Clone(clone);
			return clone;
		}

		/// <summary>
		/// Use for fast deep cloning of immutable objects. This function based on MemberwiseClone().
		/// Animators keys, Markers and SkinningWeights are shared between clone and original.
		/// </summary>
		public T DeepCloneFast<T>() where T : Node
		{
			return (T)DeepCloneFast();
		}

		public override string ToString()
		{
			return string.Format("{0}, \"{1}\", {2}", GetType().Name, Id ?? "", GetHierarchyPath());
		}

		private string GetHierarchyPath()
		{
			string r = string.IsNullOrEmpty(Id) ? String.Format("[{0}]", GetType().Name) : Id;
			if (!string.IsNullOrEmpty(Tag)) {
				r += string.Format(" ({0})", Tag);
			}
			if (Parent != null) {
				r = Parent.GetHierarchyPath() + "/" + r;
			}
			return r;
		}

		/// <summary>
		/// Unlinks the node from its parent. 
		/// </summary>
		public void Unlink()
		{
			if (Parent != null) {
				Parent.Nodes.Remove(this);
			}
		}

		public void UnlinkAndDispose()
		{
			Unlink();
			Dispose();
		}

		public virtual void Update(float delta)
		{
			//delta *= AnimationSpeed;
			//if (IsRunning) {
			//	AdvanceAnimation(delta);
			//}
			foreach (var c in Components.Behaviours) {
				c.Update(delta);
			}
			SelfUpdate(delta);
			for (var node = Nodes.FirstOrNull(); node != null; ) {
				var next = node.NextSibling;
				node.Update(delta);
				node = next;
			}
			foreach (var c in Components.LateBehaviours) {
				c.Update(delta);
			}
		}
		protected virtual void SelfUpdate(float delta) { }

		protected virtual void SelfLateUpdate(float delta) { }

		public virtual void Render() {}

		public virtual void AddToRenderChain(RenderChain chain)
		{
			for (Node node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				node.AddToRenderChain(chain);
			}
		}

		public void AddNode(Node node)
		{
			Nodes.Add(node);
		}

		public void AddToNode(Node node)
		{
			node.Nodes.Add(this);
		}

		public void PushNode(Node node)
		{
			Nodes.Push(node);
		}

		public void PushToNode(Node node)
		{
			node.Nodes.Push(this);
		}

		public T Find<T>(string id) where T : Node
		{
			T result = TryFindNode(id) as T;
			if (result == null) {
				throw new Lime.Exception("'{0}' of {1} not found for '{2}'", id, typeof(T).Name, ToString());
			}
			return result;
		}

		public T Find<T>(string format, params object[] args) where T : Node
		{
			return Find<T>(string.Format(format, args));
		}

		public bool TryFind<T>(string id, out T node) where T : Node
		{
			node = TryFindNode(id) as T;
			return node != null;
		}

		public T TryFind<T>(string id) where T : Node
		{
			return TryFindNode(id) as T;
		}

		public T TryFind<T>(string format, params object[] args) where T : Node
		{
			return TryFind<T>(string.Format(format, args));
		}

		public Node FindNode(string path)
		{
			var node = TryFindNode(path);
			if (node == null) {
				throw new Lime.Exception("'{0}' not found for '{1}'", path, ToString());
			}
			return node;
		}

		public Node TryFindNode(string path)
		{
			if (path.IndexOf('/') >= 0) {
				return TryFindNodeByPath(path);
			} else {
				return TryFindNodeById(path);
			}
		}

		private Node TryFindNodeByPath(string path)
		{
			Node child = this;
			foreach (string id in path.Split('/')) {
				child = child.TryFindNodeById(id);
				if (child == null)
					break;
			}
			return child;
		}

		public IEnumerable<Node> Descendants
		{
			get
			{
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					yield return node;
					foreach (var i in node.Descendants) {
						yield return i;
					}
				}
			}
		}

		public void RefreshLayout()
		{
			OnParentSizeChanged(Vector2.Zero);
			for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				node.RefreshLayout();
			}
		}

		/// <summary>
		/// Этот метод масштабирует позицию и размер данного нода и всех его потомков.
		/// Метод полезен для адаптирования сцены под экраны с нестандартным DPI.
		/// Если позиция или размер виджета анинимированы, то ключи анимации также подвергаются соответствующему
		/// преобразованию.
		/// Для текстовых виджетов, размер шрифта масштабируется.
		/// </summary>
		/// <param name="roundCoordinates">Если true, то после масштабирования виджета, его размер округляется, а сам виджет сдвигается таким образом,
		/// чтобы его левый верхний угол (точка (0,0) в локальных координатах виджета) перешел в целочисленную позицию.</param>
		public virtual void StaticScale(float ratio, bool roundCoordinates)
		{
			foreach (var node in Nodes) {
				node.StaticScale(ratio, roundCoordinates);
			}
		}

		protected void LayoutChildren(Vector2 sizeDelta)
		{
			for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				node.OnParentSizeChanged(sizeDelta);
			}
		}

		protected virtual void OnParentSizeChanged(Vector2 parentSizeDelta) { }

		[ThreadStatic]
		private static Queue<Node> nodeSearchQueue;

		private Node TryFindNodeById(string id)
		{
			if (nodeSearchQueue == null) {
				nodeSearchQueue = new Queue<Node>();
			}
			var queue = nodeSearchQueue;
			queue.Enqueue(this);
			while (queue.Count > 0) {
				var parent = queue.Dequeue();
				var child = parent.Nodes.FirstOrNull();
				for (; child != null; child = child.NextSibling) {
					if (child.Id == id) {
						queue.Clear();
						return child;
					}
					queue.Enqueue(child);
				}
			}
			return null;
		}

		public void PreloadAssets()
		{
			foreach (var prop in GetType().GetProperties()) {
				if (prop.PropertyType == typeof(ITexture)) {
					PreloadTexture(prop);
				} else if (prop.PropertyType == typeof(SerializableFont)) {
					PreloadFont(prop);
				}
			}
			foreach (var animator in Animators) {
				PreloadAnimatedTextures(animator);
			}
			foreach (var node in Nodes) {
				node.PreloadAssets();
			}
		}

		private static void PreloadAnimatedTextures(IAnimator animator)
		{
			var textureAnimator = animator as Animator<ITexture>;
			if (textureAnimator != null) {
				foreach (var key in textureAnimator.ReadonlyKeys) {
					key.Value.GetHandle();
				}
			}
		}

		private void PreloadFont(PropertyInfo prop)
		{
			var getter = prop.GetGetMethod();
			var font = getter.Invoke(this, new object[]{}) as SerializableFont;
			if (font != null) {
				foreach (var texture in font.Instance.Textures) {
					texture.GetHandle();
				}
			}
		}

		private void PreloadTexture(PropertyInfo prop)
		{
			var getter = prop.GetGetMethod();
			var texture = getter.Invoke(this, new object[]{}) as ITexture;
			if (texture != null) {
				texture.GetHandle();
			}
		}

		protected void LoadContent()
		{
			if (!string.IsNullOrEmpty(ContentsPath)) {
				LoadContentHelper();
			} else {
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.LoadContent();
				}
			}
		}

		private void LoadContentHelper()
		{
			Nodes.Clear();
			Markers.Clear();
			var contentsPath = Path.ChangeExtension(ContentsPath, "scene");
			if (!AssetsBundle.Instance.FileExists(contentsPath)) {
				return;
			}
			var content = new Frame(ContentsPath);
			if (content.AsWidget != null && AsWidget != null) {
				content.Update(0);
				content.AsWidget.Size = AsWidget.Size;
				content.Update(0);
			}
			//Markers.AddRange(content.Markers);
			//var nodes = content.Nodes.ToList();
			//content.Nodes.Clear();
			//Nodes.AddRange(nodes);
		}
		#endregion
	}
}