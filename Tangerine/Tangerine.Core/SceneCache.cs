using System;
using System.Collections.Generic;
using System.Threading;
using Lime;
using Yuzu = Lime.Yuzu;

namespace Tangerine.Core
{
	public class SceneCache
	{
		private class CacheEntry
		{
			public Node Node;
			public HashSet<string> Dependencies = new HashSet<string>();
			public bool IsNeedReloadExternalScenes { get; set; }
			public Func<Node> NodeProvider { get; set; }

			public void RefreshDependencies()
			{
				Dependencies.Clear();
				var node = Node ?? NodeProvider?.Invoke();
				if (node == null) {
					return;
				}
				foreach (var descendant in node.Descendants) {
					string contentsPath = descendant.ContentsPath;
					if (!contentsPath.IsNullOrWhiteSpace() && Node.ResolveScenePath(contentsPath) != null) {
						Dependencies.Add(contentsPath);
					}
				}
			}
		}

		public SceneCache()
		{
			Node.SceneLoading = new ThreadLocal<Node.SceneLoadingDelegate>(() => SceneCache_SceneLoading);
			Node.SceneLoaded = new ThreadLocal<Node.SceneLoadedDelegate>(() => SceneCache_SceneLoaded);
		}

		private readonly Dictionary<string, CacheEntry> contentPathToCacheEntry = new Dictionary<string, CacheEntry>();

		private bool SceneCache_SceneLoading(string path, ref Node instance, bool external)
		{
			Console.WriteLine($"Loading: {path}, external: {external}");
			if (!external) {
				return false;
			}
			if (!contentPathToCacheEntry.TryGetValue(path, out var t)) {
				contentPathToCacheEntry.Add(path, new CacheEntry());
				return false;
			} else if (t.Node == null) {
				return false;
			}

			if (t.NodeProvider != null) {
				t.RefreshDependencies();
				instance = t.NodeProvider().Clone();
				return true;
			}
			if (t.IsNeedReloadExternalScenes) {
				t.IsNeedReloadExternalScenes = false;
				t.Node.LoadExternalScenes(TangerineYuzu.Instance.Value);
			}
			instance = t.Node.Clone();
			return true;
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
				t.Node = instance.Clone();
				t.RefreshDependencies();
			} else {
				return;
			}
		}

		public void InvalidateEntry(string path, Func<Node> nodeProviderFunc)
		{
			var t = GetCacheEntrySafe(path);
			if (nodeProviderFunc == null) {
				t.Node = null;
				t.NodeProvider = null;
			} else {
				t.NodeProvider = nodeProviderFunc;
			}
			t.Dependencies.Clear();
			foreach (var kv in contentPathToCacheEntry) {
				if (kv.Value.Dependencies.Contains(path)) {
					// InvalidateEntry(kv.Key);
					// kv.Value.Node.LoadExternalScenes(TangerineYuzu.Instance.Value); // file access problems use lazy access
					kv.Value.IsNeedReloadExternalScenes = true;
				}
			}
		}

		public void RemoveNodeProvider(string path)
		{
			var t = GetCacheEntrySafe(path);
			t.NodeProvider = null;
			t.RefreshDependencies();
			foreach (var kv in contentPathToCacheEntry) {
				if (kv.Value.Dependencies.Contains(path)) {
					// InvalidateEntry(kv.Key);
					// kv.Value.Node.LoadExternalScenes(TangerineYuzu.Instance.Value); // file access problems use lazy
					kv.Value.IsNeedReloadExternalScenes = true;
				}
			}
		}

		private CacheEntry GetCacheEntrySafe(string path)
		{
			if (!contentPathToCacheEntry.TryGetValue(path, out CacheEntry t)) {
				contentPathToCacheEntry.Add(path, t = new CacheEntry());
			}
			return t;
		}
	}
}
