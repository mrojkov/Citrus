using System;
using System.Collections.Generic;
using System.Threading;
using Lime;
using Orange;
using Exception = Lime.Exception;

namespace Tangerine.Core
{
	// We'll just call node.LoadExternalScenes each time tab is activated and make sure our cache contains right versions of external scenes.
	// Further optimization will include loading only changed external scenes, not all of them.
	public class SceneCache
	{
		private class CacheEntry
		{
			public Node Node
			{
				get
				{
					var r = nodeProvider?.Invoke() ?? node;
					if (r != null && DoNeedReloadExternalScenes) {
						DoNeedReloadExternalScenes = false;
						r.LoadExternalScenes(TangerineYuzu.Instance.Value);
					}
					return r;
				}
			}
			private Func<Node> nodeProvider;
			private Node node;
			public HashSet<string> Dependencies
			{
				get
				{
					if (!areDependenciesValid) {
						RefreshDependencies();
					}
					return dependencies;
				}
			}

			private readonly HashSet<string> dependencies = new HashSet<string>();
			bool areDependenciesValid = false;
			public bool DoNeedReloadExternalScenes { get; set; }
			public void SetNodeProvider(Func<Node> nodeProvider)
			{
				areDependenciesValid = false;
				this.nodeProvider = nodeProvider;
			}
			public void SetNode(Node node)
			{
				areDependenciesValid = nodeProvider != null;
				this.node = node;
			}

			private void RefreshDependencies()
			{
				dependencies.Clear();
				if (Node == null) {
					return;
				}
				foreach (var descendant in Node.Descendants) {
					string contentsPath = descendant.ContentsPath;
					if (!contentsPath.IsNullOrWhiteSpace() && Node.ResolveScenePath(contentsPath) != null) {
						dependencies.Add(contentsPath);
					}
				}
				areDependenciesValid = true;
			}
		}

		public SceneCache()
		{
			Node.SceneLoading = new ThreadLocal<Node.SceneLoadingDelegate>(() => SceneCache_SceneLoading);
			Node.SceneLoaded = new ThreadLocal<Node.SceneLoadedDelegate>(() => SceneCache_SceneLoaded);
		}

		private readonly Dictionary<string, CacheEntry> contentPathToCacheEntry = new Dictionary<string, CacheEntry>();

		private bool SceneCache_SceneLoading(string path, ref Node instance, bool external, bool ignoreExternals)
		{
			Console.WriteLine($"Loading: {path}, external: {external}");
			if (!external) {
				HasGitConflicts();
				return false;
			}
			if (!contentPathToCacheEntry.TryGetValue(path, out var t)) {
				HasGitConflicts();
				contentPathToCacheEntry.Add(path, new CacheEntry());
				return false;
			}
			if (t.Node == null) {
				HasGitConflicts();
				return false;
			}
			instance = t.Node.Clone();
			Document.Current?.Decorate(instance);
			return true;
			void HasGitConflicts()
			{
				var pathWithExtension = Node.ResolveScenePath(path);
				if (pathWithExtension == null) {
					return;
				}
				if (Git.HasGitConflicts(AssetPath.Combine(Project.Current.AssetsDirectory, pathWithExtension))) {
					throw new InvalidOperationException($"{pathWithExtension} has git conflicts.");
				}
			}
		}

		private void SceneCache_SceneLoaded(string path, Node instance, bool external)
		{
			Console.WriteLine($"Loaded: {path}, external: {external}");
			if (!external) {
				return;
			}
			if (!contentPathToCacheEntry.TryGetValue(path, out var t)) {
				throw new InvalidOperationException();
			}
			if (t.Node == null) {
				t.SetNode(instance.Clone());
			} else {
				return;
			}
		}

		public void InvalidateEntryFromFilesystem(string path)
		{
			var t = GetCacheEntrySafe(path);
			t.SetNode(null);
			MarkDependentsForReload(path);
		}

		public void InvalidateEntryFromOpenedDocumentChanged(string path, Func<Node> nodeProviderFunc)
		{
			var t = GetCacheEntrySafe(path);
			t.SetNodeProvider(nodeProviderFunc);
			MarkDependentsForReload(path);
		}

		private CacheEntry GetCacheEntrySafe(string path)
		{
			if (!contentPathToCacheEntry.TryGetValue(path, out CacheEntry t)) {
				contentPathToCacheEntry.Add(path, t = new CacheEntry());
			}
			return t;
		}

		private void MarkDependentsForReload(string path)
		{
			var q = new Queue<string>();
			q.Enqueue(path);
			while (q.Count != 0) {
				var nextPath = q.Dequeue();
				foreach (var kv in contentPathToCacheEntry) {
					if (kv.Key == path) {
						continue;
					}
					if (kv.Value.Dependencies.Contains(nextPath)) {
						if (nextPath == kv.Key) {
							throw new Exception($"Scene '{nextPath}' has cyclic dependencies");
						}
						kv.Value.DoNeedReloadExternalScenes = true;
						q.Enqueue(kv.Key);
					}
				}
			}
		}

		public void Clear(string docPath)
		{
			if (contentPathToCacheEntry.TryGetValue(docPath, out CacheEntry t)) {
				contentPathToCacheEntry.Remove(docPath);
				foreach (var d in t.Dependencies) {
					Clear(d);
				}
			}
		}

		public void CheckCyclicDependencies(string path)
		{
			MarkDependentsForReload(path);
		}
	}
}
